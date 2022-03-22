namespace SectionConfig.Test.IO.CfgLoader
{
	using SectionConfig.IO;
	using System;
	using System.IO;
	using System.Text;
	using Xunit;

	public static class TryLoad
	{
		private static void Check(CfgRoot root)
		{
			Assert.Collection(root.Elements, x =>
			{
				Assert.Equal("Section", x.Key);
				Assert.Collection(x.Value.ToSection().Elements, y =>
				{
					Assert.Equal("Key", y.Key);
					Assert.Equal("Value", y.Value.ToValue().Value);
				});
			});
		}
		[Fact]
		public static void WorksFine()
		{
			using SectionCfgReader scr = new(new StringReader("Section{Key:Value\n}"));
			ValOrErr<CfgRoot, ErrMsg<LoadError>> result = CfgLoader.TryLoad(scr, keyComparer: null);
			Assert.NotNull(result.Value);

			Assert.Equal(LoadError.Ok, result.Error.Code);
			// null should default to ordinal
			Assert.Equal(StringComparer.Ordinal, result.Value!.KeyComparer);

			Check(result.Value!);
		}
		[Fact]
		public static void WorksFineFile()
		{
			File.WriteAllText("file.scfg", "Section{Key:Value\n}", Encoding.UTF8);
			ValOrErr<CfgRoot, ErrMsg<LoadError>> result = CfgLoader.TryLoad("file.scfg", Encoding.UTF8, StringComparer.Ordinal);
			Assert.NotNull(result.Value);

			Assert.Equal(LoadError.Ok, result.Error.Code);

			Check(result.Value!);
		}
		[Fact]
		public static void WorksFineTextReader()
		{
			using TextReader tr = new StringReader("Section{Key:Value\n}");
			ValOrErr<CfgRoot, ErrMsg<LoadError>> result = CfgLoader.TryLoad(tr, StringComparer.Ordinal);
			Assert.NotNull(result.Value);

			Assert.Equal(LoadError.Ok, result.Error.Code);

			Check(result.Value!);
		}
		[Fact]
		public static void WorksFineStreamReader()
		{
			using StreamReader tr = new (new MemoryStream(Encoding.UTF8.GetBytes ("Section{Key:Value\n}")), Encoding.UTF8);
			ValOrErr<CfgRoot, ErrMsg<LoadError>> result = CfgLoader.TryLoad(tr, StringComparer.Ordinal);
			Assert.NotNull(result.Value);

			Assert.Equal(LoadError.Ok, result.Error.Code);

			Check(result.Value!);
		}
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
