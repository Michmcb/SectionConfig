namespace SectionConfig.Test.IO.CfgBufferReader
{
	using SectionConfig.IO;
	using System;
	using Xunit;
	public static class Methods
	{
		[Fact]
		public static void CopyLeftoverAndResetPosition_BufferTooSmall()
		{
			char[] buf = new char[10];
			CfgBufferReader cbr = new(buf, isFinalBlock: false);
			char[] smol = new char[5];
			Assert.False(cbr.CopyLeftoverAndResetPosition(smol, out int copied));
			Assert.Equal(0, copied);
		}
		[Fact]
		public static void MassiveBufferSize()
		{
			CfgBufferReader cbr = new(new char[Array.MaxLength], isFinalBlock: false);
			int sz = cbr.SuggestedNewBufferSize();
			Assert.True(sz == int.MaxValue);
		}
	}
}
