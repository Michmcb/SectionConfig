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
		/// Line Feed: \n (Unix and Mac OS X)
		/// </summary>
		Lf,
		/// <summary>
		/// Carriage Return Line Feed: \r\n (Non-Unix)
		/// </summary>
		CrLf,
		/// <summary>
		/// Carriage Return: \r (Mac OS 9 and earlier, (released in 1999)).
		/// Carriage Return is pretty uncommon nowadays, it's best to use <see cref="Lf"/> or <see cref="CrLf"/>.
		/// </summary>
		Cr,
	}
}
