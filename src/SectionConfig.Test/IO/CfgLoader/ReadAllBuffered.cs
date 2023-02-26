namespace SectionConfig.Test.IO.CfgLoader
{
	using SectionConfig.IO;
	using System;
	using System.IO;
	using Xunit;

	public static class ReadAllBuffered
	{
		[Fact]
		public static void BadBufferSize()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => CfgLoader.ReadAllBuffered<object?>(TextReader.Null, null, (_, _, _) => { return false; }, initialBufferSize: -1));
		}

		[Fact]
		public static void StopsAfterFalse()
		{
			int i = 0;
			bool b = CfgLoader.ReadAllBuffered<object?>(new StringReader("Section{Key:Value\n}"), null, (state, token, reader) =>
			{
				++i;
				return false;
			}, initialBufferSize: 0);
			Assert.False(b);
			Assert.Equal(1, i);
		}
	}
}
