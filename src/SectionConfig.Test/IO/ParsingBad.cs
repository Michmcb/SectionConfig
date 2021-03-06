namespace SectionConfig.Test.IO
{
	using SectionConfig.IO;
	using System.IO;
	using Xunit;

	public static class ParsingBad
	{
		[Fact]
		public static void UnclosedSection()
		{
			using SectionCfgReader scr = new(new StringReader("Key{"));
			Helper.AssertReadMatches(scr, new ReadResult[]
			{
				new(SectionCfgToken.StartSection, CfgKey.Create("Key"), string.Empty),
				new(SectionCfgToken.Error, default, "Found end of stream when there were still 1 sections to close"),
			});
		}
		[Fact]
		public static void UnclosedSectionAfterKeyValue()
		{
			using SectionCfgReader scr = new(new StringReader("Key{Key:Value"));
			Helper.AssertReadMatches(scr, new ReadResult[]
			{
				new(SectionCfgToken.StartSection, CfgKey.Create("Key"), string.Empty),
				new(SectionCfgToken.Value, CfgKey.Create("Key"), "Value"),
				new(SectionCfgToken.Error, default, "Found end of stream when there were still 1 sections to close"),
			});
		}
		[Fact]
		public static void ManyUnclosedSections()
		{
			using SectionCfgReader scr = new(new StringReader("Key{Key{Key{"));
			Helper.AssertReadMatches(scr, new ReadResult[]
			{
				new(SectionCfgToken.StartSection, CfgKey.Create("Key"), string.Empty),
				new(SectionCfgToken.StartSection, CfgKey.Create("Key"), string.Empty),
				new(SectionCfgToken.StartSection, CfgKey.Create("Key"), string.Empty),
				new(SectionCfgToken.Error, default, "Found end of stream when there were still 3 sections to close"),
			});
		}
		[Fact]
		public static void UnexpectedClose()
		{
			using SectionCfgReader scr = new(new StringReader("}"));
			Helper.AssertReadMatches(scr, new ReadResult[]
			{
				new(SectionCfgToken.Error,default, "Found section close when there was no section to close"),
				new(SectionCfgToken.Error, default, "Encountered error, cannot read further"),
			});
		}
		[Fact]
		public static void TooManyCloses()
		{
			using SectionCfgReader scr = new(new StringReader("Key{}}"));
			Helper.AssertReadMatches(scr, new ReadResult[]
			{
				new(SectionCfgToken.StartSection, CfgKey.Create("Key"), string.Empty),
				new(SectionCfgToken.EndSection, CfgKey.Create("Key"), string.Empty),
				new(SectionCfgToken.Error, default, "Found section close when there was no section to close"),
			});
		}
		[Fact]
		public static void UnclosedQuotedValue()
		{
			using SectionCfgReader scr = new(new StringReader("Key:'Value"));
			Helper.AssertReadMatches(scr, new ReadResult[]
			{
				new(SectionCfgToken.Error, CfgKey.Create("Key"), "Found end of stream when reading quoted string Value"),
			});
		}
		[Fact]
		public static void UnclosedQuotedValueInsideList()
		{
			using SectionCfgReader scr = new(new StringReader("Key:{'Value"));
			CfgKey l1 = CfgKey.Create("Key");
			Helper.AssertReadMatches(scr, new ReadResult[]
			{
				new(SectionCfgToken.StartList, l1, string.Empty),
				new(SectionCfgToken.Error, l1, "Found end of stream when reading quoted string Value"),
			});
		}
		[Fact]
		public static void UnclosedList()
		{
			using SectionCfgReader scr = new(new StringReader("Key:{"));
			CfgKey l1 = CfgKey.Create("Key");
			Helper.AssertReadMatches(scr, new ReadResult[]
			{
				new(SectionCfgToken.StartList, l1, string.Empty),
				new(SectionCfgToken.Error, l1, "Encountered end of stream when trying to read List Values"),
			});
		}
		[Fact]
		public static void LoneString()
		{
			using SectionCfgReader scr = new(new StringReader("String"));
			Helper.AssertReadMatches(scr, new ReadResult[]
			{
				new(SectionCfgToken.Error, default, "Found end of stream when reading Key String"),
			});
		}
		[Fact]
		public static void CommentAfterKeyButBeforeValue()
		{
			using SectionCfgReader scr = new(new StringReader("Key:#Value\nInvalid"));
			Helper.AssertReadMatches(scr, new ReadResult[]
			{
				new(SectionCfgToken.Value, CfgKey.Create("Key"), "#Value"),
				new(SectionCfgToken.Error, default, "Found end of stream when reading Key Invalid"),
			});
		}
		[Fact]
		public static void KeyWithNewline()
		{
			using SectionCfgReader scr = new(new StringReader("Ke\ny:Value"));
			Helper.AssertReadMatches(scr, new ReadResult[]
			{
				new(SectionCfgToken.Error, default, "This key is not valid, because it's empty, entirely whitespace, or contains forbidden characters (One of #:{}\\n\\r). This is the key: Ke\ny"),
			});
			using SectionCfgReader scr2 = new(new StringReader("Ke\ry:Value"));
			Helper.AssertReadMatches(scr2, new ReadResult[]
			{
				new(SectionCfgToken.Error, default, "This key is not valid, because it's empty, entirely whitespace, or contains forbidden characters (One of #:{}\\n\\r). This is the key: Ke\ry"),
			});
			using SectionCfgReader scr3 = new(new StringReader("Ke}y:Value"));
			Helper.AssertReadMatches(scr3, new ReadResult[]
			{
				new(SectionCfgToken.Error, default, "This key is not valid, because it's empty, entirely whitespace, or contains forbidden characters (One of #:{}\\n\\r). This is the key: Ke}y"),
			});
			using SectionCfgReader scr4 = new(new StringReader("Ke#y:Value"));
			Helper.AssertReadMatches(scr4, new ReadResult[]
			{
				new(SectionCfgToken.Error, default, "This key is not valid, because it's empty, entirely whitespace, or contains forbidden characters (One of #:{}\\n\\r). This is the key: Ke#y"),
			});
		}
	}
}
