namespace SectionConfig.Test
{
	using SectionConfig;
	using SectionConfig.IO;
	using System;
	using System.Collections.Generic;
	using Xunit;
	public static class Helper
	{
		public static void AssertReadMatches(SectionCfgReader scr, IEnumerable<ReadResult> results)
		{
			foreach (ReadResult er in results)
			{
				ReadResult ar = scr.Read();
				Assert.Equal(er.Token, ar.Token);
				if (ar.Token == SectionCfgToken.Key)
				{
					Assert.Equal(er.GetKey(), ar.GetKey());
				}
				else
				{
					Assert.Equal(er.GetContent(), ar.GetContent());
				}
			}
		}
		public static CfgRoot LoadsProperly(SectionCfgReader scr, StringComparer stringComparer)
		{
			LoadResult lr = CfgLoader.TryLoad(scr, null, stringComparer);
			Assert.Empty(lr.ErrMsg);
			Assert.Equal(LoadError.Ok, lr.Error);
			Assert.NotNull(lr.Root);
			return lr.Root!;
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
	}
}
