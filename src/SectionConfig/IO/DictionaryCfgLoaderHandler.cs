namespace SectionConfig.IO
{
#if NETSTANDARD2_0
	using SectionConfig.Shims;
#endif
	using System;
	using System.Collections.Generic;
	using System.Linq;
	internal struct DictionaryCfgLoaderHandler
	{
		private readonly Stack<string> keys;
		private IList<string> currentList;
		private readonly char keySeparator;
		internal DictionaryCfgLoaderHandler(Dictionary<string, Strings> dictionary, char keySeparator)
		{
			Dictionary = dictionary;
			keys = new();
			currentList = Array.Empty<string>();
			this.keySeparator = keySeparator;
		}
		internal Dictionary<string, Strings> Dictionary { get; }
		internal ErrMsg<LoadError> Error { get; private set; }
		private bool TryAddToDictionary(string fullKey, Strings content)
		{
			if (!Dictionary.TryAdd(fullKey, content))
			{
				Error = new ErrMsg<LoadError>(LoadError.DuplicateKey, string.Concat("Duplicate key \"", fullKey, "\" was found"));
				return false;
			}
			return true;
		}
		internal bool Handle(ReadResult rr)
		{
			switch (rr.Token)
			{
				default:
				case SectionCfgToken.Comment:
					return true;
				case SectionCfgToken.Value:
					return TryAddToDictionary(FullKey(rr.Key.KeyString), rr.Content);
				case SectionCfgToken.StartList:
					currentList = new List<string>();
					return true;
				case SectionCfgToken.ListValue:
					currentList.Add(rr.Content);
					return true;
				case SectionCfgToken.EndList:
					return TryAddToDictionary(FullKey(rr.Key.KeyString), currentList.ToArray());
				case SectionCfgToken.StartSection:
					keys.Push(rr.Key.KeyString);
					return true;
				case SectionCfgToken.EndSection:
					keys.Pop();
					return true;
				case SectionCfgToken.End:
					return false;
				case SectionCfgToken.Error:
					Error = new ErrMsg<LoadError>(LoadError.MalformedStream, rr.Content);
					return false;
			}
		}
		private string FullKey(string key)
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
