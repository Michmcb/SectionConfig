namespace SectionConfig.IO
{
	/// <summary>
	/// Defines the style of newline to write.
	/// </summary>
	public enum NewLine
	{
		/// <summary>
		/// <see cref="Lf"/> if unix, <see cref="CrLf"/> if non-unix
		/// </summary>
		Platform,
		/// <summary>
		/// Line Feed: \n (Unix)
		/// </summary>
		Lf,
		/// <summary>
		/// Carriage Return Line Feed: \r\n (Non-Unix)
		/// </summary>
		CrLf,
		/// <summary>
		/// Carriage Return: \r (Obsolete)
		/// </summary>
		Cr,
	}
}
