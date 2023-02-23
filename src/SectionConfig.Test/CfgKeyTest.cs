namespace SectionConfig.Test
{
	using SectionConfig.IO;
	using System;
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
			CfgKey key = CfgKey.Create("Key");
			Assert.Equal("Key", key.ToString());
			Assert.Equal(key.KeyString.GetHashCode(), key.GetHashCode());

			foreach ((CfgKey key1, CfgKey key2) in new (CfgKey, CfgKey)[]
			{
				(CfgKey.Create("Key"), CfgKey.Create("Key")),
				(default(CfgKey), default(CfgKey))
			})
			{
				Assert.True(key1 == key2);
				Assert.True(key1.Equals(key2));
				Assert.True(key1.Equals((object?)key2));
				Assert.False(key1.Equals(null));
				Assert.False(key2.Equals(null));
				Assert.False(key1 != key2);
				Assert.Equal(key1.GetHashCode(), key2.GetHashCode());
			}
		}
	}
}
