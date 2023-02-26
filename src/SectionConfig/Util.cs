namespace SectionConfig
{
	using System;
	/// <summary>
	/// Utilities
	/// </summary>
	public static class Util
	{
		/// <summary>
		/// A callback which allows capturing a slice of string.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="offset">The offset from the beginning.</param>
		/// <param name="length">The length of the slice.</param>
		public delegate void SpanAction(ReadOnlySpan<char> str, int offset, int length);
		/// <summary>
		/// Works like splitting a string, except instead of returning the slices, it invokes <paramref name="callback"/> for every slice.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="separator">The separator.</param>
		/// <param name="callback">The callback to invoke for every slice.</param>
		public static void SpanSplit(ReadOnlySpan<char> str, char separator, SpanAction callback)
		{
			int from;
			int to = -1;
			while (true)
			{
				from = to + 1;
				if (from >= str.Length)
				{
					// If we're at the end of the string, then 
					callback(str, from, 0);
					break;
				}
#pragma warning disable IDE0057 // Use range operator
				int index = str.Slice(from).IndexOf(separator);
#pragma warning restore IDE0057 // Use range operator
				if (index != -1)
				{
					// Index 0 is the start of the string
					to = from + index;
					callback(str, from, to - from);
				}
				else
				{
					// Last one. However if from is str.Length - 1, it's an empty entry
					callback(str, from, str.Length - from);
					break;
				}
			}
		}
	}
}