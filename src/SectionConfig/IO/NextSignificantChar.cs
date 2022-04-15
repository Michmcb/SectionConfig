namespace SectionConfig.IO
{
	/// <summary>
	/// The next significant character (if any) and whether or not we saw a newline before.
	/// </summary>
	internal readonly struct NextSignificantChar
	{
		public NextSignificantChar(char? next, bool sawNewline)
		{
			Next = next;
			SawNewline = sawNewline;
		}
		/// <summary>
		/// The next significant character. If null, the end of the stream was reached.
		/// </summary>
		public char? Next { get; }
		/// <summary>
		/// If true, we saw a newline before we got to <see cref="Next"/>.
		/// </summary>
		public bool SawNewline { get; }
	}
}
