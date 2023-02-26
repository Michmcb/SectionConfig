namespace SectionConfig.Test.IO
{
	using SectionConfig.IO;
	using System.Threading.Tasks;
	using Xunit;
	public static class ParsingGood
	{
		private static readonly CfgKey key = CfgKey.Create("Key");
		private static readonly CfgKey key1 = CfgKey.Create("Key1");
		private static readonly CfgKey key2 = CfgKey.Create("Key2");
		private static readonly CfgKey key3 = CfgKey.Create("Key3");
		private static readonly CfgKey key4 = CfgKey.Create("Key4");
		private static readonly CfgKey key5 = CfgKey.Create("Key5");
		private static readonly CfgKey key6 = CfgKey.Create("Key6");
		[Fact]
		public static async Task GeneralExample()
		{
			string s = "\n" +
"Key 1:     Blah    \n" +
"Key 2: 'Blah'\n" +
"# Some comment   \n" +
"	Key3: 'Blah'\n" +
"Key 4:\n" +
"	This is a multiline value\n" +
"	It will just keep going\n" +
"	Until we find lesser indentation\n" +
"		This is still part of the string\n" +
"	Done\n" +
"\n" +
"	Key 5:\n" +
"		Aligned, still\n" +
"		a multiline value.\n" +
"Section{\n" +
"	Key:\n" +
"\t\t'A quoted string\n" +
"\t\tIt just keeps going too'\n" +
"\n" +
"}\n" +
"List:{\n" +
"	String 1\n" +
"	\"String 2\"\n" +
"	# A comment\n" +
"	'String 3'\n" +
"	\n" +
"	'String\n" +
"	4'\n" +
"	\"String\n" +
"	5\"\n" +
"}\n";
			// TODO have to decide how we're going to have the ability to have a multiline string within a string list. See below for how YAML does it. When/if we implement this, SectionCfgWriter has to be able to write multilines properly, too.

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

			CfgKey sectionKey = CfgKey.Create("Section");
			CfgKey listKey = CfgKey.Create("List");
			CfgKey key_1 = CfgKey.Create("Key 1");
			CfgKey key_2 = CfgKey.Create("Key 2");
			CfgKey key_4 = CfgKey.Create("Key 4");
			CfgKey key_5 = CfgKey.Create("Key 5");
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key_1),
				new(CfgBufToken.Value, key_1, "Blah"),
				new(CfgBufToken.Key, key_2),
				new(CfgBufToken.Value, key_2, "Blah"),
				new(CfgBufToken.Comment, key_2, " Some comment   "),
				new(CfgBufToken.Key, key3),
				new(CfgBufToken.Value, key3, "Blah"),
				new(CfgBufToken.Key, key_4),
				new(CfgBufToken.StartMultiline, key_4),
				new(CfgBufToken.Value, key_4, "This is a multiline value"),
				new(CfgBufToken.Value, key_4, "It will just keep going", "\n"),
				new(CfgBufToken.Value, key_4, "Until we find lesser indentation", "\n"),
				new(CfgBufToken.Value, key_4, "\tThis is still part of the string", "\n"),
				new(CfgBufToken.Value, key_4, "Done", "\n"),
				new(CfgBufToken.EndMultiline, key_4, "", "\n"),
				new(CfgBufToken.Key, key_5),
				new(CfgBufToken.StartMultiline, key_5),
				new(CfgBufToken.Value, key_5, "Aligned, still"),
				new(CfgBufToken.Value, key_5, "a multiline value.", "\n"),
				new(CfgBufToken.EndMultiline, key_5, "", "\n"),
				new(CfgBufToken.Key, sectionKey),
				new(CfgBufToken.StartSection, sectionKey),
				new(CfgBufToken.Key, key),
				new(CfgBufToken.Value, key, "A quoted string\n\t\tIt just keeps going too"),
				new(CfgBufToken.EndSection, sectionKey),
				new(CfgBufToken.Key, listKey),
				new(CfgBufToken.StartList, listKey),
				new(CfgBufToken.Value, listKey, "String 1"),
				new(CfgBufToken.Value, listKey, "String 2"),
				new(CfgBufToken.Comment, listKey, " A comment"),
				new(CfgBufToken.Value, listKey, "String 3"),
				new(CfgBufToken.Value, listKey, "String\n\t4"),
				new(CfgBufToken.Value, listKey, "String\n\t5"),
				new(CfgBufToken.EndList, listKey),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key_1, "Blah"),
				new(SectionCfgToken.Value, key_2, "Blah"),
				new(SectionCfgToken.Comment, key_2, " Some comment   "),
				new(SectionCfgToken.Value, key3, "Blah"),
				new(SectionCfgToken.Value, key_4, "This is a multiline value\nIt will just keep going\nUntil we find lesser indentation\n\tThis is still part of the string\nDone"),
				new(SectionCfgToken.Value, key_5, "Aligned, still\na multiline value."),
				new(SectionCfgToken.StartSection, sectionKey),
				new(SectionCfgToken.Value, key, "A quoted string\n\t\tIt just keeps going too"),
				new(SectionCfgToken.EndSection, sectionKey),
				new(SectionCfgToken.StartList, listKey),
				new(SectionCfgToken.ListValue, listKey, "String 1"),
				new(SectionCfgToken.ListValue, listKey, "String 2"),
				new(SectionCfgToken.Comment, listKey, " A comment"),
				new(SectionCfgToken.ListValue, listKey, "String 3"),
				new(SectionCfgToken.ListValue, listKey, "String\n\t4"),
				new(SectionCfgToken.ListValue, listKey, "String\n\t5"),
				new(SectionCfgToken.EndList, listKey),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task Empty()
		{
			string s = "";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new BufReadResult(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task EmptyValue()
		{
			string s = "Key:";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.Value, key),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task EmptyValues()
		{
			foreach (string s in new string[] {
				"Key:\n",
				"Key:\n\t",
				"Key:\n\t\n",
			})
			{
				Helper.TestCfgBufferReader(s, new BufReadResult[]
				{
					new(CfgBufToken.Key, key),
					new(CfgBufToken.Value, key),
					new(CfgBufToken.End),
				});
				await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key),
				new(SectionCfgToken.End),
			});
			}
		}
		[Fact]
		public static async Task EmptyValueAndMultilineValue()
		{
			string s = "Key1:\nKey2:\n\tvalue\n\n\tKey3:\n\tKey4:\n\t\tvalue\n";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key1),
				new(CfgBufToken.Value, key1),
				new(CfgBufToken.Key, key2),
				new(CfgBufToken.StartMultiline, key2),
				new(CfgBufToken.Value, key2, "value"),
				new(CfgBufToken.EndMultiline, key2, "", "\n"),
				new(CfgBufToken.Key, key3),
				new(CfgBufToken.Value, key3),
				new(CfgBufToken.Key, key4),
				new(CfgBufToken.StartMultiline, key4),
				new(CfgBufToken.Value, key4, "value"),
				new(CfgBufToken.EndMultiline, key4, "", "\n"),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key1),
				new(SectionCfgToken.Value, key2, "value"),
				new(SectionCfgToken.Value, key3),
				new(SectionCfgToken.Value, key4, "value"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task ManyEmptyKeys()
		{
			string s = "\tKey1:\n\tKey2:\n\tKey3:\nKey4:\nKey5:\nKey6:\n";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key1),
				new(CfgBufToken.Value, key1),
				new(CfgBufToken.Key, key2),
				new(CfgBufToken.Value, key2),
				new(CfgBufToken.Key, key3),
				new(CfgBufToken.Value, key3),
				new(CfgBufToken.Key, key4),
				new(CfgBufToken.Value, key4),
				new(CfgBufToken.Key, key5),
				new(CfgBufToken.Value, key5),
				new(CfgBufToken.Key, key6),
				new(CfgBufToken.Value, key6),
				new(CfgBufToken.End)
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key1),
				new(SectionCfgToken.Value, key2),
				new(SectionCfgToken.Value, key3),
				new(SectionCfgToken.Value, key4),
				new(SectionCfgToken.Value, key5),
				new(SectionCfgToken.Value, key6),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task MultilineNotTreatedAsKey()
		{
			string s = "Key1:\n\tNotAKey:\n";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key1),
				new(CfgBufToken.StartMultiline, key1),
				new(CfgBufToken.Value, key1, "NotAKey:"),
				new(CfgBufToken.EndMultiline, key1, "", "\n"),
				new(CfgBufToken.End)
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key1, "NotAKey:"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task EmptyValueDecreasingIndentation()
		{
			string s = "\t\t\tKey1:\n\t\tKey2:\n\tKey3:\nKey4:\nKey5:\n";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key1),
				new(CfgBufToken.Value, key1),
				new(CfgBufToken.Key, key2),
				new(CfgBufToken.Value, key2),
				new(CfgBufToken.Key, key3),
				new(CfgBufToken.Value, key3),
				new(CfgBufToken.Key, key4),
				new(CfgBufToken.Value, key4),
				new(CfgBufToken.Key, key5),
				new(CfgBufToken.Value, key5),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key1),
				new(SectionCfgToken.Value, key2),
				new(SectionCfgToken.Value, key3),
				new(SectionCfgToken.Value, key4),
				new(SectionCfgToken.Value, key5),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task IncreasingIndentationMultiline()
		{
			string s = "Key:\n\tThis value:\n\tis not a key!";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.StartMultiline, key),
				new(CfgBufToken.Value, key, "This value:"),
				new(CfgBufToken.Value, key, "is not a key!", "\n"),
				new(CfgBufToken.EndMultiline, key),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key, "This value:\nis not a key!"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task EmptyValueIncreasingIndentationThenMultiline()
		{
			string s = "Key1:\nKey2:\n\tThis value:\n\tis not a key!";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key1),
				new(CfgBufToken.Value, key1),
				new(CfgBufToken.Key, key2),
				new(CfgBufToken.StartMultiline, key2),
				new(CfgBufToken.Value, key2, "This value:"),
				new(CfgBufToken.Value, key2, "is not a key!", "\n"),
				new(CfgBufToken.EndMultiline, key2),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key1),
				new(SectionCfgToken.Value, key2, "This value:\nis not a key!"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task EmptyValueSingleQuoted()
		{
			string s = "Key:''";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.Value, key),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task EmptyValueDoubleQuoted()
		{
			string s = "\nKey:\"\"";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.Value, key),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task KeyQuotedValueThenKeyUnquoted()
		{
			string s = "Key1:'Value1'\nKey2:Value2";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key1),
				new(CfgBufToken.Value, key1, "Value1"),
				new(CfgBufToken.Key, key2),
				new(CfgBufToken.Value, key2, "Value2"),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key1, "Value1"),
				new(SectionCfgToken.Value, key2, "Value2"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task EmptyComment()
		{
			string s = "#";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Comment, default, ""),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Comment),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task EscapedDoubleQuote()
		{
			string s = "Key:\"\"\"\"";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.Value, key, "\""),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key, "\""),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task EscapedSingleQuote()
		{
			string s = "Key:''''";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.Value, key, "'"),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key, "'"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task KeyValue()
		{
			string s = "Key:Value";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.Value, key, "Value"),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key, "Value"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task KeyValueComment()
		{
			string s = "Key:'Value' #Explanation";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.Value, key, "Value"),
				new(CfgBufToken.Comment, key, "Explanation"),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key, "Value"),
				new(SectionCfgToken.Comment, key, "Explanation"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task KeyValueStartingWithNumberSign()
		{
			string s = "Key:#This is a very important string!";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.Value, key, "#This is a very important string!"),
				new(CfgBufToken.End)
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key, "#This is a very important string!"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task KeyValueNoWhitespace()
		{
			string s = "   Key\t\t: Value\t";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.Value, key, "Value"),
				new(CfgBufToken.End)
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key, "Value"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task KeyValueDoubleQuoted()
		{
			string s = "Key:\"'Value's all good!'\"";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.Value, key, "'Value's all good!'"),
				new(CfgBufToken.End)
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key, "'Value's all good!'"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task KeyValueSingleQuoted()
		{
			string s = "Key:'\"Value \"is\" all good!\"'";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.Value, key, "\"Value \"is\" all good!\""),
				new(CfgBufToken.End)
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key, "\"Value \"is\" all good!\""),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task ManyKeyValues()
		{
			var key_1 = CfgKey.Create("Key 1");
			var key_2 = CfgKey.Create("Key 2");
			var key_3 = CfgKey.Create("Key 3");

			string s = "Key 1:\tValue1\nKey 2:\t'Value 2'\n Key 3 : Value  3\n";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key_1),
				new(CfgBufToken.Value, key_1, "Value1"),
				new(CfgBufToken.Key, key_2),
				new(CfgBufToken.Value, key_2, "Value 2"),
				new(CfgBufToken.Key, key_3),
				new(CfgBufToken.Value, key_3, "Value  3"),
				new(CfgBufToken.End)
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key_1, "Value1"),
				new(SectionCfgToken.Value, key_2, "Value 2"),
				new(SectionCfgToken.Value, key_3, "Value  3"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task MultilineUnquotedValueLf()
		{
			string s = "Key:\n\tThis value\n\tspans many lines";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.StartMultiline, key),
				new(CfgBufToken.Value, key, "This value"),
				new(CfgBufToken.Value, key, "spans many lines", "\n"),
				new(CfgBufToken.EndMultiline, key),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key, "This value\nspans many lines"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task MultilineUnquotedValueLfTrailingNewline()
		{
			string s = "Key:\n\tThis value\n\tspans many lines\n";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.StartMultiline, key),
				new(CfgBufToken.Value, key, "This value"),
				new(CfgBufToken.Value, key, "spans many lines", "\n"),
				new(CfgBufToken.EndMultiline, key, "",  "\n"),
				new(CfgBufToken.End)
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key, "This value\nspans many lines"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task MultilineUnquotedValueLfTrailingNewlineAndTab()
		{
			string s = "Key:\n\tThis value\n\tspans many lines\n\t";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.StartMultiline, key),
				new(CfgBufToken.Value, key, "This value"),
				new(CfgBufToken.Value, key, "spans many lines", "\n"),
				new(CfgBufToken.Value, key, "", "\n"),
				new(CfgBufToken.EndMultiline, key),
				new(CfgBufToken.End)
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key, "This value\nspans many lines\n"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task MultilineUnquotedValueCrLf()
		{
			string s = "Key:\r\n\tThis value\r\n\tspans many lines";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.StartMultiline, key),
				new(CfgBufToken.Value, key, "This value"),
				new(CfgBufToken.Value, key, "spans many lines", "\r\n"),
				new(CfgBufToken.EndMultiline, key),
				new(CfgBufToken.End)
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key, "This value\r\nspans many lines"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task MultilineUnquotedValueCrLfTrailingNewline()
		{
			string s = "Key:\r\n\tThis value\r\n\tspans many lines\r\n";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.StartMultiline, key),
				new(CfgBufToken.Value, key, "This value"),
				new(CfgBufToken.Value, key, "spans many lines", "\r\n"),
				new(CfgBufToken.EndMultiline, key, "", "\r\n"),
				new(CfgBufToken.End)
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key, "This value\r\nspans many lines"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task MultilineUnquotedValueCr()
		{
			string s = "Key:\r\tThis value\r\tspans many lines";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.StartMultiline, key),
				new(CfgBufToken.Value, key, "This value"),
				new(CfgBufToken.Value, key, "spans many lines", "\r"),
				new(CfgBufToken.EndMultiline, key),
				new(CfgBufToken.End)
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key, "This value\rspans many lines"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task MultilineUnquotedValueCrTrailingCr()
		{
			string s = "Key:\r\tThis value\r\tspans many lines\r";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.StartMultiline, key),
				new(CfgBufToken.Value, key, "This value"),
				new(CfgBufToken.Value, key, "spans many lines", "\r"),
				new(CfgBufToken.EndMultiline, key, "", "\r"),
				new(CfgBufToken.End)
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key, "This value\rspans many lines"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task MultilineUnquotedValueMixed()
		{
			string s = "Key:\n\tThis value\n\tspans\r\tmany\r\n\tlines\n";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.StartMultiline, key),
				new(CfgBufToken.Value, key, "This value"),
				new(CfgBufToken.Value, key, "spans", "\n"),
				new(CfgBufToken.Value, key, "many", "\r"),
				new(CfgBufToken.Value, key, "lines", "\r\n"),
				new(CfgBufToken.EndMultiline, key, "", "\n"),
				new(CfgBufToken.End)
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key, "This value\nspans\rmany\r\nlines"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task MultilineQuotedValue()
		{
			string s = "Key:\n\"This value\nspans many\n\tlines\"";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.Value, key, "This value\nspans many\n\tlines"),
				new(CfgBufToken.End)
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key, "This value\nspans many\n\tlines"),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task Section()
		{
			string s = "Section{}";
			CfgKey k1 = CfgKey.Create("Section");
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, k1),
				new(CfgBufToken.StartSection, k1),
				new(CfgBufToken.EndSection, k1),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.StartSection, k1),
				new(SectionCfgToken.EndSection, k1),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task NestedSections()
		{
			string s = "Section1{Section2{Section3{Section4{}}}}";
			CfgKey s1 = CfgKey.Create("Section1");
			CfgKey s2 = CfgKey.Create("Section2");
			CfgKey s3 = CfgKey.Create("Section3");
			CfgKey s4 = CfgKey.Create("Section4");
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, s1),
				new(CfgBufToken.StartSection, s1),
				new(CfgBufToken.Key, s2),
				new(CfgBufToken.StartSection, s2),
				new(CfgBufToken.Key, s3),
				new(CfgBufToken.StartSection, s3),
				new(CfgBufToken.Key, s4),
				new(CfgBufToken.StartSection, s4),
				new(CfgBufToken.EndSection, s4),
				new(CfgBufToken.EndSection, s3),
				new(CfgBufToken.EndSection, s2),
				new(CfgBufToken.EndSection, s1),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.StartSection, s1),
				new(SectionCfgToken.StartSection, s2),
				new(SectionCfgToken.StartSection, s3),
				new(SectionCfgToken.StartSection, s4),
				new(SectionCfgToken.EndSection, s4),
				new(SectionCfgToken.EndSection, s3),
				new(SectionCfgToken.EndSection, s2),
				new(SectionCfgToken.EndSection, s1),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task SectionValue()
		{
			string s = "Section{Key1:Value1\nKey2:Value2\nKey3:Value3\n}";
			CfgKey s1 = CfgKey.Create("Section");
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, s1),
				new(CfgBufToken.StartSection, s1),
				new(CfgBufToken.Key, key1),
				new(CfgBufToken.Value, key1, "Value1"),
				new(CfgBufToken.Key, key2),
				new(CfgBufToken.Value, key2, "Value2"),
				new(CfgBufToken.Key, key3),
				new(CfgBufToken.Value, key3, "Value3"),
				new(CfgBufToken.EndSection, s1),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.StartSection, s1),
				new(SectionCfgToken.Value, key1, "Value1"),
				new(SectionCfgToken.Value, key2, "Value2"),
				new(SectionCfgToken.Value, key3, "Value3"),
				new(SectionCfgToken.EndSection, s1),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task List()
		{
			string s = "Section{Key :{One\n#Comment\nTwo\n\n'Three\nThree'#CommentAgain\nHeyyyy\n}}";
			CfgKey s1 = CfgKey.Create("Section");
			CfgKey l1 = key;
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, s1),
				new(CfgBufToken.StartSection, s1),
				new(CfgBufToken.Key, l1),
				new(CfgBufToken.StartList, l1),
				new(CfgBufToken.Value, l1, "One"),
				new(CfgBufToken.Comment, l1, "Comment"),
				new(CfgBufToken.Value, l1, "Two"),
				new(CfgBufToken.Value, l1, "Three\nThree"),
				new(CfgBufToken.Comment, l1, "CommentAgain"),
				new(CfgBufToken.Value, l1, "Heyyyy"),
				new(CfgBufToken.EndList, l1),
				new(CfgBufToken.EndSection, s1),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.StartSection, s1),
				new(SectionCfgToken.StartList, l1),
				new(SectionCfgToken.ListValue, l1, "One"),
				new(SectionCfgToken.Comment, l1, "Comment"),
				new(SectionCfgToken.ListValue, l1, "Two"),
				new(SectionCfgToken.ListValue, l1, "Three\nThree"),
				new(SectionCfgToken.Comment, l1, "CommentAgain"),
				new(SectionCfgToken.ListValue, l1, "Heyyyy"),
				new(SectionCfgToken.EndList, l1),
				new(SectionCfgToken.EndSection, s1),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task QuotedListNoSpaces()
		{
			string s = "List:{\"One\"'Two'\"Three\"'Four'}";
			CfgKey l1 = CfgKey.Create("List");
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, l1),
				new(CfgBufToken.StartList, l1),
				new(CfgBufToken.Value, l1, "One"),
				new(CfgBufToken.Value, l1, "Two"),
				new(CfgBufToken.Value, l1, "Three"),
				new(CfgBufToken.Value, l1, "Four"),
				new(CfgBufToken.EndList, l1),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.StartList, l1),
				new(SectionCfgToken.ListValue, l1, "One"),
				new(SectionCfgToken.ListValue, l1, "Two"),
				new(SectionCfgToken.ListValue, l1, "Three"),
				new(SectionCfgToken.ListValue, l1, "Four"),
				new(SectionCfgToken.EndList, l1),
				new(SectionCfgToken.End),
			});
		}
		[Fact]
		public static async Task QuotedListSpaces()
		{
			string s = "List: {\"One\" \"One\" \"'Two'\" 'Three' '\"Four\"'}";
			CfgKey l1 = CfgKey.Create("List");
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, l1),
				new(CfgBufToken.StartList, l1),
				new(CfgBufToken.Value, l1, "One"),
				new(CfgBufToken.Value, l1, "One"),
				new(CfgBufToken.Value, l1, "'Two'"),
				new(CfgBufToken.Value, l1, "Three"),
				new(CfgBufToken.Value, l1, "\"Four\""),
				new(CfgBufToken.EndList, l1),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
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
		[Fact]
		public static async Task ListIndentedMultilineQuotedText()
		{
			string s = "Section{\tList:{\n" +
				"\t\tOne\n" +
				"\tTwo\n" +
				"\t\t'Three\n" +
				"\t\tThree'\n" +
				"    \"Four\n" +
				"    Four\"}}";
			CfgKey s1 = CfgKey.Create("Section");
			CfgKey l1 = CfgKey.Create("List");
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, s1),
				new(CfgBufToken.StartSection, s1),
				new(CfgBufToken.Key, l1),
				new(CfgBufToken.StartList, l1),
				new(CfgBufToken.Value, l1, "One"),
				new(CfgBufToken.Value, l1, "Two"),
				new(CfgBufToken.Value, l1, "Three\n\t\tThree"),
				new(CfgBufToken.Value, l1, "Four\n    Four"),
				new(CfgBufToken.EndList, l1),
				new(CfgBufToken.EndSection, s1),
				new(CfgBufToken.End),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
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
	}
}
