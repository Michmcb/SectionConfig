namespace SectionConfig
{
	/// <summary>
	/// A way of returning either a Value or an Error.
	/// </summary>
	/// <typeparam name="TVal">The Type on success.</typeparam>
	/// <typeparam name="TErr">The Type on failure.</typeparam>
	public readonly struct ValOrErr<TVal, TErr> where TVal : class where TErr : struct
	{
		/// <summary>
		/// Creates a new instance with <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The success value.</param>
		public ValOrErr(TVal value)
		{
			Value = value;
			Error = default;
		}
		/// <summary>
		/// Creates a new instance with <paramref name="error"/>.
		/// </summary>
		/// <param name="error">The error value.</param>
		public ValOrErr(TErr error)
		{
			Value = null;
			Error = error;
		}
		/// <summary>
		/// The value. May be null on failure.
		/// </summary>
		public TVal? Value { get; }
		/// <summary>
		/// The error. Never null, will be a default value on failure.
		/// </summary>
		public TErr Error { get; }
	}
}
