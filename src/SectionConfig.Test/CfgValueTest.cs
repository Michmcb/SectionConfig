namespace SectionConfig.Test
{
	using System;
	using Xunit;

	public static class CfgValueTest
	{
		[Fact]
		public static void Properties()
		{
			CfgKey key = CfgKey.Create("Key");
			CfgValue cfg = new(key, "value");
			Assert.Equal(key, cfg.Key);
			Assert.Equal("value", cfg.Value);
			Assert.Equal(CfgType.Value, cfg.Type);
		}
		[Fact]
		public static void Casting()
		{
			CfgKey key = CfgKey.Create("Key");
			CfgValue cfg = new(key, "value");
			ICfgObject obj = cfg;
			Assert.True(obj.IsValue(out CfgValue? isValue));
			Assert.Equal(cfg, isValue);
			Assert.Equal(cfg, obj.AsValue());
			Assert.Equal(cfg, obj.ToValue());

			Assert.False(obj.IsValueList(out CfgValueList? isValueList));
			Assert.Null(isValueList);
			Assert.Null(obj.AsValueList());
			Assert.Throws<InvalidCastException>(() => obj.ToValueList());

			Assert.False(obj.IsSection(out CfgSection? isSection));
			Assert.Null(isSection);
			Assert.Null(obj.AsSection());
			Assert.Throws<InvalidCastException>(() => obj.ToSection());
		}
	}
}
