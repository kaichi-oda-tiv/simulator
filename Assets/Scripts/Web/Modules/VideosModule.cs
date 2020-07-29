/**
 * Copyright (c) 2019 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using Nancy;
using Nancy.Responses;
using Nancy.Security;
using System;
using System.IO;
using UnityEngine;

namespace Simulator.Web.Modules
{
    public class VideosModule : NancyModule
    {
        public VideosModule() : base("videos")
        {
            this.RequiresAuthentication();

            string videosPath = Path.Combine(Config.PersistentDataPath, "Videos");
            if (!Directory.Exists(videosPath))
            {
                Directory.CreateDirectory(videosPath);
            }

            Get("/{fileName}", x =>
            {
                string fileName = x.fileName;
                try
                {
                    Debug.Log($"Getting video file with name {fileName}");

                    string absolutePath = Path.Combine(videosPath, fileName);
                    var response = new StreamResponse(() => {
                        return new FileStream(absolutePath, FileMode.Open, FileAccess.Read);
                    }, "video/mp4");

                    return response;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    return Response.AsJson(new { error = $"Failed to get video with the name {fileName}: {ex.Message}" }, HttpStatusCode.InternalServerError);
                }
            });
        }
    }
}
