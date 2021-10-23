namespace SectionConfig.IO
{
	/// <summary>
	/// Syntax characters
	/// </summary>
	public static class CfgSyntax
	{
		/// <summary>
		/// Starts a comment, which lasts from this char until the end of the line.
		/// </summary>
		public const char Comment = '#';
		/// <summary>
		/// Denotes the end of a key, to be followed by a value or a list of values.
		/// </summary>
		public const char KeyEnd = ':';
		/// <summary>
		/// Starts a section or a list.
		/// </summary>
		public const char StartSectionOrList = '{';
		/// <summary>
		/// Ends a section or a list.
		/// </summary>
		public const char EndSectionOrList = '}';
		/// <summary>
		/// A single quote, to enclose literal strings.
		/// </summary>
		public const char SingleQuote = '\'';
		/// <summary>
		/// A double quote, to enclose literal strings.
		/// </summary>
		public const char DoubleQuote = '"';
	}
}
