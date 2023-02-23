namespace SectionConfig.Test.IO.CfgLoader
{
	using SectionConfig.IO;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using Xunit;

	public static class TryLoadIntoDictionary
	{
		[Fact]
		public static void WorksFine()
		{
			using CfgStreamReader scr = new(new StringReader("Key:Value\nSection1{Key:Value\nSection2{\nSection3{\nKey:Value\n}\n}\n}"));
			IReadOnlyDictionary<string, string> dict = new DictionaryCfgLoader(StringComparer.Ordinal).TryLoad(scr).ValueOrException();
			Assert.Equal("Value", Assert.Contains("Key", dict));
			Assert.Equal("Value", Assert.Contains("Section1:Key", dict));
			Assert.Equal("Value", Assert.Contains("Section1:Section2:Section3:Key", dict));
		}
		[Fact]
		public static void ListWorksFine()
		{
			using CfgStreamReader scr = new(new StringReader("Section{Key:Value\nlist1:{\n\ta\n\tb\n\tc\n}\nlist2:{\n\ta\n\tb\n\tc\n}}"));
			IReadOnlyDictionary<string, string> dict = new DictionaryCfgLoader(StringComparer.Ordinal).TryLoad(scr).ValueOrException();
			Assert.Equal("Value", Assert.Contains("Section:Key", dict));
			Assert.Equal("a", Assert.Contains("Section:list1:0", dict));
			Assert.Equal("b", Assert.Contains("Section:list1:1", dict));
			Assert.Equal("c", Assert.Contains("Section:list1:2", dict));
			Assert.Equal("a", Assert.Contains("Section:list2:0", dict));
			Assert.Equal("b", Assert.Contains("Section:list2:1", dict));
			Assert.Equal("c", Assert.Contains("Section:list2:2", dict));
		}
		[Fact]
		public static void DuplicateKey()
		{
			using CfgStreamReader scr = new(new StringReader("Section{Section2{Key:Value\nKey:Value}}"));
			var result = new DictionaryCfgLoader(StringComparer.Ordinal).TryLoad(scr);
			Assert.Null(result.Value);
			Assert.Equal(LoadError.DuplicateKey, result.Error.Code);
			Assert.Equal("Duplicate key \"Section:Section2:Key\" was found", result.Error.Message);
		}
		[Fact]
		public static void MalformedStream()
		{
			using CfgStreamReader scr = new(new StringReader("Key{"));
			var result = new DictionaryCfgLoader(StringComparer.Ordinal).TryLoad(scr);
			Assert.Null(result.Value);
			Assert.Equal(LoadError.MalformedStream, result.Error.Code);
			Assert.Equal("Found end of stream when there were still 1 sections to close", result.Error.Message);
		}
	}
}
