namespace SectionConfig.Test.IO.CfgBufferReader
{
	using SectionConfig.IO;
	using System;
	using System.IO;
	using Xunit;

	public static class StateAndNestingLevel
	{
		[Fact]
		public static void GoodTest()
		{
			string s = "Key:Value\n" +
				"Section{\n" +
				"\tListKey:{\n" +
				"\t\t1\n" +
				"\t\t2\n" +
				"\t\t3\n" +
				"\t}\n" +
				"}";
			CfgKey key = CfgKey.Create("Key");
			CfgKey sectionKey = CfgKey.Create("Section");
			CfgKey listKey = CfgKey.Create("ListKey");

			CfgBufferReader cbr = new(s, isFinalBlock: true);
			Assert.Equal(CfgBufToken.Key, cbr.Read());
			CheckState(cbr, key, 0);

			Assert.Equal(CfgBufToken.Value, cbr.Read());
			CheckState(cbr, key, 0, "Value");

			Assert.Equal(CfgBufToken.Key, cbr.Read());
			CheckState(cbr, sectionKey, 0);

			Assert.Equal(CfgBufToken.StartSection, cbr.Read());
			CheckState(cbr, sectionKey, 1);

			Assert.Equal(CfgBufToken.Key, cbr.Read());
			CheckState(cbr, listKey, 1);

			Assert.Equal(CfgBufToken.StartList, cbr.Read());
			CheckState(cbr, listKey, 1);

			Assert.Equal(CfgBufToken.Value, cbr.Read());
			CheckState(cbr, listKey, 1, "1");

			Assert.Equal(CfgBufToken.Value, cbr.Read());
			CheckState(cbr, listKey, 1, "2");

			Assert.Equal(CfgBufToken.Value, cbr.Read());
			CheckState(cbr, listKey, 1, "3");

			Assert.Equal(CfgBufToken.EndList, cbr.Read());
			CheckState(cbr, listKey, 1);

			Assert.Equal(CfgBufToken.EndSection, cbr.Read());
			CheckState(cbr, sectionKey, 0);

			Assert.Equal(CfgBufToken.End, cbr.Read());
			CheckState(cbr, default, 0);
		}
		[Fact]
		public static void BadTest()
		{
			string s = "Section{";

			CfgBufferReader cbr = new(s, isFinalBlock: true);

			CfgKey sectionKey = CfgKey.Create("Section");
			Assert.Equal(CfgBufToken.Key, cbr.Read());
			CheckState(cbr, sectionKey, 0);

			Assert.Equal(CfgBufToken.StartSection, cbr.Read());
			CheckState(cbr, sectionKey, 1);

			Assert.Equal(CfgBufToken.Error, cbr.Read());
			CheckState(cbr, sectionKey, 1, "Found end of stream when there were still 1 sections to close");

			Assert.Equal(CfgBufToken.Error, cbr.Read());
			CheckState(cbr, sectionKey, 1, "Encountered error, cannot read further");
		}
		private static void CheckState(CfgBufferReader r, CfgKey key, int sectionLevel, ReadOnlySpan<char> content = default)
		{
			Assert.Equal(key, r.Key);
			Assert.Equal(sectionLevel, r.SectionLevel);
			Helper.AssertSpanEqual(content, r.Content);
		}
	}
}
