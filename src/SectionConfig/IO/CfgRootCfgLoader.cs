namespace SectionConfig.IO
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	/* TODO Having a way to handle comments would be neat. Read below for more.
	 * 
	 * Perhaps the best way is to make CfgRootCfgLoader have a handler for comments?
	 * For example the handler that just loads things into a dictionary, 
	 */
	/// <summary>
	/// Loads config data into a <see cref="CfgRoot"/>.
	/// </summary>
	public sealed class CfgRootCfgLoader : ICfgLoader<CfgRoot>
	{
		/// <summary>
		/// Creates a new instance, using <see cref="StringComparer.Ordinal"/> as the <see cref="KeyComparer"/>.
		/// </summary>
		public CfgRootCfgLoader() : this(StringComparer.Ordinal) { }
		/// <summary>
		/// </summary>
		/// <param name="keyComparer">The key comparer to use for loaded sections.</param>
		public CfgRootCfgLoader(IEqualityComparer<string> keyComparer)
		{
			KeyComparer = keyComparer;
		}
		/// <summary>
		/// The Key comparer to use for loaded sections.
		/// </summary>
		public IEqualityComparer<string> KeyComparer { get; }
		/// <inheritdoc/>
		public ValOrErr<CfgRoot, ErrMsg<LoadError>> TryLoad(CfgStreamReader reader)
		{
			CfgRootCfgLoaderReadResultHandler h = new(new CfgRoot(KeyComparer));
			while (h.Handle(reader.Read())) ;
			return h.Result;
		}
		/// <inheritdoc/>
		public async Task<ValOrErr<CfgRoot, ErrMsg<LoadError>>> TryLoadAsync(CfgStreamReader reader)
		{
			CfgRootCfgLoaderReadResultHandler h = new(new CfgRoot(KeyComparer));
			while (h.Handle(await reader.ReadAsync())) ;
			return h.Result;
		}
	}
}
