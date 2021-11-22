#if NETSTANDARD2_0
namespace System.IO
{
	using System.Buffers;
	public static class TextWriterExtensions
	{
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