namespace SectionConfig.IO
{
#if NETSTANDARD2_0
	using SectionConfig.Shims;
#endif
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Loads config data into a <see cref="Dictionary{TKey, TValue}"/>, with keys separated with <see cref="KeySeparator"/>.
	/// </summary>
	public sealed class DictionaryCfgLoader : ICfgLoader<Dictionary<string, string>>
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
		public ValOrErr<Dictionary<string, string>, ErrMsg<LoadError>> TryLoad(CfgStreamReader reader)
		{
			Dictionary<string, string> d = new(KeyComparer);
			Stack<string> keys = new();
			int listIndex = 0;
			string fullKey;
			while (true)
			{
				ReadResult rr = reader.Read();
				switch (rr.Token)
				{
					case SectionCfgToken.Value:
						fullKey = FullKey(keys, rr.Key.KeyString, KeySeparator);
						if (!d.TryAdd(fullKey, rr.Content))
						{
							return new(new ErrMsg<LoadError>(LoadError.DuplicateKey, string.Concat("Duplicate key \"", fullKey, "\" was found")));
						}
						break;
					case SectionCfgToken.StartList:
						keys.Push(rr.Key.KeyString);
						listIndex = 0;
						break;
					case SectionCfgToken.ListValue:
						fullKey = FullKey(keys, listIndex.ToString(), KeySeparator);
						if (!d.TryAdd(fullKey, rr.Content))
						{
							return new(new ErrMsg<LoadError>(LoadError.DuplicateKey, string.Concat("Duplicate key \"", fullKey, "\" was found")));
						}
						++listIndex;
						break;
					case SectionCfgToken.EndList:
						keys.Pop();
						listIndex = 0;
						break;
					case SectionCfgToken.StartSection:
						keys.Push(rr.Key.KeyString);
						break;
					case SectionCfgToken.EndSection:
						keys.Pop();
						break;
					case SectionCfgToken.End:
						return new(d);
					case SectionCfgToken.Error:
						return new(new ErrMsg<LoadError>(LoadError.MalformedStream, rr.Content));
				}
			}

			static string FullKey(Stack<string> keys, string key, char keySeparator)
			{
				keys.Push(key);
#if NETSTANDARD2_0
				string k = string.Join(keySeparator.ToString(), keys.Reverse());
#else
				string k = string.Join(keySeparator, keys.Reverse());
#endif
				keys.Pop();
				return k;
			}
		}
	}
}
