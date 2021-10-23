namespace SectionConfig.Test
{
	using System;
	using Xunit;

	public static class CfgValueListTest
	{
		[Fact]
		public static void Properties()
		{
			CfgKey key = CfgKey.Create("Key");
			CfgValueList cfg = new(key, new string[] { "value1", "value2", "value3" });
			Assert.Equal(key, cfg.Key);
			Assert.Collection(cfg.Values,
				x => Assert.Equal("value1", x),
				x => Assert.Equal("value2", x),
				x => Assert.Equal("value3", x));
			Assert.Equal(CfgType.ValueList, cfg.Type);
		}
		[Fact]
		public static void Casting()
		{
			CfgKey key = CfgKey.Create("Key");
			CfgValueList cfg = new(key, new string[] { "value1", "value2", "value3" });
			ICfgObject obj = cfg;
			Assert.True(obj.IsValueList(out CfgValueList? isValueList));
			Assert.Equal(cfg, isValueList);
			Assert.Equal(cfg, obj.AsValueList());
			Assert.Equal(cfg, obj.ToValueList());

			Assert.False(obj.IsValue(out CfgValue? isValue));
			Assert.Null(isValue);
			Assert.Null(obj.AsValue());
			Assert.Throws<InvalidCastException>(() => obj.ToValue());

			Assert.False(obj.IsSection(out CfgSection? isSection));
			Assert.Null(isSection);
			Assert.Null(obj.AsSection());
			Assert.Throws<InvalidCastException>(() => obj.ToSection());
		}
	}
}
