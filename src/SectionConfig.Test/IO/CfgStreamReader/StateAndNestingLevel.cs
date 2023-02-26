namespace SectionConfig.Test.IO.CfgStreamReader
{
	using SectionConfig.IO;
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
			CfgKey sectionKey = CfgKey.Create("Section");
			CfgKey listKey = CfgKey.Create("ListKey");
			using CfgStreamReader csr = new(new StringReader(s), initialBufferSize: 0);
			CheckReadResult(csr, CfgKey.Create("Key"), SectionCfgToken.Value, "Value", 0, ReadStreamState.Section);

			CheckReadResult(csr, sectionKey, SectionCfgToken.StartSection, "", 1, ReadStreamState.Section);

			CheckReadResult(csr, listKey, SectionCfgToken.StartList, "", 1, ReadStreamState.List);

			CheckReadResult(csr, listKey, SectionCfgToken.ListValue, "1", 1, ReadStreamState.List);

			CheckReadResult(csr, listKey, SectionCfgToken.ListValue, "2", 1, ReadStreamState.List);

			CheckReadResult(csr, listKey, SectionCfgToken.ListValue, "3", 1, ReadStreamState.List);

			CheckReadResult(csr, listKey, SectionCfgToken.EndList, "", 1, ReadStreamState.Section);

			CheckReadResult(csr, sectionKey, SectionCfgToken.EndSection, "", 0, ReadStreamState.Section);

			CheckReadResult(csr, default, SectionCfgToken.End, "", 0, ReadStreamState.End);
		}
		[Fact]
		public static void BadTest()
		{
			string s = "Section{";

			using CfgStreamReader csr = new(new StringReader(s), initialBufferSize: 0);

			CfgKey sectionKey = CfgKey.Create("Section");
			CheckReadResult(csr, sectionKey, SectionCfgToken.StartSection, "", 1, ReadStreamState.Section);
			CheckReadResult(csr, sectionKey, SectionCfgToken.Error, "Found end of stream when there were still 1 sections to close", 1, ReadStreamState.Error);
			CheckReadResult(csr, sectionKey, SectionCfgToken.Error, "Encountered error, cannot read further", 1, ReadStreamState.Error);
		}
		private static void CheckReadResult(CfgStreamReader csr, CfgKey key, SectionCfgToken token, string content, int sectionLevel, ReadStreamState state)
		{
			var r = csr.Read();
			Assert.Equal(key, r.Key);
			Assert.Equal(token, r.Token);
			Assert.Equal(content, r.Content);
			Assert.Equal(sectionLevel, csr.SectionLevel);
			Assert.Equal(state, csr.State);
		}
	}
}
