namespace SectionConfig.IO
{
	using System;
	using System.Collections.Generic;

	// TODO make async variations of Cfg loading...but to do that we need to basically clone every single method on SectionCfgReader and SectionCfgWriter
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
			CfgRootCfgLoaderResultHandler handler = new(new CfgRoot(KeyComparer));
			while (true)
			{
				ReadResult rr = reader.Read();
				if (handler.Handle(rr))
				{
					return handler.Result;
				}
			}
		}
	}
}
