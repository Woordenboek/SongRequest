﻿using System.Configuration;

namespace SongRequest
{
	public static class ConfigReader
	{
		public static int? VlcPort
		{
			get
			{
				string vlcPort = ConfigurationManager.AppSettings["VlcPort"];

				int port = 0;
				if (int.TryParse(vlcPort, out port) && port > 0)
					return port;

				return null;
			}
		}

		public static int? SongRequestPort
		{
			get
			{
				string tpPort = ConfigurationManager.AppSettings["SongRequestPort"];

				int port = 0;
				if (int.TryParse(tpPort, out port) && port > 0)
					return port;

				return null;
			}
		}
	}
}
