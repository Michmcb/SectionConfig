namespace SectionConfig.Test.IO
{
	using SectionConfig.IO;
	using System.IO;
	using Xunit;
	public static class WritingGood
	{
		private static readonly CfgKey key = CfgKey.Create("Key");
		[Fact]
		public static void KeyValues()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf))
			{
				scw.WriteKeyValue(CfgKey.Create("Key1"), "Value1");
				scw.WriteKey(CfgKey.Create("Key2"));
				scw.WriteValue("\"Value2\"");
				scw.WriteKeyValue(CfgKey.Create("Key3"), "'Value3'");
				scw.WriteKeyValue(CfgKey.Create("Key4"), "  Value4  ");
				scw.WriteKeyValue(CfgKey.Create("Key5"), "Value5\nValue 5 still");
			}
			Assert.Equal("Key1: Value1\nKey2: \"\"\"Value2\"\"\"\nKey3: \"'Value3'\"\nKey4: \"  Value4  \"\nKey5:\n\tValue5\n\tValue 5 still\n", sw.ToString());
		}
		[Fact]
		public static void SectionKeyValue()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf))
			{
				WriteSectionToken st = scw.WriteKeyOpenSection(CfgKey.Create("Section"));
				scw.WriteKeyValue(CfgKey.Create("Key1"), "Value1");
				scw.WriteKey(CfgKey.Create("Key2"));
				scw.WriteValue("Value2");
				scw.WriteKeyValue(CfgKey.Create("Key3"), "Value3");
				st.Close();
			}
			Assert.Equal("Section {\n\tKey1: Value1\n\tKey2: Value2\n\tKey3: Value3\n}\n", sw.ToString());
		}
		[Fact]
		public static void NestedSectionKeyValue()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf))
			{
				WriteSectionToken st1 = scw.WriteKeyOpenSection(CfgKey.Create("Section1"));
				WriteSectionToken st2 = scw.WriteKeyOpenSection(CfgKey.Create("Section 2"));
				WriteSectionToken st3 = scw.WriteKeyOpenSection(CfgKey.Create("Section 3"));
				scw.WriteKeyValue(CfgKey.Create("Key1"), "Value1");
				scw.WriteKey(CfgKey.Create("Key2"));
				scw.WriteValue("Value2");
				scw.WriteKeyValue(CfgKey.Create("Key3"), "Value3");
				st3.Close();
				st2.Close();
				st1.Close();
			}
			Assert.Equal("Section1 {\n\tSection 2 {\n\t\tSection 3 {\n\t\t\tKey1: Value1\n\t\t\tKey2: Value2\n\t\t\tKey3: Value3\n\t\t}\n\t}\n}\n", sw.ToString());
		}
		[Fact]
		public static void KeyValueList()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf))
			{
				WriteValueListToken vl = scw.WriteKeyOpenValueList(CfgKey.Create("Key1"));
				vl.WriteListValue("Value1");
				vl.WriteListValue("Value 2");
				vl.WriteListValue("Value  3");
				vl.Close();
			}
			Assert.Equal("Key1: {\n\tValue1\n\tValue 2\n\tValue  3\n}\n", sw.ToString());
		}
		[Fact]
		public static void SectionKeyValueList()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf))
			{
				WriteSectionToken st = scw.WriteKeyOpenSection(CfgKey.Create("Section"));
				WriteValueListToken vl = scw.WriteKeyOpenValueList(CfgKey.Create("Key1"));
				vl.WriteListValue("Value1");
				vl.WriteListValue("#Value 2");
				vl.WriteListValue("Value  3");
				vl.Close();
				st.Close();
			}
			Assert.Equal("Section {\n\tKey1: {\n\t\tValue1\n\t\t\"#Value 2\"\n\t\tValue  3\n\t}\n}\n", sw.ToString());
		}
		[Fact]
		public static void DoubleQuotesIfNeededAutoMultiline()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf, quoting: Quoting.DoubleIfNeeded, multiline: Multiline.Auto))
			{
				scw.WriteKeyValue(key, "Value");
				scw.WriteKeyValue(key, "Value\nValue\nValue");
			}
			Assert.Equal("Key: Value\nKey:\n\tValue\n\tValue\n\tValue\n", sw.ToString());
		}
		[Fact]
		public static void DoubleQuotesIfNeededNeverMultiline()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf, quoting: Quoting.DoubleIfNeeded, multiline: Multiline.Never))
			{
				scw.WriteKeyValue(key, "Value");
				scw.WriteKeyValue(key, "Value\nValue\nValue");
			}
			Assert.Equal("Key: Value\nKey: \"Value\nValue\nValue\"\n", sw.ToString());
		}
		[Fact]
		public static void DoubleQuotesIfNeededAlwaysMultiline()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf, quoting: Quoting.DoubleIfNeeded, multiline: Multiline.AlwaysIfPossible))
			{
				scw.WriteKeyValue(key, "Value");
				scw.WriteKeyValue(key, "Value\nValue\nValue");
			}
			Assert.Equal("Key:\n\tValue\nKey:\n\tValue\n\tValue\n\tValue\n", sw.ToString());
		}
		[Fact]
		public static void SingleQuotesIfNeededAutoMultiline()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf, quoting: Quoting.SingleIfNeeded, multiline: Multiline.Auto))
			{
				scw.WriteKeyValue(key, "Value");
				scw.WriteKeyValue(key, "Value\nValue\nValue");
			}
			Assert.Equal("Key: Value\nKey:\n\tValue\n\tValue\n\tValue\n", sw.ToString());
		}
		[Fact]
		public static void SingleQuotesIfNeededNeverMultiline()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf, quoting: Quoting.SingleIfNeeded, multiline: Multiline.Never))
			{
				scw.WriteKeyValue(key, "Value");
				scw.WriteKeyValue(key, "Value\nValue\nValue");
			}
			Assert.Equal("Key: Value\nKey: 'Value\nValue\nValue'\n", sw.ToString());
		}
		[Fact]
		public static void SingleQuotesIfNeededAlwaysMultiline()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf, quoting: Quoting.SingleIfNeeded, multiline: Multiline.AlwaysIfPossible))
			{
				scw.WriteKeyValue(key, "Value");
				scw.WriteKeyValue(key, "Value\nValue\nValue");
			}
			Assert.Equal("Key:\n\tValue\nKey:\n\tValue\n\tValue\n\tValue\n", sw.ToString());
		}
		[Fact]
		public static void AlwaysDoubleQuotesAutoMultiline()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf, quoting: Quoting.AlwaysDouble, multiline: Multiline.Auto))
			{
				scw.WriteKeyValue(key, "Value");
				scw.WriteKeyValue(key, "Value\nValue\nValue");
			}
			Assert.Equal("Key: \"Value\"\nKey: \"Value\nValue\nValue\"\n", sw.ToString());
		}
		[Fact]
		public static void AlwaysDoubleQuotesNeverMultiline()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf, quoting: Quoting.AlwaysDouble, multiline: Multiline.Never))
			{
				scw.WriteKeyValue(key, "Value");
				scw.WriteKeyValue(key, "Value\nValue\nValue");
			}
			Assert.Equal("Key: \"Value\"\nKey: \"Value\nValue\nValue\"\n", sw.ToString());
		}
		[Fact]
		public static void AlwaysDoubleQuotesAlwaysMultiline()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf, quoting: Quoting.AlwaysDouble, multiline: Multiline.AlwaysIfPossible))
			{
				scw.WriteKeyValue(key, "Value");
				scw.WriteKeyValue(key, "Value\nValue\nValue");
			}
			Assert.Equal("Key: \"Value\"\nKey: \"Value\nValue\nValue\"\n", sw.ToString());
		}
		[Fact]
		public static void AlwaysSingleQuotesAutoMultiline()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf, quoting: Quoting.AlwaysSingle, multiline: Multiline.Auto))
			{
				scw.WriteKeyValue(key, "Value");
				scw.WriteKeyValue(key, "Value\nValue\nValue");
			}
			Assert.Equal("Key: 'Value'\nKey: 'Value\nValue\nValue'\n", sw.ToString());
		}
		[Fact]
		public static void AlwaysSingleQuotesNeverMultiline()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf, quoting: Quoting.AlwaysSingle, multiline: Multiline.Never))
			{
				scw.WriteKeyValue(key, "Value");
				scw.WriteKeyValue(key, "Value\nValue\nValue");
			}
			Assert.Equal("Key: 'Value'\nKey: 'Value\nValue\nValue'\n", sw.ToString());
		}
		[Fact]
		public static void AlwaysSingleQuotesAlwaysMultiline()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf, quoting: Quoting.AlwaysSingle, multiline: Multiline.AlwaysIfPossible))
			{
				scw.WriteKeyValue(key, "Value");
				scw.WriteKeyValue(key, "Value\nValue\nValue");
			}
			Assert.Equal("Key: 'Value'\nKey: 'Value\nValue\nValue'\n", sw.ToString());
		}
		[Fact]
		public static void SingleLineComment()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf, quoting: Quoting.SingleIfNeeded, multiline: Multiline.Auto))
			{
				scw.WriteComment("Hello world");
				scw.WriteKey(key);
				scw.WriteValue("Value");
			}
			Assert.Equal("#Hello world\nKey: Value\n", sw.ToString());
		}
		[Fact]
		public static void MultiLineComment()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf, quoting: Quoting.SingleIfNeeded, multiline: Multiline.Auto))
			{
				scw.WriteComment("Hello world\nHello world 2\nLine 3");
				scw.WriteKey(key);
				scw.WriteValue("Value");
			}
			Assert.Equal("#Hello world\n#Hello world 2\n#Line 3\nKey: Value\n", sw.ToString());
		}
		[Fact]
		public static void MultiLineCommentReplaceLineBreaks()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf, quoting: Quoting.SingleIfNeeded, multiline: Multiline.Auto))
			{
				scw.WriteComment("Hello world\nHello world 2\r\nLine 3", replaceLineBreaks: true);
				scw.WriteKey(key);
				scw.WriteValue("Value");
			}
			Assert.Equal("#Hello world\n#Hello world 2\n#Line 3\nKey: Value\n", sw.ToString());
		}
		[Fact]
		public static void MultiLineCommentKeepLineBreaks()
		{
			using StringWriter sw = new();
			using (SectionCfgWriter scw = new(sw, newLine: NewLine.Lf, quoting: Quoting.SingleIfNeeded, multiline: Multiline.Auto))
			{
				scw.WriteComment("Hello world\nHello world 2\r\nLine 3", replaceLineBreaks: false);
				scw.WriteKey(key);
				scw.WriteValue("Value");
			}
			Assert.Equal("#Hello world\n#Hello world 2\r\n#Line 3\nKey: Value\n", sw.ToString());
		}
	}
}
