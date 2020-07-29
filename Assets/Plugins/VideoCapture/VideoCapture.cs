/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Simulator.Plugins
{
    public class VideoCapture
    {
        int Id = -1;
        float TimeTillNextCapture;
        static IntPtr RenderEvent;

        string Ffmpeg;
        byte[] Buffer = null;
        Stream Pipe;
        Process Subprocess;
        Queue<AsyncGPUReadbackRequest> ReadbackQueue = new Queue<AsyncGPUReadbackRequest>();

        delegate void LogDelegate(string message);
        static LogDelegate Log = DebugLog;

        static void DebugLog(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        [DllImport("VideoCapture", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static extern int VideoCapture_Init(string Ffmpeg, LogDelegate log);

        [DllImport("VideoCapture", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static extern int VideoCapture_Start(int width, int height, int framerate, int bitrate, int maxBitrate, int quality, int streaming, string destination);

        [DllImport("VideoCapture", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static extern void VideoCapture_Reset(int id, IntPtr texture);

        [DllImport("VideoCapture", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static extern void VideoCapture_Stop(int id);

        [DllImport("VideoCapture", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static extern IntPtr VideoCapture_GetRenderEventFunc();

        public bool Init()
        {
            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows)
            {
                if (Application.isEditor)
                {
                    Ffmpeg = Path.Combine(Application.dataPath, "Plugins", "VideoCapture", "ffmpeg", "windows", "ffmpeg.exe");
                }
                else
                {
                    Ffmpeg = Path.Combine(Application.dataPath, "Plugins", "ffmpeg.exe");
                }
            }
            else
            {
                if (Application.isEditor)
                {
                    Ffmpeg = Path.Combine(Application.dataPath, "Plugins", "VideoCapture", "ffmpeg", "linux", "ffmpeg");
                }
                else
                {
                    Ffmpeg = Path.Combine(Application.dataPath, "Plugins", "ffmpeg");
                }
            }

            if (!File.Exists(Ffmpeg))
            {
                UnityEngine.Debug.LogWarning($"Cannot find ffmpeg at '{Ffmpeg}' location.");
                return false;
            }

            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows)
            {
                if (VideoCapture_Init(Ffmpeg, Log) == 0)
                {
                    RenderEvent = VideoCapture_GetRenderEventFunc();
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public bool Start(RenderTexture texture, int width, int height, int framerate, int bitrate, int maxBitrate, int quality, int streaming, string destination)
        {
            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows)
            {
                if (Id == -1 && texture != null)
                {
                    Id = VideoCapture_Start(width, height, framerate, bitrate, maxBitrate, quality, streaming, destination);
                    VideoCapture_Reset(Id, texture.GetNativeTexturePtr());
                }

                return Id != -1;
            }
            else
            {
                string args = $"-y -loglevel error"
                    + $" -f rawvideo -vcodec rawvideo -pixel_format rgba"
                    + $" -video_size {width}x{height}"
                    + $" -framerate {framerate}"
                    + $" -i -"
                    + $" -vf vflip"
                    + $" -c:v h264_nvenc -pix_fmt yuv420p"
                    + $" -b:v {bitrate * 1024} -maxrate:v {maxBitrate * 1024}"
                    + $" -g {framerate * 2} -profile:v high"
                    + $" -rc vbr_hq -cq {quality}"
                    + $" -f mp4"
                    + $" \"{destination}\"";

                var process = new Process()
                {
                    StartInfo =
                    {
                        FileName = Ffmpeg,
                        Arguments = args,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardInput = true,
                        RedirectStandardError = true,
                    },
                };

                process.EnableRaisingEvents = true;
                process.Exited += OnProcessExited;
                process.ErrorDataReceived += OnErrorDataReceived;

                try
                {
                    process.Start();
                    process.BeginErrorReadLine();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    return false;
                }

                Subprocess = process;
                Pipe = Subprocess.StandardInput.BaseStream;

                return true;
            }
        }

        private void OnProcessExited(object sender, EventArgs evt)
        {
            var process = sender as Process;
            if (process != null && process.ExitCode != 0)
            {
                UnityEngine.Debug.LogWarning($"FFmpeg exited with exit code: {process.ExitCode}");
            }
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs err)
        {
            if (!String.IsNullOrEmpty(err.Data))
            {
                UnityEngine.Debug.LogWarning($"FFmpeg error: {err.Data}");
            }
        }

        public void Reset(RenderTexture texture)
        {
            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows)
            {
                if (Id != -1 && texture != null)
                {
                    VideoCapture_Reset(Id, texture.GetNativeTexturePtr());
                }
            }
        }

        public void Stop()
        {
            if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows)
            {
                if (Id != -1)
                {
                    VideoCapture_Stop(Id);
                    Id = -1;
                }
            }
            else
            {
                ProcessQueue();
                ReadbackQueue.Clear();

                if (Pipe != null)
                {
                    Pipe.Close();
                    Pipe = null;
                }

                if (Subprocess != null)
                {
                    Subprocess.WaitForExit();
                    Subprocess.Close();
                    Subprocess.Dispose();
                    Subprocess = null;
                }
            }
        }

        public IEnumerator OnRecord(RenderTexture texture, int width, int height, int framerate, Action callback = null)
        {
            while (true)
            {
                if (SystemInfo.operatingSystemFamily != OperatingSystemFamily.Windows)
                {
                    if (!ProcessQueue())
                    {
                        if (callback != null)
                        {
                            callback();
                        }

                        yield break;
                    }
                }

                yield return new WaitForEndOfFrame();

                if (Time.timeScale != 0 && texture.IsCreated())
                {
                    if (TimeTillNextCapture <= 0)
                    {
                        if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows)
                        {
                            if (Id != -1 && RenderEvent != IntPtr.Zero)
                            {
                                GL.IssuePluginEvent(RenderEvent, Id);
                            }
                        }
                        else
                        {
                            if (ReadbackQueue.Count < 8)
                            {
                                ReadbackQueue.Enqueue(AsyncGPUReadback.Request(texture));
                            }
                            else
                            {
                                UnityEngine.Debug.LogWarning("Too many GPU readback requests.");
                            }
                        }

                        TimeTillNextCapture += 1.0f / framerate;
                    }

                    TimeTillNextCapture -= Time.unscaledDeltaTime;
                }
            }
        }

        bool ProcessQueue()
        {
            while (ReadbackQueue.Count > 0)
            {
                var req = ReadbackQueue.Peek();

                if (req.hasError)
                {
                    UnityEngine.Debug.LogWarning("GPU readback error detected.");
                    ReadbackQueue.Dequeue();
                }
                else if (req.done)
                {
                    var data = req.GetData<byte>();
                    if (Buffer == null || Buffer.Length != data.Length)
                    {
                        Buffer = data.ToArray();
                    }
                    else
                    {
                        data.CopyTo(Buffer);
                    }

                    try
                    {
                        Pipe.Write(Buffer, 0, Buffer.Length);
                        Pipe.Flush();
                    }
                    catch
                    {
                        return false;
                    }

                    ReadbackQueue.Dequeue();
                }
                else
                {
                    break;
                }
            }

            return true;
        }
    }
}
