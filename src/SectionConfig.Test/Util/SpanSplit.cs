namespace SectionConfig.Test.Util
{
	using SectionConfig;
	using System.Collections.Generic;
	using Xunit;

	public static class SpanSplit
	{
		[Fact]
		public static void NoSeparators()
		{
			List<(int actualOffset, int actualLength)> expected = new();
			Util.SpanSplit("Hello world", '\n', (_, offset, length) => expected.Add((offset, length)));

			Assert.Collection(expected,
				x => LengthOffset(0, 11, x));
		}
		[Fact]
		public static void LeadingSeparator()
		{
			List<(int actualOffset, int actualLength)> expected = new();
			Util.SpanSplit("\nHello world", '\n', (_, offset, length) => expected.Add((offset, length)));

			Assert.Collection(expected, x => LengthOffset(0, 0, x),
				x => LengthOffset(1, 11, x));
		}
		[Fact]
		public static void TrailingSeparator()
		{
			List<(int actualOffset, int actualLength)> expected = new();
			Util.SpanSplit("Hello world\n", '\n', (_, offset, length) => expected.Add((offset, length)));

			Assert.Collection(expected, x => LengthOffset(0, 11, x),
				x => LengthOffset(12, 0, x));
		}
		[Fact]
		public static void LeadingTrailingSeparator()
		{
			List<(int actualOffset, int actualLength)> expected = new();
			Util.SpanSplit("\nHello world\n", '\n', (_, offset, length) => expected.Add((offset, length)));

			Assert.Collection(expected, x => LengthOffset(0, 0, x),
				x => LengthOffset(1, 11, x),
				x => LengthOffset(13, 0, x));
		}
		[Fact]
		public static void LeadingInteriorTrailingSeparator()
		{
			List<(int actualOffset, int actualLength)> expected = new();
			Util.SpanSplit("\nHello\nworld\n", '\n', (_, offset, length) => expected.Add((offset, length)));

			Assert.Collection(expected, x => LengthOffset(0, 0, x),
				x => LengthOffset(1, 5, x),
				x => LengthOffset(7, 5, x),
				x => LengthOffset(13, 0, x));
		}
		[Fact]
		public static void OneSeparator()
		{
			List<(int actualOffset, int actualLength)> expected = new();
			Util.SpanSplit("Hello\nworld", '\n', (_, offset, length) => expected.Add((offset, length)));

			Assert.Collection(expected, x => LengthOffset(0, 5, x),
				x => LengthOffset(6, 5, x));
		}
		[Fact]
		public static void ThreeSeparators()
		{
			List<(int actualOffset, int actualLength)> expected = new();
			Util.SpanSplit("Hello\nworld\nfoo\nbar", '\n', (_, offset, length) => expected.Add((offset, length)));

			Assert.Collection(expected, x => LengthOffset(0, 5, x),
				x => LengthOffset(6, 5, x),
				x => LengthOffset(12, 3, x),
				x => LengthOffset(16, 3, x));
		}
		private static void LengthOffset(int offset, int length, (int offset, int length) tuple)
		{
			Assert.Equal(offset, tuple.offset);
			Assert.Equal(length, tuple.length);
		}
	}
}
