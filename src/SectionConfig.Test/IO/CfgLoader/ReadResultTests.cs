namespace SectionConfig.Test.IO.CfgLoader
{
	using SectionConfig.IO;
	using System;
	using Xunit;

	public static class ReadResultTests
	{
		[Fact]
		public static void ExceptionsAndStuff()
		{
			ReadResult rr = new(CfgKey.Create("Key"));
			Assert.Equal("Key", rr.GetKey().KeyString);
			Assert.Throws<InvalidOperationException>(() => rr.GetContent());

			rr = new(SectionCfgToken.Value, "Value");
			Assert.Equal("Value", rr.GetContent());
			Assert.Throws<InvalidOperationException>(() => rr.GetKey());
		}
	}
}
