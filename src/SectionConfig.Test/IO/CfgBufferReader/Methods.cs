namespace SectionConfig.Test.IO.CfgBufferReader
{
	using SectionConfig.IO;
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
	}
}
