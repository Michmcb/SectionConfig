#if NETSTANDARD2_0
namespace System.IO
{
	using System.Buffers;
	/// <summary>
	/// Shims for .netstandard 2.0.
	/// </summary>
	public static class TextWriterExtensions
	{
		/// <summary>
		/// Writes <paramref name="buffer"/> to <paramref name="writer"/>.
		/// Just a copy of code that is in .NET Core so .netstandard2.0 can use it.
		/// </summary>
		/// <param name="writer">The text writer to write to.</param>
		/// <param name="buffer">The data to write.</param>
		public static void Write(this TextWriter writer, ReadOnlySpan<char> buffer)
		{
			// Writes a span of characters to the text stream.
			char[] array = ArrayPool<char>.Shared.Rent(buffer.Length);

			try
			{
				buffer.CopyTo(new Span<char>(array));
				writer.Write(array, 0, buffer.Length);
			}
			finally
			{
				ArrayPool<char>.Shared.Return(array);
			}
		}
	}
}
#endif