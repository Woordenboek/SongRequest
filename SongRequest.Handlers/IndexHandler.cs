﻿using System.Net;
using SongRequest.Config;
using SongRequest.SongPlayer;

namespace SongRequest.Handlers
{
    public class IndexHandler : StaticHandler
    {
        public override void Process(HttpListenerRequest request, HttpListenerResponse response)
        {
            ConfigFile config = SongPlayerFactory.GetConfigFile();

            string text = Get("index.htm");
            response.ContentType = "text/html";

            WriteUtf8String(response.OutputStream, text);
        }
    }
}
