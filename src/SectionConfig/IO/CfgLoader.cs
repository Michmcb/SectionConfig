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
		// Having this be an instance class would let us have CfgRoot and IEqualityComparer be instance properties. That would lessen problems.
		// We can have keep these static methods for convenience, if we just want to load a single config stream.

		// TODO make async variations of these

		/// <summary>
		/// Loads section config data from the file at <paramref name="path"/> into <paramref name="root"/>, reading using the <paramref name="encoding"/>.
		/// </summary>
		/// <param name="path">The file to read.</param>
		/// <param name="encoding">Encoding to interpet file as.</param>
		/// <param name="root">The root into which to load data, or null to create a new one.</param>
		/// <param name="keyComparer">Key comparer to use for loaded sections, or null for <see cref="StringComparer.Ordinal"/>.</param>
		/// <returns>The result of loading.</returns>
		public static LoadResult TryLoad(string path, Encoding encoding, CfgRoot? root = null, IEqualityComparer<string>? keyComparer = null)
		{
			using SectionCfgReader scr = new(new StreamReader(path, encoding), true);
			return TryLoad(scr, root, keyComparer);
		}
		/// <summary>
		/// Loads section config data from <paramref name="reader"/> into <paramref name="root"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <param name="root">The root into which to load data, or null to create a new one.</param>
		/// <param name="keyComparer">Key comparer to use for loaded sections, or null for <see cref="StringComparer.Ordinal"/>.</param>
		/// <returns>The result of loading.</returns>
		public static LoadResult TryLoad(TextReader reader, CfgRoot? root = null, IEqualityComparer<string>? keyComparer = null)
		{
			using SectionCfgReader scr = new(reader, false);
			return TryLoad(scr, root, keyComparer);
		}
		/// <summary>
		/// Loads section config data from <paramref name="reader"/> into <paramref name="root"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <param name="root">The root into which to load data, or null to create a new one.</param>
		/// <param name="keyComparer">Key comparer to use for loaded sections, or null for <see cref="StringComparer.Ordinal"/>.</param>
		/// <returns>The result of loading.</returns>
		public static LoadResult TryLoad(StreamReader reader, CfgRoot? root = null, IEqualityComparer<string>? keyComparer = null)
		{
			using SectionCfgReader scr = new(reader, false);
			return TryLoad(scr, root, keyComparer);
		}
		/// <summary>
		/// Loads section config data from <paramref name="reader"/> into <paramref name="root"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <param name="root">The root into which to load data, or null to create a new one.</param>
		/// <param name="keyComparer">Key comparer to use for loaded sections, or null for <see cref="StringComparer.Ordinal"/>.</param>
		/// <returns>The result of loading.</returns>
		public static LoadResult TryLoad(SectionCfgReader reader, CfgRoot? root = null, IEqualityComparer<string>? keyComparer = null)
		{
			keyComparer ??= StringComparer.Ordinal;
			ReadResultHandler handler = new(root ?? new(keyComparer), keyComparer);
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
