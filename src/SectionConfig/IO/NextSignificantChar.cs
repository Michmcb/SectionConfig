namespace SectionConfig.IO
{
	/// <summary>
	/// The next significant character (if any) and whether or not we saw a newline before .
	/// </summary>
	internal readonly struct NextSignificantChar
	{
		public NextSignificantChar(char? next, bool sawNewline)
		{
			Next = next;
			SawNewline = sawNewline;
		}
		public char? Next { get; }
		public bool SawNewline { get; }
	}
}
