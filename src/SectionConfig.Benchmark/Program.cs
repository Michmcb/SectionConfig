namespace SectionConfig.Benchmark
{
	using BenchmarkDotNet.Running;
	using System;

	internal class Program
	{
		static void Main(string[] args)
		{
			BenchmarkRunner.Run<BufferedVsNot>();
		}
	}
}