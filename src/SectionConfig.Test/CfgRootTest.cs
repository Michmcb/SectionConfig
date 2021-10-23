namespace SectionConfig.Test
{
	using System;
	using Xunit;

	public static class CfgRootTest
	{
		[Fact]
		public static void PropertiesAndMethods()
		{
			CfgKey key = CfgKey.Create("Key");
			CfgRoot cfg = new(StringComparer.Ordinal);

			Assert.Empty(cfg.Elements);

			CfgValue val = new(CfgKey.Create("V"), "value");
			CfgValueList list = new(CfgKey.Create("L"), new string[] { "value1", "value2", "value3" });
			CfgSection section = new(CfgKey.Create("S"), StringComparer.Ordinal);
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
	}
}
