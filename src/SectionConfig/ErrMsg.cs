namespace SectionConfig
{
	/// <summary>
	/// A combination of an error code and a message.
	/// </summary>
	/// <typeparam name="TCode">The type of the error code.</typeparam>
	public readonly struct ErrMsg<TCode> where TCode : struct
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="code">The error code.</param>
		/// <param name="message">The error message.</param>
		public ErrMsg(TCode code, string? message)
		{
			Code = code;
			Message = message;
		}
		/// <summary>
		/// The error code.
		/// </summary>
		public TCode Code { get; }
		/// <summary>
		/// The error message.
		/// </summary>
		public string? Message { get; }
		/// <summary>
		/// Returns <see cref="Message"/>.
		/// </summary>
		/// <returns><see cref="Message"/>.</returns>
		public override string? ToString()
		{
			return Message;
		}
	}
}
