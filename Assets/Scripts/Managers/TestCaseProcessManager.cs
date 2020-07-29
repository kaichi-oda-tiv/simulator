/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using UnityEngine;
using SimpleJSON;

using System;

using System.Collections;
using System.Collections.Generic;

using System.Diagnostics;
using System.IO;
using System.Text;


namespace Simulator
{
    public class TestCaseProcess : Process
    {

        private StringBuilder outputData = new StringBuilder();
        private StringBuilder errorData = new StringBuilder();

        public String GetOutputData()
        {
            return outputData.ToString();
        }

        public bool HasErrorData()
        {
            return (errorData.Length > 0);
        }

        public String GetErrorData()
        {
            return errorData.ToString();
        }

        public TestCaseProcess(string executibleName, string configFilename, IDictionary<string, string> environment)
        {
            StartInfo.FileName = executibleName;
            StartInfo.Arguments = $"--config \"{configFilename}\"";
            StartInfo.UseShellExecute = false;
            StartInfo.RedirectStandardOutput = true;
            StartInfo.RedirectStandardInput = true;
            StartInfo.RedirectStandardError = true;

            if (environment != null)
            {
                foreach (var envvar in environment)
                {
                    StartInfo.EnvironmentVariables.Add(envvar.Key.ToString(), envvar.Value.ToString());
                }
            }

            EnableRaisingEvents = true;
            OutputDataReceived += new DataReceivedEventHandler( DataReceived );
            ErrorDataReceived += new DataReceivedEventHandler( ErrorReceived );
        }

        void SendSignal(int signal)
        {
            var kill_process = new Process();

            using (Process proc = new Process())
            {
                Console.WriteLine("[PROC][{0}] Sending signal {1}", Id, signal);
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.FileName = "kill";
                proc.StartInfo.Arguments = $"--signal {signal} {Id}";
                proc.Start();

                proc.WaitForExit();
                Console.WriteLine("[PROC][{0}] Send signal done with status {1}", Id, proc.ExitCode);
            }
        }

        public void Start()
        {
            base.Start();
            BeginOutputReadLine();
            BeginErrorReadLine();
        }

        void DataReceived( object sender, DataReceivedEventArgs eventArgs )
        {
            var proc = (Process)sender;
            // UnityEngine.Debug.Log($"[DataReceived] from:{sender}");
            Console.WriteLine("[PROC][{0}][OUT] {1}", proc.Id, eventArgs.Data);

            if (outputData != null)
            {
                outputData.Append(eventArgs.Data);
            }
        }
        
        void ErrorReceived( object sender, DataReceivedEventArgs eventArgs )
        {
            var proc = (Process)sender;
            Console.WriteLine("[PROC][{0}][ERR] {1}", proc.Id, eventArgs.Data);

            if (errorData != null)
            {
                errorData.Append(eventArgs.Data);
            }
        }

        public void Terminate(int timeout)
        {
            try
            {
                if (HasExited)
                {
                    Console.WriteLine($"[PROC][{Id}] Process is already exited");
                    return;
                }

                Console.WriteLine($"[PROC][{Id}] Sending SIGINT");
                SendSignal(2);

                WaitForExit(timeout);

                if (!HasExited)
                {
                    Console.WriteLine($"[PROC][{Id}] Sending SIGKILL");
                    SendSignal(9);
                    WaitForExit(timeout);
                }

                if (!HasExited)
                {
                    Console.WriteLine($"[PROC][{Id}] Shoot process to the head");
                    Kill();
                }
            }
            catch (InvalidOperationException e)
            {
                UnityEngine.Debug.LogError($"[PROC][{Id}] Failed to terminate process: {e.Message}");
            }
        }
    }

    public struct TestCaseFinishedArgs
    {
        public int ExitCode;
        public string OutputData;
        public string ErrorData;

        public bool Failed {get {return ExitCode != 0;} }

        public TestCaseFinishedArgs(int exitCode, string outputData, string errorData)
        {
            ExitCode = exitCode;
            OutputData = outputData;
            ErrorData = errorData;
        }

        public string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendFormat("TestCaseFinishedArgs(ExitCode={0}, output: {1} bytes error: {2} bytes)",
                                 ExitCode,
                                 OutputData.Length,
                                 ErrorData.Length);

