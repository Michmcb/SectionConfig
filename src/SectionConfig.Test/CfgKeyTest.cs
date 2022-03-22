namespace SectionConfig.Test
{
	using SectionConfig.IO;
	using Xunit;

	public static class CfgKeyTest
	{
		[Fact]
		public static void Good()
		{
			CfgKey.Create("key");
		}
		[Fact]
		public static void Forbidden()
		{
			Assert.Throws<InvalidCfgKeyException>(() => CfgKey.Create(null!));
			Assert.Null(CfgKey.TryCreate(null!));

			Assert.Throws<InvalidCfgKeyException>(() => CfgKey.Create(""));
			Assert.Null(CfgKey.TryCreate(""));

			Assert.Throws<InvalidCfgKeyException>(() => CfgKey.Create(" "));
			Assert.Null(CfgKey.TryCreate(" "));

			Assert.Throws<InvalidCfgKeyException>(() => CfgKey.Create("\t"));
			Assert.Null(CfgKey.TryCreate("\t"));

			Assert.Throws<InvalidCfgKeyException>(() => CfgKey.Create("\n"));
			Assert.Null(CfgKey.TryCreate("\n"));

			Assert.Throws<InvalidCfgKeyException>(() => CfgKey.Create("#"));
			Assert.Null(CfgKey.TryCreate("#"));

			Assert.Throws<InvalidCfgKeyException>(() => CfgKey.Create(":"));
			Assert.Null(CfgKey.TryCreate(":"));

			Assert.Throws<InvalidCfgKeyException>(() => CfgKey.Create("{"));
			Assert.Null(CfgKey.TryCreate("{"));

			Assert.Throws<InvalidCfgKeyException>(() => CfgKey.Create("}"));
			Assert.Null(CfgKey.TryCreate("}"));
		}
		[Fact]
		public static void Equality()
		{
			Assert.True(CfgKey.Create("Key") == CfgKey.Create("Key"));
			Assert.True(CfgKey.Create("Key").Equals(CfgKey.Create("Key")));
			Assert.True(CfgKey.Create("Key").Equals((object?)CfgKey.Create("Key")));
			Assert.False(CfgKey.Create("Key").Equals(null));
			Assert.False(CfgKey.Create("Key") != CfgKey.Create("Key"));

			Assert.Equal("Key", CfgKey.Create("Key").ToString());
			Assert.Equal("Key".GetHashCode(), CfgKey.Create("Key").GetHashCode());
		}
	}
}
