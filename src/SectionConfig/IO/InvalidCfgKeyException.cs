namespace SectionConfig.IO
{
	using System;

	/// <summary>
	/// The exception thrown when a <see cref="CfgKey"/> is not valid.
	/// </summary>
	public sealed class InvalidCfgKeyException : Exception
	{
		/// <summary>
		/// New instance with no error message.
		/// </summary>
		public InvalidCfgKeyException() { }
		/// <summary>
		/// New instance with the provided error message.
		/// </summary>
		/// <param name="message">The error message. Should state the key and why it is not valid.</param>
		public InvalidCfgKeyException(string? message) : base(message) { }
		/// <summary>
		/// New instance with the provided error message, and an inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message. Should state the key and why it is not valid.</param>
		/// <param name="innerException">The cause of this exception.</param>
		public InvalidCfgKeyException(string? message, Exception? innerException) : base(message, innerException) { }
	}
}
