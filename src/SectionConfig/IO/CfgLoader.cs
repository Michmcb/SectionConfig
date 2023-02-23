namespace SectionConfig.IO
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	/// <summary>
	/// Helper class for loading config data from various sources.
	/// </summary>
	public static class CfgLoader
	{
		/// <summary>
		/// Loads section config data from the file at <paramref name="path"/>, reading using the <paramref name="encoding"/>.
		/// </summary>
		/// <param name="path">The file to read.</param>
		/// <param name="encoding">Encoding to interpet file as.</param>
		/// <param name="loader">The loader that to invoke.</param>
		/// <returns>A <typeparamref name="T"/> with all loaded data, or an <see cref="ErrMsg{TCode}"/> describing an error which prevented the data from being loaded.</returns>
		public static ValOrErr<T, ErrMsg<LoadError>> TryLoad<T>(string path, Encoding encoding, ICfgLoader<T> loader)
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
		public static ValOrErr<T, ErrMsg<LoadError>> TryLoad<T>(TextReader reader, ICfgLoader<T> loader)
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
		public static ValOrErr<T, ErrMsg<LoadError>> TryLoad<T>(StreamReader reader, ICfgLoader<T> loader)
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
		/// <param name="keyComparer">Key comparer to use for loaded sections, or null for <see cref="StringComparer.Ordinal"/>.</param>
		/// <returns>A <see cref="CfgRoot"/> with all loaded data, or an <see cref="ErrMsg{TCode}"/> describing an error which prevented the data from being loaded.</returns>
		[Obsolete("Prefer using a generic overload of " + nameof(TryLoad) + ", and pass in an instance of " + nameof(CfgRootCfgLoader))]
		public static ValOrErr<CfgRoot, ErrMsg<LoadError>> TryLoad(string path, Encoding encoding, IEqualityComparer<string>? keyComparer = null)
		{
			using SectionCfgReader scr = new(new StreamReader(path, encoding), closeInput: true);
			return TryLoad(scr, keyComparer);
		}
		/// <summary>
		/// Loads section config data from <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <param name="keyComparer">Key comparer to use for loaded sections, or null for <see cref="StringComparer.Ordinal"/>.</param>
		/// <returns>A <see cref="CfgRoot"/> with all loaded data, or an <see cref="ErrMsg{TCode}"/> describing an error which prevented the data from being loaded.</returns>
		[Obsolete("Prefer using a generic overload of " + nameof(TryLoad) + ", and pass in an instance of " + nameof(CfgRootCfgLoader))]
		public static ValOrErr<CfgRoot, ErrMsg<LoadError>> TryLoad(TextReader reader, IEqualityComparer<string>? keyComparer = null)
		{
			using SectionCfgReader scr = new(reader, closeInput: true);
			return TryLoad(scr, keyComparer);
		}
		/// <summary>
		/// Loads section config data from <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <param name="keyComparer">Key comparer to use for loaded sections, or null for <see cref="StringComparer.Ordinal"/>.</param>
		/// <returns>A <see cref="CfgRoot"/> with all loaded data, or an <see cref="ErrMsg{TCode}"/> describing an error which prevented the data from being loaded.</returns>
		[Obsolete("Prefer using a generic overload of " + nameof(TryLoad) + ", and pass in an instance of " + nameof(CfgRootCfgLoader))]
		public static ValOrErr<CfgRoot, ErrMsg<LoadError>> TryLoad(StreamReader reader, IEqualityComparer<string>? keyComparer = null)
		{
			using SectionCfgReader scr = new(reader, closeInput: true);
			return TryLoad(scr, keyComparer);
		}
		/// <summary>
		/// Loads section config data from <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <param name="keyComparer">Key comparer to use for loaded sections, or null for <see cref="StringComparer.Ordinal"/>.</param>
		/// <returns>A <see cref="CfgRoot"/> with all loaded data, or an <see cref="ErrMsg{TCode}"/> describing an error which prevented the data from being loaded.</returns>
		[Obsolete("Prefer creating an instance of " + nameof(CfgRootCfgLoader) + " and calling " + nameof(CfgRootCfgLoader.TryLoad) + " on that instance")]
		public static ValOrErr<CfgRoot, ErrMsg<LoadError>> TryLoad(SectionCfgReader reader, IEqualityComparer<string>? keyComparer = null)
		{
			CfgRootCfgLoader loader = new(keyComparer ?? StringComparer.Ordinal);
			return loader.TryLoad(reader.CfgStreamReader);
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
