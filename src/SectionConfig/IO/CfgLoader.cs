namespace SectionConfig.IO
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	/// <summary>
	/// Loads section config data from a stream or file.
	/// </summary>
	public sealed class CfgLoader
	{
		// TODO make async variations of these

		/// <summary>
		/// Loads section config data from the file at <paramref name="path"/>, reading using the <paramref name="encoding"/>.
		/// </summary>
		/// <param name="path">The file to read.</param>
		/// <param name="encoding">Encoding to interpet file as.</param>
		/// <param name="keyComparer">Key comparer to use for loaded sections, or null for <see cref="StringComparer.Ordinal"/>.</param>
		/// <returns>The result of loading.</returns>
		public static ValOrErr<CfgRoot, ErrMsg<LoadError>> TryLoad(string path, Encoding encoding, IEqualityComparer<string>? keyComparer = null)
		{
			using SectionCfgReader scr = new(new StreamReader(path, encoding), true);
			return TryLoad(scr, keyComparer);
		}
		/// <summary>
		/// Loads section config data from <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <param name="keyComparer">Key comparer to use for loaded sections, or null for <see cref="StringComparer.Ordinal"/>.</param>
		/// <returns>The result of loading.</returns>
		public static ValOrErr<CfgRoot, ErrMsg<LoadError>> TryLoad(TextReader reader, IEqualityComparer<string>? keyComparer = null)
		{
			using SectionCfgReader scr = new(reader, false);
			return TryLoad(scr, keyComparer);
		}
		/// <summary>
		/// Loads section config data from <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <param name="keyComparer">Key comparer to use for loaded sections, or null for <see cref="StringComparer.Ordinal"/>.</param>
		/// <returns>The result of loading.</returns>
		public static ValOrErr<CfgRoot, ErrMsg<LoadError>> TryLoad(StreamReader reader, IEqualityComparer<string>? keyComparer = null)
		{
			using SectionCfgReader scr = new(reader, false);
			return TryLoad(scr, keyComparer);
		}
		/// <summary>
		/// Loads section config data from <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <param name="keyComparer">Key comparer to use for loaded sections, or null for <see cref="StringComparer.Ordinal"/>.</param>
		/// <returns>The result of loading.</returns>
		public static ValOrErr<CfgRoot, ErrMsg<LoadError>> TryLoad(SectionCfgReader reader, IEqualityComparer<string>? keyComparer = null)
		{
			keyComparer ??= StringComparer.Ordinal;
			ReadResultHandler handler = new(new CfgRoot(keyComparer));
			while (true)
			{
				ReadResult rr = reader.Read();
				if (handler.Handle(rr))
				{
					return handler.Result;
				}
			}
		}
		//public static async Task<LoadResult> TryLoadAsync(SectionCfgReader reader, CfgRoot? root = null, IEqualityComparer<string>? keyComparer = null)
		//{
		//	keyComparer ??= StringComparer.Ordinal;
		//	ReadResultHandler handler = new(root ?? new(keyComparer), keyComparer);
		//	while (true)
		//	{
		//		ReadResult rr = await reader.ReadAsync();
		//		if (handler.Handle(rr))
		//		{
		//			return handler.Result;
		//		}
		//	}
		//}
	}
}
