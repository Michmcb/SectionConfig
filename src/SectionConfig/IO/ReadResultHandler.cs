namespace SectionConfig.IO
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	internal sealed class ReadResultHandler
	{
		private readonly CfgRoot root;
		private readonly IEqualityComparer<string> keyComparer;
		private ICfgObjectParent section;
		private CfgValueList valueList;
		private CfgKey key;
		private readonly Stack<ICfgObjectParent> parentSections;
		public ReadResultHandler(CfgRoot root, IEqualityComparer<string> keyComparer)
		{
			this.root = root;
			this.keyComparer = keyComparer;
			section = root;
			key = default;
			valueList = null!;
			parentSections = new();
		}
		public LoadResult Result { get; private set; }
		internal bool Handle(ReadResult rr)
		{
			switch (rr.Token)
			{
				case SectionCfgToken.Key:
					key = rr.GetKey();
					break;
				case SectionCfgToken.Value:
					CfgValue value = new(key, rr.GetContent());
					if (section.TryAdd(value) != AddError.Ok)
					{
						Result = new(LoadError.DuplicateKey, string.Concat("Duplicate key \"", string.Join(':', parentSections.Reverse().Skip(1).Select(x => ((CfgSection)x).Key.KeyString)), key, "\" was found"));
						return true;
					}
					break;
				case SectionCfgToken.Comment:
					break;
				case SectionCfgToken.StartList:
					valueList = new(key, new List<string>());
					if (section.TryAdd(valueList) != AddError.Ok)
					{
						Result = new(LoadError.DuplicateKey, string.Concat("Duplicate key \"", string.Join(':', parentSections.Reverse().Skip(1).Select(x => ((CfgSection)x).Key.KeyString)), key, "\" was found"));
						return true;
					}
					break;
				case SectionCfgToken.ListValue:
					valueList.Values.Add(rr.GetContent());
					break;
				case SectionCfgToken.EndList:
					break;
				case SectionCfgToken.StartSection:
					parentSections.Push(section);
					CfgSection newSection = new(key, keyComparer);
					if (section.TryAdd(newSection) != AddError.Ok)
					{
						Result = new(LoadError.DuplicateKey, string.Concat("Duplicate key \"", string.Join(':', parentSections.Reverse().Skip(1).Select(x => ((CfgSection)x).Key.KeyString)), "\" was found"));
						return true;
					}
					section = newSection;
					break;
				case SectionCfgToken.EndSection:
					section = parentSections.Pop();
					break;
				case SectionCfgToken.End:
					Result = new(root);
					return true;
				default:
				case SectionCfgToken.Error:
					Result = new(LoadError.MalformedStream, rr.GetContent());
					return true;
			}
			return false;
		}
	}
}