            return builder.ToString();
        }
    }

    public class TestCaseProcessManager : MonoBehaviour
    {
        enum ErrorCodes : int
        {
            ProcessStartError = 127,
        }

        public delegate void Finshed(TestCaseFinishedArgs args);
        public event Finshed OnFinished;

        public string Root;

        public void Init()
        {
            Root = Path.Combine(Application.dataPath, "..");
        }

        public void Start()
        {
            Init();
        }

        TestCaseProcess Process;
        public bool Spawn(string runtimeType, string testCaseName, string testCaseBridge)
        {
            UnityEngine.Debug.Log($"[PROC][main] Prepare external test case type:{runtimeType}");

            var environment = CreateEnvironment(runtimeType, testCaseBridge);
            environment.Add("SIMULATOR_TC_FILENAME", testCaseName);

            var config = CreateTestCaseConfig(runtimeType, testCaseName, environment);

            // Write config to file
            var configFileName = Path.Combine(Path.GetTempPath(), "config-"+Path.GetRandomFileName()+".json");
            Console.WriteLine("[PROC][main] Writing JSON Config to {0}", configFileName);

            using(var stream = new StreamWriter(configFileName))
            {
                stream.Write(config.ToString(2));
            }

            var testCaseRunner = Environment.GetEnvironmentVariable("SIMULATOR_TC_RUNNER");

            if (testCaseRunner == null)
            {
                testCaseRunner = Path.Combine(Root, "TestCaseRunner", runtimeType, "run");
            }

            Process = StartProcess(testCaseRunner, configFileName, environment);

            return Process != null;
        }

        private Dictionary<string,string> CreateEnvironment(string runtimeType, string testCaseBridge)
        {
            var environment = new Dictionary<string,string>();

            string[] bridge = testCaseBridge.Split(':');
            if (bridge.Length == 2) {
                environment.Add("BRIDGE_HOST", bridge[0]);
                environment.Add("BRIDGE_PORT", bridge[1]);
            } else if (bridge.Length == 1) {
                environment.Add("BRIDGE_HOST", bridge[1]);
            }

            environment.Add("SIMULATOR_HOST", "127.0.0.1");
            environment.Add("SIMULATOR_PORT", "8181");
            environment.Add("SIMULATOR_TC_RUNTIME", runtimeType);

            return environment;
        }

        TestCaseProcess StartProcess(string testCaseRunner, string configFilename, IDictionary<string,string> environment = null)
        {
            try
            {
                var proc = new TestCaseProcess(testCaseRunner, configFilename, environment);

                // Event handlers
                proc.Exited += new EventHandler(ProcessExited);
                proc.Exited += new EventHandler((object sender, System.EventArgs e) => {
                    File.Delete(configFilename);
                });

                proc.Start();

                UnityEngine.Debug.Log($"[PROC][main] Successfully launched app Id={proc.Id}");
                
                return proc;
            }
            catch( Exception e )
            {
                UnityEngine.Debug.LogError("Unable to launch app: " + e.Message);
            }

            return null;
        }

        void ProcessExited(object sender, System.EventArgs e)
        {
            var proc = (TestCaseProcess)sender;
            UnityEngine.Debug.Log($"[PROC][main] Process #{proc.Id} exited with result {proc.ExitCode}");

            var args = new TestCaseFinishedArgs(proc.ExitCode, proc.GetOutputData(), proc.GetErrorData());
            OnFinished?.Invoke(args);
        }
        
        public void Terminate()
        {
            if (Process != null)
            {
                Process.Terminate(5000);
                Process = null;
            }
            else 
            {
                Console.WriteLine("[PROC][main] Process is not running. Nothing to terminate");
            }
        }

        void OnApplicationQuit()
        {
            Terminate();
        }

        private JSONObject CreateTestCaseConfig(string runtimeType, string testCaseFile, IDictionary<string,string> environment = null)
        {
            var config = new JSONObject();

            // version
            config.Add("version", "1.0");

            // runtime
            var runtime = new JSONObject();
            config.Add("runtime", runtime);

            // runtime.type
            runtime.Add("type", runtimeType);
            runtime.Add("testCaseFile", testCaseFile);

            // runtime.environment
            var environmentObj = new JSONObject();
            runtime.Add("environment", environmentObj);

            // runtime.environment.VAR = VALUE

            if (environment != null)
            {
                foreach (var envvar in environment)
                {
                    environmentObj.Add(envvar.Key, envvar.Value);
                }
            }
            return config;
        }
    }
}
