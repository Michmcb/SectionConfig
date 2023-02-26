namespace SectionConfig.IO
{
	using System;
	using System.Buffers;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// Helper class for loading config data from various sources.
	/// </summary>
	public static class CfgLoader
	{
		internal const int stackMax = 512;
		/// <summary>
		/// Uses an instance of <see cref="CfgBufferReader"/> to read all data from <paramref name="reader"/>.
		/// Takes care of all buffering for you, so you will never have to handle <see cref="CfgBufToken.NeedMoreData"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <param name="state">Any extra state information to pass to <paramref name="callback"/>.</param>
		/// <param name="callback">The callback to invoke for every token read, except <see cref="CfgBufToken.NeedMoreData"/>.</param>
		/// <param name="initialBufferSize">The initial buffer size. If the default size or smaller (and not netstandard2.0), uses stackalloc. If it's larger than the default size or if it grows, uses <see cref="ArrayPool{T}.Shared"/></param>
		/// <returns><see langword="true"/> if all invocations of <paramref name="callback"/> returned true, otherwise <see langword="false"/>.</returns>
		public static bool ReadAllBuffered<T>(TextReader reader, T state, HandleBufferToken<T> callback, int initialBufferSize = stackMax)
		{
			if (initialBufferSize == 0)
			{
				initialBufferSize = stackMax;
			}
			else if (initialBufferSize < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(initialBufferSize), "Initial buffer size is less than zero");
			}
			int charsRead;
#if NETSTANDARD2_0
			char[]? poolBuf = ArrayPool<char>.Shared.Rent(initialBufferSize);
			char[] buf = poolBuf;
			charsRead = reader.Read(buf, 0, buf.Length);
			var slice = buf.AsSpan(0, charsRead);
#else
			char[]? poolBuf = null;
			if (initialBufferSize > stackMax)
			{
				poolBuf = ArrayPool<char>.Shared.Rent(initialBufferSize);
			}
			Span<char> buf = poolBuf ?? stackalloc char[initialBufferSize];
			charsRead = reader.Read(buf);
			var slice = buf[..charsRead];
#endif
			CfgBufferReader cbr = new(slice, isFinalBlock: charsRead < buf.Length);

			try
			{
				while (true)
				{
					CfgBufToken t = cbr.Read();
					if (t == CfgBufToken.NeedMoreData)
					{
						// If we didn't manage to read anything, then double our buffer size
						bool growBuffer = false;
						if (cbr.Position == 0)
						{
							growBuffer = true;
							do
							{
								initialBufferSize *= 2;
							} while (cbr.Leftover > initialBufferSize);
						}
						// We always use the array pool if we grew the buffer; using stackalloc here would be a very bad idea!
						int copied;
						if (growBuffer)
						{
							var oldPoolBuf = poolBuf;
							poolBuf = ArrayPool<char>.Shared.Rent(initialBufferSize);
							buf = poolBuf;
							cbr.CopyLeftoverAndResetPosition(buf, out copied);
							if (oldPoolBuf != null)
							{
								ArrayPool<char>.Shared.Return(oldPoolBuf);
							}
						}
						else
						{
							// Didn't grow the buffer, so just re-use the buffer we have
							cbr.CopyLeftoverAndResetPosition(buf, out copied);
						}
						// Refill the buffer and make a new buffer reader ready for next iteration
#if NETSTANDARD2_0
					charsRead = reader.Read(buf, copied, buf.Length - copied);
					int dataSize = copied + charsRead;
					slice = buf.AsSpan(0, dataSize);
#else
						charsRead = reader.Read(buf[copied..]);
						int dataSize = copied + charsRead;
						slice = buf[..dataSize];
#endif
						cbr = new(slice, isFinalBlock: dataSize < buf.Length, cbr.GetState());
					}
					else
					{
						if (!callback(state, t, cbr))
						{
							return false;
						}
					}
					if (t == CfgBufToken.End || t == CfgBufToken.Error)
					{
						return true;
					}
				}
			}
			finally
			{
				if (poolBuf != null)
				{
					ArrayPool<char>.Shared.Return(poolBuf);
				}
			}
		}
		/// <summary>
		/// Loads section config data from the file at <paramref name="path"/>, reading using the <paramref name="encoding"/>.
		/// </summary>
		/// <param name="path">The file to read.</param>
		/// <param name="encoding">Encoding to interpet file as.</param>
		/// <param name="loader">The loader that to invoke.</param>
		/// <returns>A <typeparamref name="T"/> with all loaded data, or an <see cref="ErrMsg{TCode}"/> describing an error which prevented the data from being loaded.</returns>
		public static ValOrErr<T, ErrMsg<LoadError>> TryLoad<T>(string path, Encoding encoding, ICfgLoaderSync<T> loader)
			where T : class
		{
			using CfgStreamReader scr = new(new StreamReader(path, encoding), leaveOpen: false);
			return loader.TryLoad(scr);
		}
		/// <summary>
		/// Loads section config data from <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <param name="loader">The loader that to invoke.</param>
		/// <returns>A <typeparamref name="T"/> with all loaded data, or an <see cref="ErrMsg{TCode}"/> describing an error which prevented the data from being loaded.</returns>
		public static ValOrErr<T, ErrMsg<LoadError>> TryLoad<T>(TextReader reader, ICfgLoaderSync<T> loader)
			where T : class
		{
			using CfgStreamReader scr = new(reader, leaveOpen: false);
			return loader.TryLoad(scr);
		}
		/// <summary>
		/// Loads section config data from <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <param name="loader">The loader that to invoke.</param>
		/// <returns>A <typeparamref name="T"/> with all loaded data, or an <see cref="ErrMsg{TCode}"/> describing an error which prevented the data from being loaded.</returns>
		public static ValOrErr<T, ErrMsg<LoadError>> TryLoad<T>(StreamReader reader, ICfgLoaderSync<T> loader)
			where T : class
		{
			using CfgStreamReader scr = new(reader, leaveOpen: false);
			return loader.TryLoad(scr);
		}
		/// <summary>
		/// Loads section config data from the file at <paramref name="path"/>, reading using the <paramref name="encoding"/>.
		/// </summary>
		/// <param name="path">The file to read.</param>
		/// <param name="encoding">Encoding to interpet file as.</param>
		/// <param name="loader">The loader that to invoke.</param>
		/// <returns>A <typeparamref name="T"/> with all loaded data, or an <see cref="ErrMsg{TCode}"/> describing an error which prevented the data from being loaded.</returns>
		public static async Task<ValOrErr<T, ErrMsg<LoadError>>> TryLoadAsync<T>(string path, Encoding encoding, ICfgLoaderAsync<T> loader)
			where T : class
		{
			using CfgStreamReader scr = new(new StreamReader(path, encoding), leaveOpen: false);
			return await loader.TryLoadAsync(scr);
		}
		/// <summary>
		/// Loads section config data from <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <param name="loader">The loader that to invoke.</param>
		/// <returns>A <typeparamref name="T"/> with all loaded data, or an <see cref="ErrMsg{TCode}"/> describing an error which prevented the data from being loaded.</returns>
		public static async Task<ValOrErr<T, ErrMsg<LoadError>>> TryLoadAsync<T>(TextReader reader, ICfgLoaderAsync<T> loader)
			where T : class
		{
			using CfgStreamReader scr = new(reader, leaveOpen: false);
			return await loader.TryLoadAsync(scr);
		}
		/// <summary>
		/// Loads section config data from <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <param name="loader">The loader that to invoke.</param>
		/// <returns>A <typeparamref name="T"/> with all loaded data, or an <see cref="ErrMsg{TCode}"/> describing an error which prevented the data from being loaded.</returns>
		public static async Task<ValOrErr<T, ErrMsg<LoadError>>> TryLoadAsync<T>(StreamReader reader, ICfgLoaderAsync<T> loader)
			where T : class
		{
			using CfgStreamReader scr = new(reader, leaveOpen: false);
			return await loader.TryLoadAsync(scr);
		}
	}
}
