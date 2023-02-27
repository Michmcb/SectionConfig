namespace SectionConfig.Benchmark
{
	using BenchmarkDotNet.Attributes;
	using BenchmarkDotNet.Jobs;
	using SectionConfig.IO;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading;
	using System.Threading.Tasks;
	public sealed class SmallStringReader : TextReader
	{
		public SmallStringReader(string str, int maxSizePerRead)
		{
			Str = str;
			MaxSizePerRead = maxSizePerRead;
		}
		public string Str { get; }
		public int MaxSizePerRead { get; }
		public int Position { get; set; }
		public override int Peek()
		{
			if (Position < Str.Length)
			{
				return Str[Position];
			}
			else return -1;
		}
		public override int Read()
		{
			if (Position < Str.Length)
			{
				return Str[Position++];
			}
			else return -1;
		}
		public override int Read(char[] buffer, int index, int count)
		{
			return Read(buffer.AsSpan(index, count));
		}
		public override int Read(Span<char> buffer)
		{
			int i = 0;
			for (; Position < Str.Length && i < MaxSizePerRead && i < buffer.Length; i++, Position++)
			{
				buffer[i] = Str[Position];
			}
			return i;
		}
		public override Task<int> ReadAsync(char[] buffer, int index, int count)
		{
			return Task.FromResult(Read(buffer.AsSpan(index, count)));
		}
		public override ValueTask<int> ReadAsync(Memory<char> buffer, CancellationToken cancellationToken = default)
		{
			return ValueTask.FromResult(Read(buffer.Span));
		}
		public override string ReadToEnd()
		{
			string s = Str[Position..];
			Position = s.Length;
			return s;
		}
		public override Task<string> ReadToEndAsync()
		{
			return Task.FromResult(ReadToEnd());
		}
	}
	//[SimpleJob(RuntimeMoniker.Net60)]
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
		public Dictionary<string, Strings>? TryLoad()
		{
			using CfgStreamReader csr = new(new StringReader(ExampleConfig));
			return new DictionaryCfgLoader().TryLoad(csr).Value;
		}
		[Benchmark]
		public Dictionary<string, Strings>? TryLoadChunksOf10()
		{
			using CfgStreamReader csr = new(new SmallStringReader(ExampleConfig, 10));
			return new DictionaryCfgLoader().TryLoad(csr).Value;
		}
		//[Benchmark]
		//public async Task<Dictionary<string, Strings>?> TryLoadAsync()
		//{
		//	using CfgStreamReader csr = new(new StringReader(ExampleConfig));
		//	return (await new DictionaryCfgLoader().TryLoadAsync(csr)).Value;
		//}
		//[Benchmark]
		//public Dictionary<string, Strings>? TryLoadTextReader()
		//{
		//	using StringReader ss = new(ExampleConfig);
		//	return new DictionaryCfgLoader().TryLoadTextReader(ss).Value;
		//}
		//[Benchmark]
		//public Dictionary<string, Strings> Baseline()
		//{
		//	using StringReader ss = new(ExampleConfig);
		//	CfgLoader.ReadAllBuffered(ss, 0, (_, _, _) => true);
		//	return null;
		//}
	}
}