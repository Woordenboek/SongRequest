using System;
using System.Runtime.Serialization;

namespace SongRequest.SongPlayer
{
	[Serializable]
	public class SongPlayerException : Exception
	{
		/// <summary>
		/// Initializes a new SongPlayerException
		/// </summary>
		public SongPlayerException()
		{
		}

		/// <summary>
		/// Initializes a new SongPlayerException based on a string message
		/// </summary>
		/// <param name="message">The message</param>
		public SongPlayerException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new SongPlayerException based on a string message and an inner exception
		/// </summary>
		/// <param name="message">The message</param>
		/// <param name="innerException">The inner exception</param>
		public SongPlayerException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Constructor needed for serialization
		/// </summary>
		protected SongPlayerException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}

