namespace SectionConfig.IO
{
	/// <summary>
	/// Defines how to write multiline strings.
	/// </summary>
	public enum Multiline
	{
		/// <summary>
		/// If the value has any newline chars, then it will be multiline. Otherwise, single line.
		/// </summary>
		Auto,
		/// <summary>
		/// Always single line. If the value has any newline chars, it will be quoted.
		/// </summary>
		Never,
		/// <summary>
		/// Always multi line. Will still be quoted and not be multiline if not doing so would make it syntactically invalid.
		/// </summary>
		AlwaysIfPossible,
	}
}
