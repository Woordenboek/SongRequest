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

		private static byte[] _lastImage = null;
		private static string _lastId = null;

		private static object _lockObject = new object();

		private static Dictionary<string, Tuple<DateTime, byte[]>> thumbnailBuffer = new Dictionary<string, Tuple<DateTime, byte[]>>();

		public static void Purge()
		{
			lock (_lockObject)
			{
				thumbnailBuffer.Clear();
			}
		}

		public static void CleanBuffer()
		{
			lock (_lockObject)
			{
				foreach (KeyValuePair<string, Tuple<DateTime, byte[]>> keyValuePair in thumbnailBuffer.ToArray())
				{
					DateTime lastAccess = keyValuePair.Value.Item1;

					if (lastAccess.AddHours(8) < DateTime.Now)
					{
						thumbnailBuffer.Remove(keyValuePair.Key);
					}
				}
			}
		}

		public static void HelpMe(HttpListenerResponse response, string tempId, ISongPlayer songPlayer, bool large)
		{
			lock (_lockObject)
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
		}

		private static void HelpMeLarge(HttpListenerResponse response, string tempId, ISongPlayer songPlayer)
		{
			// use cached image if possible
			if (tempId.Equals(_lastId, StringComparison.OrdinalIgnoreCase) && _lastImage != null)
			{
				WriteImage(response, _lastImage);
				return;
			}

			// get from player
			MemoryStream imageStream = null;
			if (!string.IsNullOrEmpty(tempId))
			{
				try
				{
					// this is locked in function
					imageStream = songPlayer.GetImageStream(tempId, true);
				}
				catch (Exception)
				{
					imageStream = null;
				}

				using (MemoryStream streamFromSongPlayer = imageStream)
				{
					if (streamFromSongPlayer != null)
					{
						// set last id
						_lastId = tempId;
						_lastImage = streamFromSongPlayer.ToArray();
						WriteImage(response, _lastImage);
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

		private static void HelpMeSmall(HttpListenerResponse response, string tempId, ISongPlayer songPlayer)
		{
			if (thumbnailBuffer.ContainsKey(tempId))
			{
				Tuple<DateTime, byte[]> tuple = thumbnailBuffer[tempId];
				byte[] content = tuple.Item2;

				// update last access
				thumbnailBuffer[tempId] = new Tuple<DateTime, byte[]>(DateTime.Now, content);

				WriteImage(response, content);
				return;
			}

			// get from player
			MemoryStream imageStream = null;
			if (!string.IsNullOrEmpty(tempId))
			{
				try
				{
					// this is locked in function
					imageStream = songPlayer.GetImageStream(tempId, false);
				}
				catch (Exception)
				{
					imageStream = null;
				}

				using (MemoryStream streamFromSongPlayer = imageStream)
				{
					if (streamFromSongPlayer != null)
					{
						if (!thumbnailBuffer.ContainsKey(tempId))
							thumbnailBuffer.Add(tempId, new Tuple<DateTime, byte[]>(DateTime.Now, streamFromSongPlayer.ToArray()));
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

			if (!thumbnailBuffer.ContainsKey(tempId))
				thumbnailBuffer.Add(tempId, new Tuple<DateTime, byte[]>(DateTime.Now, _emptyImageSmall));
			WriteImage(response, _emptyImageSmall);
		}

		private static void WriteImage(HttpListenerResponse response, MemoryStream streamToCopy)
		{
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
				memoryStream.Position = 0;
				WriteImage(response, memoryStream);
			}
		}
	}
}
