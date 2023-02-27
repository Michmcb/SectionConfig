#if NET6_0_OR_GREATER
namespace SectionConfig.IO
{
	using System;
	using System.Buffers;
	using System.IO;
	/// <summary>
	/// Wraps a <see cref="CfgBufferReader"/> to make it easier to use. Takes care of pooling buffers and resizing.
	/// Use <see cref="Create(TextReader, Span{char})"/> to create a new instance.
	/// </summary>
	public ref struct CfgBufferHelper
	{
		private CfgBufferReader cbr;
		private CfgBufferHelper(CfgBufferReader bufferReader, IMemoryOwner<char>? pooledBuffer, Span<char> buffer)
		{
			cbr = bufferReader;
			PooledBuffer = pooledBuffer;
			Buffer = buffer;
		}
		/// <summary>
		/// The <see cref="CfgBufferReader"/> that this instance wraps.
		/// </summary>
		public CfgBufferReader BufferReader => cbr;
		/// <summary>
		/// The buffer that has been allocated from <see cref="MemoryPool{T}.Shared"/>, if any.
		/// </summary>
		public IMemoryOwner<char>? PooledBuffer { get; private set; }
		/// <summary>
		/// The buffer that's currently being used.
		/// </summary>
		public Span<char> Buffer { get; private set; }
		/// <summary>
		/// Creates a new instance, initialized with data from <paramref name="reader"/>.
		/// If provided, uses <paramref name="buffer"/> as the initial buffer; this may be a stack allocated buffer to possibly avoid memory pool allocations.
		/// </summary>
		/// <param name="reader">The reader from which to initial data.</param>
		/// <param name="buffer">The inital buffer. Passing a buffer of length 0 (or <see langword="default"/>) will allocate from <see cref="MemoryPool{T}.Shared"/></param>
		public static CfgBufferHelper Create(TextReader reader, Span<char> buffer = default)
		{
			IMemoryOwner<char>? pooledBuffer = null;
			if (buffer.Length == 0)
			{
				pooledBuffer = MemoryPool<char>.Shared.Rent(1024);
				buffer = pooledBuffer.Memory.Span;
			}

			int charsRead = reader.Read(buffer);
			return new(new CfgBufferReader(buffer[..charsRead], isFinalBlock: charsRead == 0), pooledBuffer, buffer);
		}
		/// <summary>
		/// Reads <see cref="BufferReader"/> for more data.
		/// If <see cref="CfgBufToken.NeedMoreData"/> is encountered, reads <paramref name="reader"/> for more data.
		/// </summary>
		/// <param name="reader">The reader from which to retrieve more data, if necessary.</param>
		/// <returns>The token read.</returns>
		public CfgBufToken Read(TextReader reader)
		{
			while (true)
			{
				CfgBufToken t = cbr.Read();
				if (t == CfgBufToken.NeedMoreData)
				{
					int leftover;
					int newBufferSize = Math.Min(cbr.SuggestedNewBufferSize(), Array.MaxLength);
					if (newBufferSize > Buffer.Length)
					{
						// We need a bigger buffer, so rent a new one
						IMemoryOwner<char> newPoolBuf = MemoryPool<char>.Shared.Rent(newBufferSize);
						Buffer = newPoolBuf.Memory.Span;
						cbr.CopyLeftoverAndResetPosition(Buffer, out leftover);
						PooledBuffer?.Dispose();
						PooledBuffer = newPoolBuf;
					}
					else
					{
						// Our buffer size is still totally fine, so we can just shift the remaining data backwards to overwrite old data
						cbr.CopyLeftoverAndResetPosition(Buffer, out leftover);
					}

					// Fill the remaining space in the buffer
					int charsRead = reader.Read(Buffer[leftover..]);
					// Now the new current data is the leftover data plus the data we just read
					cbr = new(Buffer[..(leftover + charsRead)], isFinalBlock: charsRead == 0, cbr.GetState());
				}
				else
				{
					return t;
				}
			}
		}
		/// <summary>
		/// Returns <see cref="PooledBuffer"/> if one was rented.
		/// </summary>
		public void Dispose()
		{
			PooledBuffer?.Dispose();
		}
		internal const int stackMax = 512;
		/// <summary>
		/// Uses an instance of <see cref="CfgBufferReader"/> to read all data from <paramref name="reader"/>.
		/// Takes care of all buffering for you, so you will never have to handle <see cref="CfgBufToken.NeedMoreData"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <param name="state">Any extra state information to pass to <paramref name="callback"/>.</param>
		/// <param name="callback">The callback to invoke for every token read, except <see cref="CfgBufToken.NeedMoreData"/>.</param>
		/// <param name="initialBufferSize">The initial buffer size. If the default size or smaller (and not netstandard2.0), uses stackalloc. If it's larger than the default size or if it grows, uses <see cref="MemoryPool{T}.Shared"/></param>
		/// <returns><see langword="true"/> if all invocations of <paramref name="callback"/> returned true, otherwise <see langword="false"/>.</returns>
		public static bool ReadAll<T>(TextReader reader, T state, HandleBufferToken<T> callback, int initialBufferSize = stackMax)
		{
			if (initialBufferSize == 0)
			{
				initialBufferSize = stackMax;
			}
			else if (initialBufferSize < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(initialBufferSize), "Initial buffer size is less than zero");
			}
			Span<char> buf2 = new char[initialBufferSize];
			using CfgBufferHelper helper = Create(reader, buf2);
			while (true)
			{
				CfgBufToken t = helper.Read(reader);
				if (!callback(state, t, helper.BufferReader))
				{
					return false;
				}
				if (t == CfgBufToken.End || t == CfgBufToken.Error)
				{
					return true;
				}
			}
		}
	}
}
#endif