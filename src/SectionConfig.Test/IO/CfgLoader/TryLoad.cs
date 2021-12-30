namespace SectionConfig.Test.IO.CfgLoader
{
	using SectionConfig.IO;
	using System;
	using System.IO;
	using Xunit;

	public static class TryLoad
	{
		[Fact]
		public static void DuplicateKey()
		{
			using SectionCfgReader scr = new(new StringReader("Section{Section2{Key:Value\nKey:Value}}"));
			ValOrErr<CfgRoot, ErrMsg<LoadError>> result = CfgLoader.TryLoad(scr, StringComparer.Ordinal);
			Assert.Null(result.Value);

			Assert.Equal(LoadError.DuplicateKey, result.Error.Code);
			Assert.Equal("Duplicate key \"Section:Section2:Key\" was found", result.Error.Message);
		}
	}
}
