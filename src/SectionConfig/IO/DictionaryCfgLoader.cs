namespace SectionConfig.IO
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	/// <summary>
	/// Loads config data into a <see cref="Dictionary{TKey, TValue}"/>, with keys separated with <see cref="KeySeparator"/>.
	/// </summary>
	public sealed class DictionaryCfgLoader : ICfgLoader<Dictionary<string, Strings>>
	{
		/// <summary>
		/// </summary>
		/// <param name="keyComparer">The key comparer to use for the created dictionary.</param>
		/// <param name="keySeparator">The separator to use between individual keys to build a full dictionary key.</param>
		public DictionaryCfgLoader(IEqualityComparer<string>? keyComparer = null, char keySeparator = CfgSyntax.KeyEnd)
		{
			KeyComparer = keyComparer;
			KeySeparator = keySeparator;
		}
		/// <summary>
		/// The key comparer to use for the created dictionary.
		/// </summary>
		public IEqualityComparer<string>? KeyComparer { get; set; }
		/// <summary>
		/// The key separator to use.
		/// </summary>
		public char KeySeparator { get; }
		/// <inheritdoc/>
		public ValOrErr<Dictionary<string, Strings>, ErrMsg<LoadError>> TryLoad(CfgStreamReader reader)
		{
			DictionaryCfgLoaderHandler lh = new(new Dictionary<string, Strings>(KeyComparer), KeySeparator);
			while (lh.Handle(reader.Read())) ;
			return lh.Error.Code == LoadError.Ok ? new(lh.Dictionary) : new(lh.Error);
		}
		/// <inheritdoc/>
		public async Task<ValOrErr<Dictionary<string, Strings>, ErrMsg<LoadError>>> TryLoadAsync(CfgStreamReader reader)
		{
			DictionaryCfgLoaderHandler lh = new(new Dictionary<string, Strings>(KeyComparer), KeySeparator);
			while (lh.Handle(await reader.ReadAsync())) ;
			return lh.Error.Code == LoadError.Ok ? new(lh.Dictionary) : new(lh.Error);
		}
//#if NET6_0_OR_GREATER
//		/// <inheritdoc/>
//		public ValOrErr<Dictionary<string, Strings>, ErrMsg<LoadError>> TryLoadTextReader(TextReader reader)
//		{
//			DictionaryCfgBufferHandler handler = new(new Dictionary<string, Strings>(KeyComparer), KeySeparator);

//			//CfgBufferReader cbr = CfgBufferHelper.CreateBufferReader(initBuf);
//			//CfgBufferHelper helper = new(cbr, initBuf);
//			Span<char> initBuf = stackalloc char[512];
//			CfgBufferHelper helper = CfgBufferHelper.Create(reader, initBuf);
//			bool b = true;
//			while (b)
//			{
//				var t = helper.Read(reader);
//				b = handler.Handle(t, helper.BufferReader);
//			}
//			return handler.Error.Code == LoadError.Ok ? new(handler.Dictionary) : new(handler.Error);
//		}
//#endif
	}
}
