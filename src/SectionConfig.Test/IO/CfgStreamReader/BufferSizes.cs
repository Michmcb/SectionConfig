namespace SectionConfig.Test.IO.CfgStreamReader
{
	using SectionConfig.IO;
	using System;
	using System.IO;
	using Xunit;

	public static class BufferSizes
	{
		
		[Fact]
		public static void TestBufferSizes()
		{
			using CfgStreamReader csr = new(TextReader.Null, initialBufferSize: 0);
			Assert.Equal(CfgStreamReader.DefaultBufferSize, csr.BufferSize);
			Assert.Throws<ArgumentOutOfRangeException>(() => new CfgStreamReader(TextReader.Null, initialBufferSize: -1));
		}
	}
}
