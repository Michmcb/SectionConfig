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
		public static void AddAndRemove()
		{
			CfgValue v = new(CfgKey.Create("1"), "val");
			CfgValueList vl = new(CfgKey.Create("2"), new string[] { "val1", "val2" });
			CfgSection s = new(CfgKey.Create("3"), StringComparer.Ordinal);
			CfgRoot root = new( StringComparer.Ordinal);

			Assert.Equal(AddError.Ok, root.TryAdd(v));
			Assert.Equal(AddError.Ok, root.TryAdd(vl));
			Assert.Equal(AddError.Ok, root.TryAdd(s));

			Assert.Equal(AddError.AlreadyHasParent, root.TryAdd(v));
			Assert.Equal(AddError.AlreadyHasParent, root.TryAdd(vl));
			Assert.Equal(AddError.AlreadyHasParent, root.TryAdd(s));

			Assert.Equal(AddError.KeyAlreadyExists, root.TryAdd(new CfgValue(CfgKey.Create("1"), "val")));
			Assert.Equal(AddError.KeyAlreadyExists, root.TryAdd(new CfgValueList(CfgKey.Create("2"), new string[] { "val1", "val2" })));
			Assert.Equal(AddError.KeyAlreadyExists, root.TryAdd(new CfgSection(CfgKey.Create("3"), StringComparer.Ordinal)));

			Assert.True(root.Remove("1"));
			Assert.True(root.Remove("2"));
			Assert.True(root.Remove("3"));

			Assert.False(root.Remove("1"));
			Assert.False(root.Remove("2"));
			Assert.False(root.Remove("3"));

			Assert.Equal(AddError.Ok, root.TryAdd(v));
			Assert.Equal(AddError.Ok, root.TryAdd(vl));
			Assert.Equal(AddError.Ok, root.TryAdd(s));

			Assert.True(root.Remove(CfgKey.Create("1")));
			Assert.True(root.Remove(CfgKey.Create("2")));
			Assert.True(root.Remove(CfgKey.Create("3")));

			Assert.False(root.Remove(CfgKey.Create("1")));
			Assert.False(root.Remove(CfgKey.Create("2")));
			Assert.False(root.Remove(CfgKey.Create("3")));
		}
		[Fact]
		public static void TryGet()
		{
			CfgValue v = new(CfgKey.Create("1"), "val");
			CfgValueList vl = new(CfgKey.Create("2"), new string[] { "val1", "val2" });
			CfgSection s = new(CfgKey.Create("3"), StringComparer.Ordinal);
			CfgRoot sec = new(StringComparer.Ordinal);

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
			CfgRoot s = new(StringComparer.Ordinal);
			Assert.Null(s.Find(Array.Empty<string>()));
		}
		[Fact]
		public static void Find_NullNotFound()
		{
			CfgRoot s = new(StringComparer.Ordinal);
			Assert.Null(s.Find(new string[] { "1", "2", "3" }));

			Assert.Equal(AddError.Ok, s.TryAdd(new CfgSection(CfgKey.Create("1"), StringComparer.Ordinal)));
			Assert.Null(s.Find(new string[] { "1", "2", "3" }));
		}
		[Fact]
		public static void Find_FoundButWrongType()
		{
			CfgRoot s = new(StringComparer.Ordinal);
			Assert.Equal(AddError.Ok, s.TryAdd(new CfgSection(CfgKey.Create("1"), StringComparer.Ordinal)));
			Assert.Equal(AddError.Ok, s.Elements["1"].ToSection().TryAdd(new CfgValue(CfgKey.Create("Value"), "Val")));

			Assert.Null(s.Find(new string[] { "1", "Value", "Value2" }));
		}
		[Fact]
		public static void Find_Found()
		{
			CfgRoot s = new(StringComparer.Ordinal);
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
