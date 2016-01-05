using SongRequest.Config;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace SongRequest.SongPlayer
{
	public class SongPlayerFactory : IDisposable
	{
		private static object _lockObject = new object();
		private static SongPlayerFactory _factory;

		private SongPlayer _songPlayer;

		private static SongPlayerFactory Instance
		{
			get
			{
				lock (_lockObject)
				{
					if (_factory == null)
					{
						_factory = new SongPlayerFactory();
						_factory._songPlayer = new SongPlayer(_factory.MediaDevice);
					}
				}

				return _factory;
			}
		}

		public static ISongPlayer GetSongPlayer()
		{
			return Instance._songPlayer;
		}

		[Import(typeof(IMediaDevice))]
		public IMediaDevice MediaDevice { get; set; }

		private SongPlayerFactory()
		{
			using (DirectoryCatalog catalog = new DirectoryCatalog("addins"))
			using (CompositionContainer container = new CompositionContainer(catalog))
			{
				container.ComposeParts(this);
			}
		}

		public static ConfigFile GetConfigFile()
		{
			if (!System.IO.File.Exists("songrequest.config"))
			{
				ConfigFile configFile = new ConfigFile("songrequest.config");
				configFile.SetValue("server.host", "*");
				configFile.SetValue("server.port", "8765");
				configFile.SetValue("server.clients", "all");

				if (Settings.IsRunningOnWindows())
					configFile.SetValue("library.path", "c:\\music");
				else
					configFile.SetValue("library.path", "~/Music");

				configFile.SetValue("library.minutesbetweenscans", "1");
				configFile.SetValue("library.extensions", "mp3");
				configFile.SetValue("player.minimalsonginqueue", "0");
				configFile.SetValue("player.maximalsonginqueue", "500");
				configFile.SetValue("player.startupvolume", "50");
				configFile.Save();
			}

			return new ConfigFile("songrequest.config");
		}

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_songPlayer != null)
				{
					_songPlayer.Dispose();
					_songPlayer = null;
				}
			}
		}
	}
}

