namespace SectionConfig.Test
{
	using System;
	using Xunit;

	public static class CfgSectionTest
	{
		[Fact]
		public static void PropertiesAndMethods()
		{
			CfgKey key = CfgKey.Create("Key");
			CfgSection cfg = new(key, StringComparer.Ordinal);

			Assert.Equal(key, cfg.Key);
			Assert.Equal(CfgType.Section, cfg.Type);
			Assert.Empty(cfg.Elements);

			CfgValue val = new (CfgKey.Create("V"), "value");
			CfgValueList list = new (CfgKey.Create("L"), new string[] { "value1", "value2", "value3" });
			CfgSection section = new (CfgKey.Create("S"), StringComparer.Ordinal);
			Assert.Equal(AddError.Ok, cfg.TryAdd(val));
			Assert.Equal(AddError.AlreadyHasDifferentParent, cfg.TryAdd(val));
			Assert.Equal(AddError.KeyAlreadyExists, cfg.TryAdd(new CfgValue(val.Key, val.Value)));
			Assert.Equal(AddError.Ok, cfg.TryAdd(list));
			Assert.Equal(AddError.AlreadyHasDifferentParent, cfg.TryAdd(list));
			Assert.Equal(AddError.KeyAlreadyExists, cfg.TryAdd(new CfgValueList(list.Key, list.Values)));
			Assert.Equal(AddError.Ok, cfg.TryAdd(section));
			Assert.Equal(AddError.AlreadyHasDifferentParent, cfg.TryAdd(section));
			Assert.Equal(AddError.KeyAlreadyExists, cfg.TryAdd(new CfgSection(section.Key, StringComparer.Ordinal)));

			Assert.Equal(val, cfg.Elements["V"]);
			Assert.Equal(list, cfg.Elements["L"]);
			Assert.Equal(section, cfg.Elements["S"]);
		}
		[Fact]
		public static void Casting()
		{
			CfgKey key = CfgKey.Create("Key");
			CfgSection cfg = new(key, StringComparer.Ordinal);
			ICfgObject obj = cfg;
			Assert.True(obj.IsSection(out CfgSection? isSection));
			Assert.Equal(cfg, isSection);
			Assert.Equal(cfg, obj.AsSection());
			Assert.Equal(cfg, obj.ToSection());

			Assert.False(obj.IsValueList(out CfgValueList? isValueList));
			Assert.Null(isValueList);
			Assert.Null(obj.AsValueList());
			Assert.Throws<InvalidCastException>(() => obj.ToValueList());

			Assert.False(obj.IsValue(out CfgValue? isValue));
			Assert.Null(isValue);
			Assert.Null(obj.AsValue());
			Assert.Throws<InvalidCastException>(() => obj.ToValueList());
		}
	}
}
