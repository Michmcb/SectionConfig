namespace SectionConfig.Benchmark
{
	using BenchmarkDotNet.Attributes;
	using BenchmarkDotNet.Jobs;
	using SectionConfig.IO;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;

	[SimpleJob(RuntimeMoniker.Net60)]
	[SimpleJob(RuntimeMoniker.Net70)]
	[RPlotExporter]
	[MemoryDiagnoser]
	public class BufferedVsNot
	{
		public const string ExampleConfig = "\n" +
"Key 1:     Blah    \n" +
"Key 2: 'Blah'\n" +
"# Some comment   \n" +
"	Key3: 'Blah'\n" +
"Key 4:\n" +
"	This is a multiline value\n" +
"	It will just keep going\n" +
"	Until we find lesser indentation\n" +
"		This is still part of the string\n" +
"	Done\n" +
"\n" +
"	Key 5:\n" +
"		Aligned, still\n" +
"		a multiline value.\n" +
"Section{\n" +
"	Key:\n" +
"\t\t'A quoted string\n" +
"\t\tIt just keeps going too'\n" +
"\n" +
"}\n" +
"List:{\n" +
"	String 1\n" +
"	\"String 2\"\n" +
"	# A comment\n" +
"	'String 3'\n" +
"	\n" +
"	'String\n" +
"	4'\n" +
"	\"String\n" +
"	5\"\n" +
"}\n";

		[Benchmark]
		public Dictionary<string, Strings> TryLoad()
		{
			using CfgStreamReader csr = new(new StringReader(ExampleConfig));
			return new DictionaryCfgLoader().TryLoad(csr).Value ?? throw new InvalidDataException();
		}
		[Benchmark]
		public async Task<Dictionary<string, Strings>> TryLoadAsync()
		{
			using CfgStreamReader csr = new(new StringReader(ExampleConfig));
			return (await new DictionaryCfgLoader().TryLoadAsync(csr)).Value ?? throw new InvalidDataException();
		}
		//[Benchmark]
		//public Dictionary<string, string> TryLoadTextReader()
		//{
		//	using StringReader ss = new(ExampleConfig);
		//	return new DictionaryCfgLoader().TryLoadTextReader(ss).Value ?? throw new InvalidDataException();
		//}
		//[Benchmark]
		//public Dictionary<string, string> LoadEntireBuffer()
		//{
		//	return new DictionaryCfgLoader().TryLoadBuffer(ExampleConfig).Value ?? throw new InvalidDataException();
		//}
	}
}