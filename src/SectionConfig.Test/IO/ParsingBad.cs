namespace SectionConfig.Test.IO
{
	using SectionConfig.IO;
	using System.Threading.Tasks;
	using Xunit;

	public static class ParsingBad
	{
		private static readonly CfgKey key = CfgKey.Create("Key");
		[Fact]
		public static async Task UnclosedSection()
		{
			string s = "Key{";
			string errMsg = "Found end of stream when there were still 1 sections to close";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.StartSection, key),
				new(CfgBufToken.Error, key, errMsg),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.StartSection, key),
				new(SectionCfgToken.Error, key, errMsg),
			}, LoadError.MalformedStream);
		}
		[Fact]
		public static async Task UnclosedSectionAfterKeyValue()
		{
			string s = "Key{Key:Value";
			string errMsg = "Found end of stream when there were still 1 sections to close";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.StartSection, key),
				new(CfgBufToken.Key, key),
				new(CfgBufToken.Value, key, "Value"),
				new(CfgBufToken.Error, key, errMsg),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.StartSection, key),
				new(SectionCfgToken.Value, key, "Value"),
				new(SectionCfgToken.Error, key, errMsg),
			}, LoadError.MalformedStream);
		}
		[Fact]
		public static async Task ManyUnclosedSections()
		{
			string s = "Key{Key{Key{";
			string errMsg = "Found end of stream when there were still 3 sections to close";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.StartSection, key),
				new(CfgBufToken.Key, key),
				new(CfgBufToken.StartSection, key),
				new(CfgBufToken.Key, key),
				new(CfgBufToken.StartSection, key),
				new(CfgBufToken.Error, key, errMsg),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.StartSection, key),
				new(SectionCfgToken.StartSection, key),
				new(SectionCfgToken.StartSection, key),
				new(SectionCfgToken.Error, key, errMsg),
			}, LoadError.MalformedStream);
		}
		[Fact]
		public static async Task UnexpectedClose()
		{
			string s = "}";
			string errMsg = "Found section close when there was no section to close";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Error, default, errMsg),
				new(CfgBufToken.Error, default, errMsg),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Error, default, errMsg),
				new(SectionCfgToken.Error, default, errMsg),
			}, LoadError.MalformedStream);
		}
		[Fact]
		public static async Task TooManyCloses()
		{
			string s = "Key{}}";
			string errMsg = "Found section close when there was no section to close";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.StartSection, key),
				new(CfgBufToken.EndSection, key),
				new(CfgBufToken.Error, key, errMsg),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.StartSection, key),
				new(SectionCfgToken.EndSection, key),
				new(SectionCfgToken.Error, key, errMsg),
			}, LoadError.MalformedStream);
		}
		[Fact]
		public static async Task UnclosedQuotedValue()
		{
			string s = "Key:'Value";
			string errMsg = "Found end of stream when reading quoted string: Value";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.Error, key, errMsg),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Error, key, errMsg),
			}, LoadError.MalformedStream);
		}
		[Fact]
		public static async Task UnclosedQuotedValueInsideList()
		{
			CfgKey l1 = key;
			string s = "Key:{'Value";
			string errMsg = "Found end of stream when reading a list-contained quoted string: Value";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, l1),
				new(CfgBufToken.StartList, l1),
				new(CfgBufToken.Error, l1, errMsg),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.StartList, key),
				new(SectionCfgToken.Error, l1, errMsg),
			}, LoadError.MalformedStream);
		}
		[Fact]
		public static async Task UnclosedUnquotedValueInsideList()
		{
			CfgKey l1 = key;
			string s = "Key:{Value";
			string errMsg = "Found end of stream when reading a list-contained unquoted string: Value";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, l1),
				new(CfgBufToken.StartList, l1),
				new(CfgBufToken.Error, l1, errMsg),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.StartList, l1),
				new(SectionCfgToken.Error, l1, errMsg),
			}, LoadError.MalformedStream);
		}
		[Fact]
		public static async Task UnclosedList()
		{
			CfgKey l1 = key;
			string s = "Key:{";
			string errMsg = "Encountered end of stream when trying to read List Values";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, l1),
				new(CfgBufToken.StartList, l1),
				new(CfgBufToken.Error, l1, errMsg),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.StartList, l1),
				new(SectionCfgToken.Error, l1, errMsg),
			}, LoadError.MalformedStream);
		}
		[Fact]
		public static async Task UnclosedListWithComment()
		{
			CfgKey l1 = key;
			string s = "Key:{#MyComment";
			string errMsg = "Found end of stream when reading a comment inside a list, and the list was not closed properly. The comment is: MyComment";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, l1),
				new(CfgBufToken.StartList, l1),
				new(CfgBufToken.Error, l1, errMsg),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.StartList, l1),
				new(SectionCfgToken.Error, l1, errMsg),
			}, LoadError.MalformedStream);
		}
		[Fact]
		public static async Task LoneString()
		{
			string s = "String";
			string errMsg = "Found end of stream when reading key: String";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Error, default, errMsg),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Error, default, errMsg),
			}, LoadError.MalformedStream);
		}
		[Fact]
		public static async Task CommentAfterKeyButBeforeValue()
		{
			string s = "Key:#Value\nInvalid";
			string errMsg = "Found end of stream when reading key: Invalid";
			Helper.TestCfgBufferReader(s, new BufReadResult[]
			{
				new(CfgBufToken.Key, key),
				new(CfgBufToken.Value, key, "#Value"),
				new(CfgBufToken.Error, key, errMsg),
			});
			await Helper.TestCfgStreamReader(s, new ReadResult[]
			{
				new(SectionCfgToken.Value, key, "#Value"),
				new(SectionCfgToken.Error, key, errMsg),
			}, LoadError.MalformedStream);
		}
		[Fact]
		public static async Task KeyWithNewline()
		{
			foreach (var item in new (string key, string value)[]
			{
				("Ke\ny", "Value"),
				("Ke\ry", "Value"),
				("Ke}y", "Value"),
				("Ke#y", "Value"),
			})
			{
				string s = string.Concat(item.key, ":", item.value);
				string errMsg = "This key is not valid, because it's empty, entirely whitespace, or contains forbidden characters (One of #:{}\\n\\r). This is the key: " + item.key;
				Helper.TestCfgBufferReader(s, new BufReadResult[]
				{
					new(CfgBufToken.Error, default, errMsg),
				});
				await Helper.TestCfgStreamReader(s, new ReadResult[]
				{
					new(SectionCfgToken.Error, default, errMsg),
				}, LoadError.MalformedStream);
			}
		}
	}
}
