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
# Some comment   
	Key3: 'Blah'
Key 4:
	This is a multiline value
	It will just keep going
	Until we find lesser indentation
		This is still part of the string
	Done

	Key 5:
		Aligned, still
		a multiline value.
Section{
	Key:
		'Also a multiline string
		It just keeps going too'

}
List:{
	String 1
	""String 2""
	# A comment
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
				CfgKey s1 = CfgKey.Create("Section");
				CfgKey l1 = CfgKey.Create("List");
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.Value, CfgKey.Create("Key 1"), "Blah"),
					new(SectionCfgToken.Value, CfgKey.Create("Key 2"), "Blah"),
					new(SectionCfgToken.Comment, default, " Some comment   "),
					new(SectionCfgToken.Value, CfgKey.Create("Key3"), "Blah"),
					new(SectionCfgToken.Value, CfgKey.Create("Key 4"), "This is a multiline value\nIt will just keep going\nUntil we find lesser indentation\n	This is still part of the string\nDone"),
					new(SectionCfgToken.Value, CfgKey.Create("Key 5"), "Aligned, still\na multiline value."),
					new(SectionCfgToken.StartSection, s1),
					new(SectionCfgToken.Value, CfgKey.Create("Key"), "Also a multiline string\n\t\tIt just keeps going too"),
					new(SectionCfgToken.EndSection, s1),
					new(SectionCfgToken.StartList, l1),
					new(SectionCfgToken.ListValue, l1, "String 1"),
					new(SectionCfgToken.ListValue, l1, "String 2"),
					new(SectionCfgToken.Comment, default, " A comment"),
					new(SectionCfgToken.ListValue, l1, "String 3"),
					new(SectionCfgToken.ListValue, l1, "String\n\t4"),
					new(SectionCfgToken.ListValue, l1, "String\n\t5"),
					new(SectionCfgToken.EndList, l1),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Assert.Equal(7, root.Elements.Count);
				Helper.AssertKeyValues(root,
						new("Key 1", "Blah"),
						new("Key 2", "Blah"),
						new("Key3", "Blah"),
						new("Key 4", "This is a multiline value\nIt will just keep going\nUntil we find lesser indentation\n	This is still part of the string\nDone")
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
					new(SectionCfgToken.Value, CfgKey.Create("Key"), ""),
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
		public static void EmptyValueAndMultilineValue()
		{
			string s = "Key1:\nKey2:\n\tvalue\n\n\tKey3:\n\tKey4:\n\t\tvalue\n";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.Value, CfgKey.Create("Key1"), ""),
					new(SectionCfgToken.Value, CfgKey.Create("Key2"), "value"),
					new(SectionCfgToken.Value, CfgKey.Create("Key3"), ""),
					new(SectionCfgToken.Value, CfgKey.Create("Key4"), "value"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key1", ""),
					new KeyValuePair<string, string>("Key2", "value"),
					new KeyValuePair<string, string>("Key3", ""),
					new KeyValuePair<string, string>("Key4", "value")
				);
			}
		}
		[Fact]
		public static void ManyEmptyKeys()
		{
			string s = "\tKey1:\n\tKey2:\n\tKey3:\nKey4:\nKey5:\nKey6:\n";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.Value, CfgKey.Create("Key1"), ""),
					new(SectionCfgToken.Value, CfgKey.Create("Key2"), ""),
					new(SectionCfgToken.Value, CfgKey.Create("Key3"), ""),
					new(SectionCfgToken.Value, CfgKey.Create("Key4"), ""),
					new(SectionCfgToken.Value, CfgKey.Create("Key5"), ""),
					new(SectionCfgToken.Value, CfgKey.Create("Key6"), ""),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key1", ""),
					new KeyValuePair<string, string>("Key2", ""),
					new KeyValuePair<string, string>("Key3", ""),
					new KeyValuePair<string, string>("Key4", ""),
					new KeyValuePair<string, string>("Key5", ""),
					new KeyValuePair<string, string>("Key6", "")
				);
			}
		}
		[Fact]
		public static void MultilineNotTreatedAsKey()
		{
			string s = "Key1:\n\tNotAKey:\n";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.Value, CfgKey.Create("Key1"), "NotAKey:"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key1", "NotAKey:")
				);
			}
		}
		[Fact]
		public static void EmptyValueDecreasingIndentation()
		{
			string s = "\t\t\tKey1:\n\t\tKey2:\n\tKey3:\nKey4:\nKey5:\n";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.Value, CfgKey.Create("Key1"), ""),
					new(SectionCfgToken.Value, CfgKey.Create("Key2"), ""),
					new(SectionCfgToken.Value, CfgKey.Create("Key3"), ""),
					new(SectionCfgToken.Value, CfgKey.Create("Key4"), ""),
					new(SectionCfgToken.Value, CfgKey.Create("Key5"), ""),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key1", ""),
					new KeyValuePair<string, string>("Key2", ""),
					new KeyValuePair<string, string>("Key3", ""),
					new KeyValuePair<string, string>("Key4", ""),
					new KeyValuePair<string, string>("Key5", "")
				);
			}
		}
		[Fact]
		public static void IncreasingIndentationMultiline()
		{
			string s = "Key:\n\tThis value:\n\tis not a key!";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.Value, CfgKey.Create("Key"), "This value:\nis not a key!"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "This value:\nis not a key!")
				);
			}
		}
		[Fact]
		public static void EmptyValueIncreasingIndentationThenMultiline()
		{
			string s = "Key1:\nKey2:\n\tThis value:\n\tis not a key!";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.Value, CfgKey.Create("Key1"), ""),
					new(SectionCfgToken.Value, CfgKey.Create("Key2"), "This value:\nis not a key!"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key1", ""),
					new KeyValuePair<string, string>("Key2", "This value:\nis not a key!")
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
					new(SectionCfgToken.Value, CfgKey.Create("Key"), ""),
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
			string s = "\nKey:\"\"";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.Value, CfgKey.Create("Key"), ""),
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
		public static void KeyQuotedValueThenKeyUnquoted()
		{
			string s = "Key1:'Value1'\nKey2:Value2";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.Value, CfgKey.Create("Key1"), "Value1"),
					new(SectionCfgToken.Value, CfgKey.Create("Key2"), "Value2"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key1", "Value1"),
					new KeyValuePair<string, string>("Key2", "Value2")
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
					new(SectionCfgToken.Comment, default, ""),
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
					new(SectionCfgToken.Value, CfgKey.Create("Key"), "\""),
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
					new(SectionCfgToken.Value, CfgKey.Create("Key"), "'"),
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
					new(SectionCfgToken.Value, CfgKey.Create("Key"), "Value"),
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
					new(SectionCfgToken.Value, CfgKey.Create("Key"), "Value"),
					new(SectionCfgToken.Comment, default, "Explanation"),
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
		public static void KeyValueStartingWithNumberSign()
		{
			string s = "Key:#This is a very important string!";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.Value, CfgKey.Create("Key"), "#This is a very important string!"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "#This is a very important string!")
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
					new(SectionCfgToken.Value, CfgKey.Create("Key"), "Value"),
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
					new(SectionCfgToken.Value, CfgKey.Create("Key"), "'Value's all good!'"),
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
					new(SectionCfgToken.Value, CfgKey.Create("Key"), "\"Value \"is\" all good!\""),
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
					new(SectionCfgToken.Value, CfgKey.Create("Key 1"), "Value1"),
					new(SectionCfgToken.Value, CfgKey.Create("Key 2"), "Value 2"),
					new(SectionCfgToken.Value, CfgKey.Create("Key 3"), "Value 3"),
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
			string s = "Key:\n\tThis value\n\tspans many lines";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.Value, CfgKey.Create("Key"), "This value\nspans many lines"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "This value\nspans many lines")
				);
			}
		}
		[Fact]
		public static void MultilineUnquotedValueLfTrailingNewline()
		{
			// Trailing newlines are removed
			string s = "Key:\n\tThis value\n\tspans many lines\n";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.Value, CfgKey.Create("Key"), "This value\nspans many lines"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "This value\nspans many lines")
				);
			}
		}
		[Fact]
		public static void MultilineUnquotedValueLfTrailingNewlineAndTab()
		{
			// In a text editor, another line of indentation makes it appear as if the multiline value has a blank last line, hence why we should have a trailing newline
			string s = "Key:\n\tThis value\n\tspans many lines\n\t";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.Value, CfgKey.Create("Key"), "This value\nspans many lines\n"),
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
			string s = "Key:\r\n\tThis value\r\n\tspans many lines";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.Value, CfgKey.Create("Key"), "This value\r\nspans many lines"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "This value\r\nspans many lines")
				);
			}
		}
		[Fact]
		public static void MultilineUnquotedValueCrLfTrailingNewline()
		{
			string s = "Key:\r\n\tThis value\r\n\tspans many lines\r\n";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.Value, CfgKey.Create("Key"), "This value\r\nspans many lines"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "This value\r\nspans many lines")
				);
			}
		}
		/// <summary>
		/// This treats \r as any old character because we don't support just \r as newlines
		/// </summary>
		[Fact]
		public static void SinglelineUnquotedValueCr()
		{
			string s = "Key:\r\tThis value\r\tspans many lines";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.Value, CfgKey.Create("Key"), "This value\r\tspans many lines"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "This value\r\tspans many lines")
				);
			}
		}
		/// <summary>
		/// This treats \r as any old character because we don't support just \r as newlines
		/// </summary>
		[Fact]
		public static void SinglelineUnquotedValueCrTrailingCr()
		{
			string s = "Key:\r\tThis value\r\tspans many lines\r";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.Value, CfgKey.Create("Key"), "This value\r\tspans many lines\r"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "This value\r\tspans many lines\r")
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
					new(SectionCfgToken.Value, CfgKey.Create("Key"), "This value\nspans\r\tmany\r\nlines"),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				Helper.AssertKeyValues(root,
					new KeyValuePair<string, string>("Key", "This value\nspans\r\tmany\r\nlines")
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
					new(SectionCfgToken.Value, CfgKey.Create("Key"), "This value\nspans many\n\tlines"),
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
				CfgKey k1 = CfgKey.Create("Section");
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.StartSection, k1, string.Empty),
					new(SectionCfgToken.EndSection, k1, string.Empty),
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
				CfgKey s1 = CfgKey.Create("Section1");
				CfgKey s2 = CfgKey.Create("Section2");
				CfgKey s3 = CfgKey.Create("Section3");
				CfgKey s4 = CfgKey.Create("Section4");
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.StartSection, s1, string.Empty),
					new(SectionCfgToken.StartSection, s2, string.Empty),
					new(SectionCfgToken.StartSection, s3, string.Empty),
					new(SectionCfgToken.StartSection, s4, string.Empty),
					new(SectionCfgToken.EndSection, s4, string.Empty),
					new(SectionCfgToken.EndSection, s3, string.Empty),
					new(SectionCfgToken.EndSection, s2, string.Empty),
					new(SectionCfgToken.EndSection, s1, string.Empty),
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
				CfgKey s1 = CfgKey.Create("Section");
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.StartSection, s1, string.Empty),
					new(SectionCfgToken.Value, CfgKey.Create("Key1"), "Value1"),
					new(SectionCfgToken.Value, CfgKey.Create("Key2"), "Value2"),
					new(SectionCfgToken.Value, CfgKey.Create("Key3"), "Value3"),
					new(SectionCfgToken.EndSection, s1, string.Empty),
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
			string s = "Section{Key :{One\n#Comment\nTwo\n'Three\nThree'#CommentAgain\nHeyyyy\n}}";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgKey s1 = CfgKey.Create("Section");
				CfgKey l1 = CfgKey.Create("Key");
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.StartSection, s1, string.Empty),
					new(SectionCfgToken.StartList, l1, string.Empty),
					new(SectionCfgToken.ListValue, l1, "One"),
					new(SectionCfgToken.Comment, default, "Comment"),
					new(SectionCfgToken.ListValue, l1, "Two"),
					new(SectionCfgToken.ListValue, l1, "Three\nThree"),
					new(SectionCfgToken.Comment, default, "CommentAgain"),
					new(SectionCfgToken.ListValue, l1, "Heyyyy"),
					new(SectionCfgToken.EndList, l1, string.Empty),
					new(SectionCfgToken.EndSection, s1, string.Empty),
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
				CfgKey l1 = CfgKey.Create("List");
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.StartList, l1),
					new(SectionCfgToken.ListValue, l1, "One"),
					new(SectionCfgToken.ListValue, l1, "Two"),
					new(SectionCfgToken.ListValue, l1, "Three"),
					new(SectionCfgToken.ListValue, l1, "Four"),
					new(SectionCfgToken.EndList, l1, string.Empty),
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
			string s = "List:{\"One\" \"One\" \"'Two'\" 'Three' '\"Four\"'}";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgKey l1 = CfgKey.Create("List");
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.StartList, l1),
					new(SectionCfgToken.ListValue, l1, "One"),
					new(SectionCfgToken.ListValue, l1, "One"),
					new(SectionCfgToken.ListValue, l1, "'Two'"),
					new(SectionCfgToken.ListValue, l1, "Three"),
					new(SectionCfgToken.ListValue, l1, "\"Four\""),
					new(SectionCfgToken.EndList, l1),
					new(SectionCfgToken.End),
				});
			}
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgRoot root = Helper.LoadsProperly(scr, StringComparer.Ordinal);
				CfgValueList list = Assert.IsType<CfgValueList>(root.Elements["List"]);
				Assert.Collection(list.Values,
					x => Assert.Equal("One", x),
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
				"\tTwo\n" +
				"\t\t'Three\n" +
				"\t\tThree'\n" +
				"    \"Four\n" +
				"    Four\"}}";
			using (SectionCfgReader scr = new(new StringReader(s)))
			{
				CfgKey s1 = CfgKey.Create("Section");
				CfgKey l1 = CfgKey.Create("List");
				Helper.AssertReadMatches(scr, new ReadResult[]
				{
					new(SectionCfgToken.StartSection, s1),
					new(SectionCfgToken.StartList, l1),
					new(SectionCfgToken.ListValue, l1, "One"),
					new(SectionCfgToken.ListValue, l1, "Two"),
					new(SectionCfgToken.ListValue, l1, "Three\n\t\tThree"),
					new(SectionCfgToken.ListValue, l1, "Four\n    Four"),
					new(SectionCfgToken.EndList, l1),
					new(SectionCfgToken.EndSection, s1),
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
