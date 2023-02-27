namespace SectionConfig.Test
{
	using System;
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
		public override Task<string> ReadToEndAsync(CancellationToken cancellationToken)
		{
			return Task.FromResult(ReadToEnd());
		}
	}
}
