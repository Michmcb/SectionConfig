namespace SectionConfig.Test
{
	using SectionConfig;
	using SectionConfig.IO;
	using System;
	using System.Collections.Generic;
	using Xunit;
	public static class Helper
	{
#pragma warning disable CS0618 // Type or member is obsolete
		public static void AssertReadMatches(SectionCfgReader scr, IEnumerable<ReadResult> results)
		{
			foreach (ReadResult er in results)
			{
				ReadResult ar = scr.Read();
				Assert.Equal(er.Token, ar.Token);
				Assert.Equal(er.Key, ar.Key);
				Assert.Equal(er.Content, ar.Content);
			}
			if (scr.State == ReadStreamState.Error)
			{
				ReadResult ar = scr.Read();
				Assert.Equal(SectionCfgToken.Error, ar.Token);
				Assert.Equal(default, ar.Key);
				Assert.Equal("Encountered error, cannot read further", ar.Content);
			}
			else
			{
				ReadResult ar = scr.Read();
				Assert.Equal(SectionCfgToken.End, ar.Token);
			}
		}
		public static void AssertReadMatches(CfgStreamReader scr, IEnumerable<ReadResult> results)
		{
			foreach (ReadResult er in results)
			{
				ReadResult ar = scr.Read();
				Assert.Equal(er.Token, ar.Token);
				Assert.Equal(er.Key, ar.Key);
				Assert.Equal(er.Content, ar.Content);
			}
			if (scr.State == ReadStreamState.Error)
			{
				ReadResult ar = scr.Read();
				Assert.Equal(SectionCfgToken.Error, ar.Token);
				Assert.Equal(default, ar.Key);
				Assert.Equal("Encountered error, cannot read further", ar.Content);
			}
			else
			{
				ReadResult ar = scr.Read();
				Assert.Equal(SectionCfgToken.End, ar.Token);
			}
		}
		public static CfgRoot LoadsProperly(CfgStreamReader scr, StringComparer stringComparer)
		{
			ValOrErr<CfgRoot, ErrMsg<LoadError>> lr = new CfgRootCfgLoader(stringComparer).TryLoad(scr);
			CfgRoot root = lr.ValueOrException();
			ErrMsg<LoadError> err = lr.Error;
			Assert.Equal(LoadError.Ok, err.Code);
			Assert.Null(err.Message);
			Assert.NotNull(root);
			return root;
		}
		public static void AssertKeyValues(ICfgObjectParent section, params KeyValuePair<string, string>[] kvps)
		{
			IReadOnlyDictionary<string, ICfgObject> elems = section.Elements;
			foreach (KeyValuePair<string, string> kvp in kvps)
			{
				CfgValue cv = Assert.IsType<CfgValue>(elems[kvp.Key]);
				Assert.Equal(kvp.Key, cv.Key.KeyString);
				Assert.Equal(kvp.Value, cv.Value);
			}
		}
#pragma warning restore CS0618 // Type or member is obsolete
	}
}
