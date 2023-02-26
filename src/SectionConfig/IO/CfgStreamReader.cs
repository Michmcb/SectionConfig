namespace SectionConfig.IO
{
	using System;
	using System.Buffers;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// Reads section config data from a stream, forward-only.
	/// Practically, this wraps instances of <see cref="CfgBufferReader"/>, manages the buffer, and translates the calls into simpler results for you.
	/// If you want finer control, you can use <see cref="CfgLoader.ReadAllBuffered{T}(TextReader, T, HandleBufferToken{T}, int)"/>, for a thinner wrapper,
	/// or use <see cref="CfgBufferReader"/> directly.
	/// </summary>
	public sealed class CfgStreamReader : IDisposable
	{
		/// <summary>
		/// The default buffer size that is used.
		/// </summary>
		public const int DefaultBufferSize = 16384;
		private StringBuilder? multiline;
		private int currentDataSize;
		private char[] buf;
		private bool isFinalBlock;
		private bool readingList;
		private CfgReaderState state;
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="reader">The stream to read. Does not have to be seekable.</param>
		/// <param name="initialBufferSize">The default buffer size to use. May grow if required.</param>
		/// <param name="leaveOpen"><see langword="true"/> to leave <paramref name="reader"/> open after this <see cref="CfgStreamReader"/> is disposed. Otherwise, <see langword="false"/>.</param>
		public CfgStreamReader(TextReader reader, int initialBufferSize = DefaultBufferSize, bool leaveOpen = false)
		{
			Reader = reader;
			if (initialBufferSize == 0)
			{
				BufferSize = DefaultBufferSize;
			}
			else if (initialBufferSize < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(initialBufferSize), "Initial buffer size is less than zero");
			}
			else
			{
				BufferSize = initialBufferSize;
			}
			LeaveOpen = leaveOpen;
			buf = Array.Empty<char>();
			state = new CfgBufferReader(buf, false).GetState();
		}
		/// <summary>
		/// The underlying <see cref="TextReader"/> being used.
		/// </summary>
		public TextReader Reader { get; }
		/// <summary>
		/// The size of the buffer to use.
		/// This may grow if required.
		/// </summary>
		public int BufferSize { get; private set; }
		/// <summary>
		/// <see langword="true"/> to leave <see cref="Reader"/> open after this <see cref="CfgStreamReader"/> is disposed. Otherwise, false.
		/// </summary>
		public bool LeaveOpen { get; set; }
		/// <summary>
		/// The current state of the underlying <see cref="CfgBufferReader"/>.
		/// Due to the wrapping behaviour of this stream, you won't ever see <see cref="ReadStreamState.Key"/> or <see cref="ReadStreamState.Multiline"/>.
		/// </summary>
		public ReadStreamState State => state.state;
		/// <summary>
		/// The current nesting level of sections. Initially it is 0.
		/// </summary>
		public int SectionLevel => state.sectionKeys.Count;
		/// <summary>
		/// Reads the next result from <see cref="Reader"/>.
		/// </summary>
		public ReadResult Read()
		{
			while (true)
			{
				// Null means we need more data
				var rr = ReadInternal(out char[] newBuf, out int leftover);
				if (rr.HasValue)
				{
					return rr.Value;
				}
				else
				{
					buf = newBuf;
#if NETSTANDARD2_0
					int charsRead = Reader.Read(buf, leftover, buf.Length - leftover);
#else
					int charsRead = Reader.Read(buf.AsSpan(leftover));
#endif
					currentDataSize = leftover + charsRead;
					isFinalBlock = currentDataSize < buf.Length;
				}
			}
		}
		/// <summary>
		/// Reads the next result from <see cref="Reader"/> asynchronously.
		/// </summary>
		public async Task<ReadResult> ReadAsync()
		{
			while (true)
			{
				// Null means we need more data
				var rr = ReadInternal(out char[] newBuf, out int leftover);
				if (rr.HasValue)
				{
					return rr.Value;
				}
				else
				{
					buf = newBuf;
#if NETSTANDARD2_0
					int charsRead = await Reader.ReadAsync(buf, leftover, buf.Length - leftover);
#else
					int charsRead = await Reader.ReadAsync(buf.AsMemory(leftover));
#endif
					currentDataSize = leftover + charsRead;
					isFinalBlock = currentDataSize < buf.Length;
				}
			}
		}
		/// <summary>
		/// Reads the next token from <see cref="Reader"/>.
		/// </summary>
		/// <returns>The result of reading the next token.</returns>
		internal ReadResult? ReadInternal(out char[] newBuf, out int leftover)
		{
			newBuf = Array.Empty<char>();
			leftover = 0;
			CfgBufferReader reader = new(buf.AsSpan(0, currentDataSize), isFinalBlock, state);
			while (true)
			{
				// We don't care if we read a key, because the reader keeps track of the current key for us
				switch (reader.Read())
				{
					case CfgBufToken.Value:
						if (multiline == null)
						{
							state = reader.GetState();
							return new(readingList ? SectionCfgToken.ListValue : SectionCfgToken.Value, reader.Key, SpanAsString(reader.Content));
						}
						else
						{
							multiline.Append(reader.LeadingNewLine);
							multiline.Append(reader.Content);
						}
						break;
					case CfgBufToken.Comment:
						state = reader.GetState();
						return new(SectionCfgToken.Comment, reader.Key, SpanAsString(reader.Content));
					case CfgBufToken.StartMultiline:
						multiline = new();
						break;
					case CfgBufToken.EndMultiline:
						string multilineText = multiline?.ToString() ?? string.Empty;
						multiline = null;
						state = reader.GetState();
						return new(SectionCfgToken.Value, reader.Key, multilineText);
					case CfgBufToken.StartList:
						readingList = true;
						multiline = null;
						state = reader.GetState();
						return new(SectionCfgToken.StartList, reader.Key, SpanAsString(reader.Content));
					case CfgBufToken.EndList:
						readingList = false;
						state = reader.GetState();
						return new(SectionCfgToken.EndList, reader.Key, SpanAsString(reader.Content));
					case CfgBufToken.StartSection:
						state = reader.GetState();
						return new(SectionCfgToken.StartSection, reader.Key, SpanAsString(reader.Content));
					case CfgBufToken.EndSection:
						state = reader.GetState();
						return new(SectionCfgToken.EndSection, reader.Key, SpanAsString(reader.Content));
					case CfgBufToken.NeedMoreData:
						// Need some more data, so, we rent a new buffer
						if (buf.Length > 0 && state.position == 0)
						{
							// If we didn't manage to read anything, then double our buffer size
							do
							{
								BufferSize *= 2;
							} while (reader.Leftover > BufferSize);
						}
						newBuf = ArrayPool<char>.Shared.Rent(BufferSize);
						reader.CopyLeftoverAndResetPosition(newBuf, out leftover);
						state = reader.GetState();
						// Finally, return our old buffer (if we rented one)
						if (buf.Length > 0)
						{
							ArrayPool<char>.Shared.Return(buf);
						}
						return null;
					case CfgBufToken.End:
						if (buf.Length > 0)
						{
							ArrayPool<char>.Shared.Return(buf);
							buf = Array.Empty<char>();
						}
						currentDataSize = 0;
						state = reader.GetState();
						return new(SectionCfgToken.End, reader.Key, SpanAsString(reader.Content));
					case CfgBufToken.Error:
						state = reader.GetState();
						return new(SectionCfgToken.Error, reader.Key, SpanAsString(reader.Content));
				}
			}
		}
		private static string SpanAsString(ReadOnlySpan<char> str)
		{
			return str.Length == 0 ? string.Empty : str.ToString();
		}
		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls
		/// <summary>
		/// If <see cref="LeaveOpen"/> is <see langword="false"/>, disposes of <see cref="Reader"/>. Otherwise, leaves <see cref="Reader"/> open.
		/// </summary>
		public void Dispose()
		{
			if (!disposedValue)
			{
				if (LeaveOpen == false)
				{
					Reader.Dispose();
				}

				disposedValue = true;
			}
		}
		#endregion
	}
}
