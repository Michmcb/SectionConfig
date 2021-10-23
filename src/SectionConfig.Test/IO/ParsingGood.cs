namespace SectionConfig.Test.IO
{
	using SectionConfig.IO;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using Xunit;
	public static class ParsingGood
	{
		[Fact]
		public static void GeneralExmaple()
		{
			string s = @"
Key 1:     Blah    
Key 2: 'Blah'
	Key3: 'Blah'
Key 4: #Comment
	This is a multiline value
	It will just keep going
	Until we find lesser indentation
		This is still part of the string
	Done
Section{
	Key:
		'Also a multiline string
		It just keeps going too'

}
List:{
	String 1
	""String 2""
	'String 3'
	'String
	4'
	""String
	5""
}
";
			// TODO have to decide how we're going to have the ability to have a multiline string within a string list

			// This is how YAML does it. Basically each string is an item in a sequence, and the multiline string specifies > (multiline string). it's >- cause the library meant "block chomping".
			/*
			Strings:
				- Hello world
				- >-
					Hello

					world

			 */

			// So in theory we can do a similar thing. Exactly like our key/value pairs. If there's a colon then just read the string until the end of the line.
			// And if there's a colon with nothing significant after it, then read multiline text until we hit different indentation. In other words, exactly the same as key/values.
			/*
			 	: String 6
				: 'String 7'
				: String
			 */
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Key 1")),
					new(SectionCfgToken.Value, "Blah"),
					new(CfgKey.Create("Key 2")),
					new(SectionCfgToken.Value, "Blah"),
					new(CfgKey.Create("Key3")),
					new(SectionCfgToken.Value, "Blah"),
					new(CfgKey.Create("Key 4")),
					new(SectionCfgToken.Comment, "Comment"),
					new(SectionCfgToken.Value, "This is a multiline value\nIt will just keep going\nUntil we find lesser indentation\n	This is still part of the string\nDone\n"),
					new(CfgKey.Create("Section")),
					new(SectionCfgToken.StartSection),
					new(CfgKey.Create("Key")),
					new(SectionCfgToken.Value, "Also a multiline string\n\t\tIt just keeps going too"),
					new(SectionCfgToken.EndSection),
					new(CfgKey.Create("List")),
					new(SectionCfgToken.StartList),
					new(SectionCfgToken.ListValue, "String 1"),
					new(SectionCfgToken.ListValue, "String 2"),
					new(SectionCfgToken.ListValue, "String 3"),
					new(SectionCfgToken.ListValue, "String\n\t4"),
					new(SectionCfgToken.ListValue, "String\n\t5"),
					new(SectionCfgToken.EndList),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Assert.Equal(6, root.Elements.Count);
				Helper.AssertKeyValues(root,
						new("Key 1", "Blah"),
						new("Key 2", "Blah"),
						new("Key3", "Blah"),
						new("Key 4", "This is a multiline value\nIt will just keep going\nUntil we find lesser indentation\n	This is still part of the string\nDone\n")
					);
				CfgSection section = Assert.IsType<CfgSection>(root.Elements["Section"]);
				Helper.AssertKeyValues(section,
						new KeyValuePair<string, string>("Key", "Also a multiline string\n\t\tIt just keeps going too")
					);
				CfgValueList list = Assert.IsType<CfgValueList>(root.Elements["List"]);
				Assert.Collection(list.Values,
					x => Assert.Equal("String 1", x),
					x => Assert.Equal("String 2", x),
					x => Assert.Equal("String 3", x),
					x => Assert.Equal("String\n\t4", x),
					x => Assert.Equal("String\n\t5", x));
			}
		}
		[Fact]
		public static void Empty()
		{
			string s = "";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Assert.Empty(root.Elements.Values);
			}
		}
		[Fact]
		public static void EmptyValue()
		{
			string s = "Key:";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Key")),
					new(SectionCfgToken.Value, ""),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "")
				);
			}
		}
		[Fact]
		public static void EmptyValueSingleQuoted()
		{
			string s = "Key:''";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Key")),
					new(SectionCfgToken.Value, ""),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "")
				);
			}
		}
		[Fact]
		public static void EmptyValueDoubleQuoted()
		{
			string s = "Key:\"\"";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Key")),
					new(SectionCfgToken.Value, ""),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "")
				);
			}
		}
		[Fact]
		public static void KeyCommentEmptyValue()
		{
			string s = "Key:#Comment";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Key")),
					new(SectionCfgToken.Comment, "Comment"),
					new(SectionCfgToken.Value, ""),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "")
				);
			}
		}
		[Fact]
		public static void EmptyComment()
		{
			string s = "#";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.Comment, ""),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Assert.Empty(root.Elements.Values);
			}
		}
		[Fact]
		public static void EscapedDoubleQuote()
		{
			string s = "Key:\"\"\"\"";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Key")),
					new(SectionCfgToken.Value, "\""),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "\"")
				);
			}
		}
		[Fact]
		public static void EscapedSingleQuote()
		{
			string s = "Key:''''";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Key")),
					new(SectionCfgToken.Value, "'"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "'")
				);
			}
		}
		[Fact]
		public static void KeyValue()
		{
			string s = "Key:Value";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Key")),
					new(SectionCfgToken.Value, "Value"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "Value")
				);
			}
		}
		[Fact]
		public static void KeyValueComment()
		{
			string s = "Key:'Value' #Explanation";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Key")),
					new(SectionCfgToken.Value, "Value"),
					new(SectionCfgToken.Comment, "Explanation"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "Value")
				);
			}
		}
		[Fact]
		public static void KeyCommentValue()
		{
			string s = "Key:#This is a very important string!\nValue";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Key")),
					new(SectionCfgToken.Comment, "This is a very important string!"),
					new(SectionCfgToken.Value, "Value"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "Value")
				);
			}
		}
		[Fact]
		public static void KeyValueNoWhitespace()
		{
			string s = "   Key		: Value	";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Key")),
					new(SectionCfgToken.Value, "Value"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "Value")
				);
			}
		}
		[Fact]
		public static void KeyValueDoubleQuoted()
		{
			string s = "Key:\"'Value's all good!'\"";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Key")),
					new(SectionCfgToken.Value, "'Value's all good!'"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "'Value's all good!'")
				);
			}
		}
		[Fact]
		public static void KeyValueSingleQuoted()
		{
			string s = "Key:'\"Value \"is\" all good!\"'";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Key")),
					new(SectionCfgToken.Value, "\"Value \"is\" all good!\""),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "\"Value \"is\" all good!\"")
				);
			}
		}
		[Fact]
		public static void ManyKeyValues()
		{
			string s = "Key 1:\tValue1\nKey 2:\t'Value 2'\n Key 3 : Value 3\n";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Key 1")),
					new(SectionCfgToken.Value, "Value1"),
					new(CfgKey.Create("Key 2")),
					new(SectionCfgToken.Value, "Value 2"),
					new(CfgKey.Create("Key 3")),
					new(SectionCfgToken.Value, "Value 3"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key 1", "Value1"),
					new KeyValuePair<string, string>("Key 2", "Value 2"),
					new KeyValuePair<string, string>("Key 3", "Value 3")
				);
			}
		}
		[Fact]
		public static void MultilineUnquotedValueLf()
		{
			string s = "Key:\n\tThis value\n\tspans many lines\n";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Key")),
					new(SectionCfgToken.Value, "This value\nspans many lines\n"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "This value\nspans many lines\n")
				);
			}
		}
		[Fact]
		public static void MultilineUnquotedValueCrLf()
		{
			string s = "Key:\r\n\tThis value\r\n\tspans many lines\r\n";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Key")),
					new(SectionCfgToken.Value, "This value\r\nspans many lines\r\n"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "This value\r\nspans many lines\r\n")
				);
			}
		}
		[Fact]
		public static void MultilineUnquotedValueCr()
		{
			string s = "Key:\r\tThis value\r\tspans many lines\r";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Key")),
					new(SectionCfgToken.Value, "This value\rspans many lines\r"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "This value\rspans many lines\r")
				);
			}
		}
		[Fact]
		public static void MultilineUnquotedValueMixed()
		{
			string s = "Key:\n\tThis value\n\tspans\r\tmany\r\n\tlines\n";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Key")),
					new(SectionCfgToken.Value, "This value\nspans\rmany\r\nlines\n"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "This value\nspans\rmany\r\nlines\n")
				);
			}
		}
		[Fact]
		public static void MultilineQuotedValue()
		{
			string s = "Key:\n\"This value\nspans many\n\tlines\"";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Key")),
					new(SectionCfgToken.Value, "This value\nspans many\n\tlines"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "This value\nspans many\n\tlines")
				);
			}
		}
		[Fact]
		public static void Section()
		{
			string s = "Section{}";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Section")),
					new(SectionCfgToken.StartSection),
					new(SectionCfgToken.EndSection),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				CfgSection section = Assert.IsType<CfgSection>(root.Elements["Section"]);
				Assert.Empty(section.Elements);
			}
		}
		[Fact]
		public static void NestedSections()
		{
			string s = "Section1{Section2{Section3{Section4{}}}}";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Section1")),
					new(SectionCfgToken.StartSection),
					new(CfgKey.Create("Section2")),
					new(SectionCfgToken.StartSection),
					new(CfgKey.Create("Section3")),
					new(SectionCfgToken.StartSection),
					new(CfgKey.Create("Section4")),
					new(SectionCfgToken.StartSection),
					new(SectionCfgToken.EndSection),
					new(SectionCfgToken.EndSection),
					new(SectionCfgToken.EndSection),
					new(SectionCfgToken.EndSection),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				CfgSection section1 = Assert.IsType<CfgSection>(root.Elements["Section1"]);
				CfgSection section2 = Assert.IsType<CfgSection>(section1.Elements["Section2"]);
				CfgSection section3 = Assert.IsType<CfgSection>(section2.Elements["Section3"]);
				CfgSection section4 = Assert.IsType<CfgSection>(section3.Elements["Section4"]);
				Assert.Equal(1, section1.Elements.Count);
				Assert.Equal(1, section2.Elements.Count);
				Assert.Equal(1, section3.Elements.Count);
				Assert.Empty(section4.Elements);
			}
		}
		[Fact]
		public static void SectionValue()
		{
			string s = "Section{Key1:Value1\nKey2:Value2\nKey3:Value3\n}";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Section")),
					new(SectionCfgToken.StartSection),
					new(CfgKey.Create("Key1")),
					new(SectionCfgToken.Value, "Value1"),
					new(CfgKey.Create("Key2")),
					new(SectionCfgToken.Value, "Value2"),
					new(CfgKey.Create("Key3")),
					new(SectionCfgToken.Value, "Value3"),
					new(SectionCfgToken.EndSection),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				CfgSection section = Assert.IsType<CfgSection>(root.Elements["Section"]);
				Helper.AssertKeyValues(section,
					new("Key1", "Value1"),
					new("Key2", "Value2"),
					new("Key3", "Value3")
				);
			}
		}
		[Fact]
		public static void List()
		{
			string s = "Section{Key:{One\n#Comment\nTwo\n'Three\nThree'#CommentAgain\nHeyyyy\n}}";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Section")),
					new(SectionCfgToken.StartSection),
					new(CfgKey.Create("Key")),
					new(SectionCfgToken.StartList),
					new(SectionCfgToken.ListValue, "One"),
					new(SectionCfgToken.Comment, "Comment"),
					new(SectionCfgToken.ListValue, "Two"),
					new(SectionCfgToken.ListValue, "Three\nThree"),
					new(SectionCfgToken.Comment, "CommentAgain"),
					new(SectionCfgToken.ListValue, "Heyyyy"),
					new(SectionCfgToken.EndList),
					new(SectionCfgToken.EndSection),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				CfgSection section = Assert.IsType<CfgSection>(root.Elements["Section"]);
				CfgValueList list = Assert.IsType<CfgValueList>(section.Elements["Key"]);
				Assert.Collection(list.Values,
					x => Assert.Equal("One", x),
					x => Assert.Equal("Two", x),
					x => Assert.Equal("Three\nThree", x),
					x => Assert.Equal("Heyyyy", x));
			}
		}
		[Fact]
		public static void QuotedListNoSpaces()
		{
			string s = "List:{\"One\"'Two'\"Three\"'Four'}";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("List")),
					new(SectionCfgToken.StartList),
					new(SectionCfgToken.ListValue, "One"),
					new(SectionCfgToken.ListValue, "Two"),
					new(SectionCfgToken.ListValue, "Three"),
					new(SectionCfgToken.ListValue, "Four"),
					new(SectionCfgToken.EndList),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				CfgValueList list = Assert.IsType<CfgValueList>(root.Elements["List"]);
				Assert.Collection(list.Values,
					x => Assert.Equal("One", x),
					x => Assert.Equal("Two", x),
					x => Assert.Equal("Three", x),
					x => Assert.Equal("Four", x));
			}
		}
		[Fact]
		public static void QuotedListSpaces()
		{
			string s = "List:{\"One\" \"'Two'\" 'Three' '\"Four\"'}";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("List")),
					new(SectionCfgToken.StartList),
					new(SectionCfgToken.ListValue, "One"),
					new(SectionCfgToken.ListValue, "'Two'"),
					new(SectionCfgToken.ListValue, "Three"),
					new(SectionCfgToken.ListValue, "\"Four\""),
					new(SectionCfgToken.EndList),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				CfgValueList list = Assert.IsType<CfgValueList>(root.Elements["List"]);
				Assert.Collection(list.Values,
					x => Assert.Equal("One", x),
					x => Assert.Equal("'Two'", x),
					x => Assert.Equal("Three", x),
					x => Assert.Equal("\"Four\"", x));
			}
		}
		[Fact]
		public static void ListIndentedMultilineQuotedText()
		{
			string s = "Section{\tList:{\n" +
				"\t\tOne\n" +
				"\t\tTwo\n" +
				"\t\t'Three\n" +
				"\t\tThree'\n" +
				"    \"Four\n" +
				"    Four\"}}";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(CfgKey.Create("Section")),
					new(SectionCfgToken.StartSection),
					new(CfgKey.Create("List")),
					new(SectionCfgToken.StartList),
					new(SectionCfgToken.ListValue, "One"),
					new(SectionCfgToken.ListValue, "Two"),
					new(SectionCfgToken.ListValue, "Three\n\t\tThree"),
					new(SectionCfgToken.ListValue, "Four\n    Four"),
					new(SectionCfgToken.EndList),
					new(SectionCfgToken.EndSection),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				CfgSection section = Assert.IsType<CfgSection>(root.Elements["Section"]);
				CfgValueList list = Assert.IsType<CfgValueList>(section.Elements["List"]);
				Assert.Collection(list.Values,
					x => Assert.Equal("One", x),
					x => Assert.Equal("Two", x),
					x => Assert.Equal("Three\n\t\tThree", x),
					x => Assert.Equal("Four\n    Four", x));
			}
		}
	}
}
