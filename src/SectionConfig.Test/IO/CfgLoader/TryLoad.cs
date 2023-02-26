namespace SectionConfig.Test.IO.CfgLoader
{
	using SectionConfig.IO;
	using System;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;
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
			using CfgStreamReader scr = new(new StringReader("Section{Key:Value\n}"));
			ValOrErr<CfgRoot, ErrMsg<LoadError>> result = new CfgRootCfgLoader().TryLoad(scr);
			Assert.NotNull(result.Value);

			Assert.Equal(LoadError.Ok, result.Error.Code);
			// null should default to ordinal
			Assert.Equal(StringComparer.Ordinal, result.Value!.KeyComparer);

			Check(result.Value!);
		}
		[Fact]
		public static void WorksFineFile()
		{
			CfgRootCfgLoader loader = new();
			File.WriteAllText("WorksFineFile.scfg", "Section{Key:Value\n}", Encoding.UTF8);
			ValOrErr<CfgRoot, ErrMsg<LoadError>> result = CfgLoader.TryLoad("WorksFineFile.scfg", Encoding.UTF8, loader);
			Assert.NotNull(result.Value);

			Assert.Equal(LoadError.Ok, result.Error.Code);

			Check(result.Value!);
		}
		[Fact]
		public static async Task WorksFineFileAsync()
		{
			CfgRootCfgLoader loader = new();
			File.WriteAllText("WorksFineFileAsync.scfg", "Section{Key:Value\n}", Encoding.UTF8);
			ValOrErr<CfgRoot, ErrMsg<LoadError>> result = await CfgLoader.TryLoadAsync("WorksFineFileAsync.scfg", Encoding.UTF8, loader);
			Assert.NotNull(result.Value);

			Assert.Equal(LoadError.Ok, result.Error.Code);

			Check(result.Value!);
		}
		[Fact]
		public static void WorksFineTextReader()
		{
			CfgRootCfgLoader loader = new();
			using TextReader tr = new StringReader("Section{Key:Value\n}");
			ValOrErr<CfgRoot, ErrMsg<LoadError>> result = CfgLoader.TryLoad(tr, loader);
			Assert.NotNull(result.Value);

			Assert.Equal(LoadError.Ok, result.Error.Code);

			Check(result.Value!);
		}
		[Fact]
		public static async Task WorksFineTextReaderAsync()
		{
			CfgRootCfgLoader loader = new();
			using TextReader tr = new StringReader("Section{Key:Value\n}");
			ValOrErr<CfgRoot, ErrMsg<LoadError>> result = await CfgLoader.TryLoadAsync(tr, loader);
			Assert.NotNull(result.Value);

			Assert.Equal(LoadError.Ok, result.Error.Code);

			Check(result.Value!);
		}
		[Fact]
		public static void WorksFineStreamReader()
		{
			CfgRootCfgLoader loader = new();
			using StreamReader tr = new(new MemoryStream(Encoding.UTF8.GetBytes("Section{Key:Value\n}")), Encoding.UTF8);
			ValOrErr<CfgRoot, ErrMsg<LoadError>> result = CfgLoader.TryLoad(tr, loader);
			Assert.NotNull(result.Value);

			Assert.Equal(LoadError.Ok, result.Error.Code);

			Check(result.Value!);
		}
		[Fact]
		public static async Task WorksFineStreamReaderAsync()
		{
			CfgRootCfgLoader loader = new();
			using StreamReader tr = new(new MemoryStream(Encoding.UTF8.GetBytes("Section{Key:Value\n}")), Encoding.UTF8);
			ValOrErr<CfgRoot, ErrMsg<LoadError>> result = await CfgLoader.TryLoadAsync(tr, loader);
			Assert.NotNull(result.Value);

			Assert.Equal(LoadError.Ok, result.Error.Code);

			Check(result.Value!);
		}
		[Fact]
		public static void DuplicateKey()
		{
			using CfgStreamReader scr = new(new StringReader("Section{Section2{Key:Value\nKey:Value}}"));
			ValOrErr<CfgRoot, ErrMsg<LoadError>> result = new CfgRootCfgLoader().TryLoad(scr);
			Assert.Null(result.Value);

			Assert.Equal(LoadError.DuplicateKey, result.Error.Code);
			Assert.Equal("Duplicate key \"Section:Section2:Key\" was found", result.Error.Message);
		}
		[Fact]
		public static void MalformedStream()
		{
			ValOrErr<CfgRoot, ErrMsg<LoadError>> result = CfgLoader.TryLoad(new StringReader("Key{"), new CfgRootCfgLoader());
			Assert.Null(result.Value);
			Assert.Equal(LoadError.MalformedStream, result.Error.Code);
			Assert.Equal("Found end of stream when there were still 1 sections to close", result.Error.Message);
		}
		[Fact]
		public static void DuplicateKeyList()
		{
			ValOrErr<CfgRoot, ErrMsg<LoadError>> result = CfgLoader.TryLoad(new StringReader("Key:{}Key:{}"), new CfgRootCfgLoader());
			Assert.Null(result.Value);
			Assert.Equal(LoadError.DuplicateKey, result.Error.Code);
			Assert.Equal("Duplicate key \":Key\" was found", result.Error.Message);
		}
		[Fact]
		public static void DuplicateKeySection()
		{
			ValOrErr<CfgRoot, ErrMsg<LoadError>> result = CfgLoader.TryLoad(new StringReader("Key{}Key{}"), new CfgRootCfgLoader());
			Assert.Null(result.Value);
			Assert.Equal(LoadError.DuplicateKey, result.Error.Code);
			Assert.Equal("Duplicate key \":Key\" was found", result.Error.Message);
		}
	}
}
