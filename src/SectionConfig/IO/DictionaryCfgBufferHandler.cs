#if NET6_0_OR_GREATER
namespace SectionConfig.IO
{
	using System;
	using System.Buffers;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	internal ref struct DictionaryCfgBufferHandler
	{
		private readonly Stack<string> keys;
		private StringBuilder? multiline;
		private IList<string>? currentList;
		private readonly char keySeparator;
		internal DictionaryCfgBufferHandler(Dictionary<string, Strings> dictionary, char keySeparator)
		{
			Dictionary = dictionary;
			keys = new();
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
		internal bool Handle(CfgBufToken token, scoped CfgBufferReader cbr)
		{
			switch (token)
			{
				default:
				case CfgBufToken.Key:
				case CfgBufToken.Comment:
				case CfgBufToken.NeedMoreData:
					return true;
				case CfgBufToken.Value:
					if (multiline == null)
					{
						if (currentList != null)
						{
							currentList.Add(cbr.Content.ToString());
							return true;
						}
						else
						{
							return TryAddToDictionary(FullKey(cbr.Key.KeyString), cbr.Content.ToString());
						}
					}
					else
					{
						multiline.Append(cbr.LeadingNewLine);
						multiline.Append(cbr.Content);
						return true;
					}

				case CfgBufToken.StartList:
					currentList = new List<string>();
					return true;
				case CfgBufToken.EndList:
					Strings s = currentList?.ToArray() ?? Array.Empty<string>();
					currentList = null;
					return TryAddToDictionary(FullKey(cbr.Key.KeyString), s);

				case CfgBufToken.StartMultiline:
					multiline = new();
					return true;
				case CfgBufToken.EndMultiline:
					string multilineText = multiline?.ToString() ?? string.Empty;
					multiline = null;
					return TryAddToDictionary(FullKey(cbr.Key.KeyString), multilineText);

				case CfgBufToken.StartSection:
					keys.Push(cbr.Key.KeyString);
					return true;
				case CfgBufToken.EndSection:
					keys.Pop();
					return true;

				case CfgBufToken.End:
					return false;
				case CfgBufToken.Error:
					Error = new ErrMsg<LoadError>(LoadError.MalformedStream, cbr.Content.ToString());
					return false;
			}
		}
		private string FullKey(string key)
		{
			keys.Push(key);
			string k = string.Join(keySeparator, keys.Reverse());
			keys.Pop();
			return k;
		}
	}
}
#endif