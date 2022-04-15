//namespace SectionConfig.Test.IO.CfgLoader
//{
//	using SectionConfig.IO;
//	using System;
//	using Xunit;

//	public static class ReadResultTests
//	{
//		[Fact]
//		public static void ExceptionsAndStuff()
//		{
//			ReadResult rr = new( CfgKey.Create("Key"));
//			Assert.Equal("Key", rr.Key.KeyString);
//			Assert.Throws<InvalidOperationException>(() => rr.Content);

//			rr = new(SectionCfgToken.Value, "Value");
//			Assert.Equal("Value", rr.Content);
//			Assert.Throws<InvalidOperationException>(() => rr.Key);
//		}
//	}
//}
