using System;
using System.Runtime.Serialization;

namespace SongRequest.Config
{
	[Serializable]
	public class ConfigException : Exception
	{
		/// <summary>
		/// Initializes a new SongPlayerException
		/// </summary>
		public ConfigException()
		{
		}

		/// <summary>
		/// Initializes a new ConfigException based on a string message
		/// </summary>
		/// <param name="message">The message</param>
		public ConfigException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new ConfigException based on a string message and an inner exception
		/// </summary>
		/// <param name="message">The message</param>
		/// <param name="innerException">The inner exception</param>
		public ConfigException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Constructor needed for serialization
		/// </summary>
		protected ConfigException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}

