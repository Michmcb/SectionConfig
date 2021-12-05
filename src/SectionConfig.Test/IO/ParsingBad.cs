namespace SectionConfig.Test.IO
{
	using SectionConfig.IO;
	using System;
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
				new(CfgKey.Create("Key")),
				new(SectionCfgToken.StartSection),
				new(SectionCfgToken.Error, "Found end of stream when there were still 1 sections to close"),
			});
		}
		[Fact]
		public static void UnclosedSectionAfterKeyValue()
		{
			using SectionCfgReader scr = new(new StringReader("Key{Key:Value"));
			Helper.AssertReadMatches(scr, new ReadResult[]
			{
				new(CfgKey.Create("Key")),
				new(SectionCfgToken.StartSection),
				new(CfgKey.Create("Key")),
				new(SectionCfgToken.Value, "Value"),
				new(SectionCfgToken.Error, "Found end of stream when there were still 1 sections to close"),
			});
		}
		[Fact]
		public static void ManyUnclosedSections()
		{
			using SectionCfgReader scr = new(new StringReader("Key{Key{Key{"));
			Helper.AssertReadMatches(scr, new ReadResult[]
			{
				new(CfgKey.Create("Key")),
				new(SectionCfgToken.StartSection),
				new(CfgKey.Create("Key")),
				new(SectionCfgToken.StartSection),
				new(CfgKey.Create("Key")),
				new(SectionCfgToken.StartSection),
				new(SectionCfgToken.Error, "Found end of stream when there were still 3 sections to close"),
			});
		}
		[Fact]
		public static void UnexpectedClose()
		{
			using SectionCfgReader scr = new(new StringReader("}"));
			Helper.AssertReadMatches(scr, new ReadResult[]
			{
				new(SectionCfgToken.Error, "Found section close when there was no section to close"),
				new(SectionCfgToken.Error, "Encountered error, cannot read further"),
			});
		}
		[Fact]
		public static void TooManyCloses()
		{
			using SectionCfgReader scr = new(new StringReader("Key{}}"));
			Helper.AssertReadMatches(scr, new ReadResult[]
			{
				new(CfgKey.Create("Key")),
				new(SectionCfgToken.StartSection),
				new(SectionCfgToken.EndSection),
				new(SectionCfgToken.Error, "Found section close when there was no section to close"),
			});
		}
		[Fact]
		public static void LoneString()
		{
			using SectionCfgReader scr = new(new StringReader("String"));
			Helper.AssertReadMatches(scr, new ReadResult[]
			{
				new(SectionCfgToken.Error, "Found end of stream when reading Key String"),
			});
		}
		[Fact]
		public static void CommentAfterKeyButBeforeValue()
		{
			using SectionCfgReader scr4 = new(new StringReader("Key:#Value\nInvalid"));
			Helper.AssertReadMatches(scr4, new ReadResult[]
			{
				new(CfgKey.Create("Key")),
				new(SectionCfgToken.Value, "#Value"),
				new(SectionCfgToken.Error, "Found end of stream when reading Key Invalid"),
			});
		}
		[Fact]
		public static void KeyWithNewline()
		{
			using SectionCfgReader scr = new(new StringReader("Ke\ny:Value"));
			Helper.AssertReadMatches(scr, new ReadResult[]
			{
				new(SectionCfgToken.Error, "Encountered an invalid character (\n) in the middle of Key Ke"),
			});
			using SectionCfgReader scr2 = new(new StringReader("Ke\ry:Value"));
			Helper.AssertReadMatches(scr2, new ReadResult[]
			{
				new(SectionCfgToken.Error, "Encountered an invalid character (\r) in the middle of Key Ke"),
			});
			using SectionCfgReader scr3 = new(new StringReader("Ke}y:Value"));
			Helper.AssertReadMatches(scr3, new ReadResult[]
			{
				new(SectionCfgToken.Error, "Encountered an invalid character (}) in the middle of Key Ke"),
			});
			using SectionCfgReader scr4 = new(new StringReader("Ke#y:Value"));
			Helper.AssertReadMatches(scr4, new ReadResult[]
			{
				new(SectionCfgToken.Error, "Encountered an invalid character (#) in the middle of Key Ke"),
			});
		}
	}
}
