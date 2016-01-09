using System;

namespace skwas.Forms
{
	/// <summary>
	/// The exception that is thrown when an error occurs in the Window class.
	/// </summary>
	[Serializable]
	public class WindowException 
		: ApplicationException
	{
		/// <summary>
		/// Initializes a new instance of <see cref="WindowException"/>.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="innerException">The inner exception.</param>
		public WindowException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="WindowException"/>.
		/// </summary>
		/// <param name="message">The message.</param>
		public WindowException(string message)
			: base(message)
		{
		}
	}
}