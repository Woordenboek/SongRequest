using SongRequest.SongPlayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace SongRequest.Handlers
{
    public static class ImageHelper
    {
        private static byte[] _emptyImageLarge = null;
        private static byte[] _emptyImageSmall = null;

        private static object _lockObject = new object();

        private static string _imageCachePath = Path.Combine(Environment.CurrentDirectory, "image-cache");

        public static void Purge()
        {
            lock (_lockObject)
            {
                if (Directory.Exists(_imageCachePath))
                    Directory.Delete(_imageCachePath, true);
            }
        }

        public static void HelpMe(HttpListenerResponse response, string tempId, ISongPlayer songPlayer, bool large)
        {
            if (!large)
            {
                HelpMeSmall(response, tempId ?? string.Empty, songPlayer);
            }
            else
            {
                HelpMeLarge(response, tempId ?? string.Empty, songPlayer);
            }
        }

        private static void HelpMeLarge(HttpListenerResponse response, string tempId, ISongPlayer songPlayer)
        {
            int size = 300;
            string cacheFolder = Path.Combine(_imageCachePath, tempId.Length > 2 ? tempId.Substring(0, 2) : tempId);
            string cacheKey = Path.Combine(cacheFolder, tempId + "_" + size + ".png");
            if (File.Exists(cacheKey))
            {
                using (FileStream fileStream = File.OpenRead(cacheKey))
                {
                    WriteImage(response, fileStream);
                    return;
                }
            }

            lock (_lockObject)
            {
                // get from player
                MemoryStream imageStream = null;
                if (!string.IsNullOrEmpty(tempId))
                {
                    try
                    {
                        // this is locked in function
                        imageStream = songPlayer.GetImageStream(tempId, size);
                    }
                    catch (Exception)
                    {
                        imageStream = null;
                    }

                    using (MemoryStream streamFromSongPlayer = imageStream)
                    {
                        if (streamFromSongPlayer != null)
                        {
                            if (!Directory.Exists(cacheFolder))
                                Directory.CreateDirectory(cacheFolder);

                            using (FileStream fileStream = File.Create(cacheKey))
                            {
                                streamFromSongPlayer.CopyTo(fileStream);
                            }

                            WriteImage(response, streamFromSongPlayer);
                            return;
                        }
                    }
                }

                // cache large if not present
                if (_emptyImageLarge == null)
                {
                    using (Stream stream = Assembly.GetEntryAssembly().GetManifestResourceStream("SongRequest.Static.empty.png"))
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        _emptyImageLarge = memoryStream.ToArray();
                    }
                }

                WriteImage(response, _emptyImageLarge);
            }
        }

        private static void HelpMeSmall(HttpListenerResponse response, string tempId, ISongPlayer songPlayer)
        {
            string cacheFolder = Path.Combine(_imageCachePath, tempId.Length > 2 ? tempId.Substring(0, 2) : tempId);

            int size = 20;
            string cacheKey = Path.Combine(cacheFolder, tempId + "_" + size + ".png");

            if (File.Exists(cacheKey))
            {
                using (FileStream fileStream = File.OpenRead(cacheKey))
                {
                    WriteImage(response, fileStream);
                    return;
                }
            }

            lock (_lockObject)
            {
                // get from player
                MemoryStream imageStream = null;
                if (!string.IsNullOrEmpty(tempId))
                {
                    try
                    {
                        // this is locked in function
                        imageStream = songPlayer.GetImageStream(tempId, size);
                    }
                    catch (Exception)
                    {
                        imageStream = null;
                    }

                    using (MemoryStream streamFromSongPlayer = imageStream)
                    {
                        if (streamFromSongPlayer != null)
                        {
                            if (!Directory.Exists(cacheFolder))
                                Directory.CreateDirectory(cacheFolder);

                            using (FileStream fileStream = File.Create(cacheKey))
                            {
                                streamFromSongPlayer.CopyTo(fileStream);
                            }

                            WriteImage(response, streamFromSongPlayer);
                            return;
                        }
                    }
                }

                // cache small if not present
                if (_emptyImageSmall == null)
                {
                    using (Stream stream = Assembly.GetEntryAssembly().GetManifestResourceStream("SongRequest.Static.empty_small.png"))
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        _emptyImageSmall = memoryStream.ToArray();
                    }
                }

                WriteImage(response, _emptyImageSmall);
            }
        }

        private static void WriteImage(HttpListenerResponse response, Stream streamToCopy)
        {
            if (streamToCopy.Position != 0)
                streamToCopy.Position = 0;

            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentLength64 = streamToCopy.Length;
            response.ContentType = "image/png";

            // copy to response
            streamToCopy.CopyTo(response.OutputStream);
        }

        private static void WriteImage(HttpListenerResponse response, byte[] bytes)
        {
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                WriteImage(response, memoryStream);
            }
        }
    }
}
