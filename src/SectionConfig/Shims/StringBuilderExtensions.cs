#if NETSTANDARD2_0
namespace System.Text
{
	using System;
	/// <summary>
	/// Shims for .netstandard 2.0.
	/// </summary>
	public static class StringBuilderExtensions
	{
		/// <summary>
		/// Appends <paramref name="str"/> to <paramref name="sb"/>.
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="str"></param>
		/// <returns></returns>
		public static unsafe StringBuilder Append(this StringBuilder sb, ReadOnlySpan<char> str)
		{
			fixed (char* ptr = str)
			{
				sb.Append(ptr, str.Length);
			}
			return sb;
		}
	}
}
#endif