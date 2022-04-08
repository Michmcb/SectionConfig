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

			CfgValue val = new(CfgKey.Create("V"), "value");
			CfgValueList list = new(CfgKey.Create("L"), new string[] { "value1", "value2", "value3" });
			CfgSection section = new(CfgKey.Create("S"), StringComparer.Ordinal);
			Assert.Equal(AddError.Ok, cfg.TryAdd(val));
			Assert.Equal(AddError.AlreadyHasParent, cfg.TryAdd(val));
			Assert.Equal(AddError.KeyAlreadyExists, cfg.TryAdd(new CfgValue(val.Key, val.Value)));
			Assert.Equal(AddError.Ok, cfg.TryAdd(list));
			Assert.Equal(AddError.AlreadyHasParent, cfg.TryAdd(list));
			Assert.Equal(AddError.KeyAlreadyExists, cfg.TryAdd(new CfgValueList(list.Key, list.Values)));
			Assert.Equal(AddError.Ok, cfg.TryAdd(section));
			Assert.Equal(AddError.AlreadyHasParent, cfg.TryAdd(section));
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
			Assert.Throws<InvalidCastException>(() => obj.ToValue());

			Assert.False(obj.IsValue(out CfgValue? isValue));
			Assert.Null(isValue);
			Assert.Null(obj.AsValue());
			Assert.Throws<InvalidCastException>(() => obj.ToValueList());
		}
		[Fact]
		public static void AddAndRemove()
		{
			CfgValue v = new(CfgKey.Create("1"), "val");
			CfgValueList vl = new(CfgKey.Create("2"), new string[] { "val1", "val2" });
			CfgSection s = new(CfgKey.Create("3"), StringComparer.Ordinal);
			CfgSection sec = new(CfgKey.Create("Key"), StringComparer.Ordinal);

			Assert.Equal(AddError.Ok, sec.TryAdd(v));
			Assert.Equal(AddError.Ok, sec.TryAdd(vl));
			Assert.Equal(AddError.Ok, sec.TryAdd(s));

			Assert.Equal(AddError.AlreadyHasParent, sec.TryAdd(v));
			Assert.Equal(AddError.AlreadyHasParent, sec.TryAdd(vl));
			Assert.Equal(AddError.AlreadyHasParent, sec.TryAdd(s));

			Assert.Equal(AddError.KeyAlreadyExists, sec.TryAdd(new CfgValue(CfgKey.Create("1"), "val")));
			Assert.Equal(AddError.KeyAlreadyExists, sec.TryAdd(new CfgValueList(CfgKey.Create("2"), new string[] { "val1", "val2" })));
			Assert.Equal(AddError.KeyAlreadyExists, sec.TryAdd(new CfgSection(CfgKey.Create("3"), StringComparer.Ordinal)));

			Assert.True(sec.Remove("1"));
			Assert.True(sec.Remove("2"));
			Assert.True(sec.Remove("3"));

			Assert.False(sec.Remove("1"));
			Assert.False(sec.Remove("2"));
			Assert.False(sec.Remove("3"));

			Assert.Equal(AddError.Ok, sec.TryAdd(v));
			Assert.Equal(AddError.Ok, sec.TryAdd(vl));
			Assert.Equal(AddError.Ok, sec.TryAdd(s));

			Assert.True(sec.Remove(CfgKey.Create("1")));
			Assert.True(sec.Remove(CfgKey.Create("2")));
			Assert.True(sec.Remove(CfgKey.Create("3")));

			Assert.False(sec.Remove(CfgKey.Create("1")));
			Assert.False(sec.Remove(CfgKey.Create("2")));
			Assert.False(sec.Remove(CfgKey.Create("3")));
		}
		[Fact]
		public static void TryGet()
		{
			CfgValue v = new(CfgKey.Create("1"), "val");
			CfgValueList vl = new(CfgKey.Create("2"), new string[] { "val1", "val2" });
			CfgSection s = new(CfgKey.Create("3"), StringComparer.Ordinal);
			CfgSection sec = new(CfgKey.Create("Key"), StringComparer.Ordinal);

			Assert.Equal(AddError.Ok, sec.TryAdd(v));
			Assert.Equal(AddError.Ok, sec.TryAdd(vl));
			Assert.Equal(AddError.Ok, sec.TryAdd(s));

			Assert.NotNull(sec.TryGetValue("1"));
			Assert.Null(sec.TryGetValue("2"));
			Assert.Null(sec.TryGetValue("3"));
			Assert.Null(sec.TryGetValue("4"));

			Assert.Null(sec.TryGetValueList("1"));
			Assert.NotNull(sec.TryGetValueList("2"));
			Assert.Null(sec.TryGetValueList("3"));
			Assert.Null(sec.TryGetValueList("4"));

			Assert.Null(sec.TryGetSection("1"));
			Assert.Null(sec.TryGetSection("2"));
			Assert.NotNull(sec.TryGetSection("3"));
			Assert.Null(sec.TryGetSection("4"));
		}
		[Fact]
		public static void Find_NullEmptyKeys()
		{
			CfgSection s = new(CfgKey.Create("3"), StringComparer.Ordinal);
			Assert.Null(s.Find(Array.Empty<string>()));
		}
		[Fact]
		public static void Find_NullNotFound()
		{
			CfgSection s = new(CfgKey.Create("Section"), StringComparer.Ordinal);
			Assert.Null(s.Find(new string[] { "1", "2", "3" }));

			Assert.Equal(AddError.Ok, s.TryAdd(new CfgSection(CfgKey.Create("1"), StringComparer.Ordinal)));
			Assert.Null(s.Find(new string[] { "1", "2", "3" }));
		}
		[Fact]
		public static void Find_FoundButWrongType()
		{
			CfgSection s = new(CfgKey.Create("Section"), StringComparer.Ordinal);
			Assert.Equal(AddError.Ok, s.TryAdd(new CfgSection(CfgKey.Create("1"), StringComparer.Ordinal)));
			Assert.Equal(AddError.Ok, s.Elements["1"].ToSection().TryAdd(new CfgValue(CfgKey.Create("Value"), "Val")));

			Assert.Null(s.Find(new string[] { "1", "Value", "Value2" }));
		}
		[Fact]
		public static void Find_Found()
		{
			CfgSection s = new(CfgKey.Create("Section"), StringComparer.Ordinal);
			Assert.Equal(AddError.Ok, s.TryAdd(new CfgSection(CfgKey.Create("1"), StringComparer.Ordinal)));
			Assert.Equal(AddError.Ok, s.Elements["1"].ToSection().TryAdd(new CfgValue(CfgKey.Create("Value"), "Val")));

			CfgSection foundSection = Assert.IsType<CfgSection>(s.Find(new string[] { "1" }));
			Assert.Equal("1", foundSection.Key.KeyString);

			CfgValue foundValue = Assert.IsType<CfgValue>(s.Find(new string[] { "1", "Value" }));
			Assert.Equal("Value", foundValue.Key.KeyString);
			Assert.Equal("Val", foundValue.Value);
		}
	}
}
