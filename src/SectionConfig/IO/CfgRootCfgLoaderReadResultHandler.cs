namespace SectionConfig.IO
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	internal struct CfgRootCfgLoaderReadResultHandler
	{
#if NETSTANDARD2_0
		private const string KeySep = ":";
#else
		private const char KeySep = ':';
#endif
		private readonly CfgRoot root;
		private ICfgObjectParent section;
		private CfgValueList valueList;
		private readonly Stack<ICfgObjectParent> parentSections;
		internal CfgRootCfgLoaderReadResultHandler(CfgRoot root)
		{
			this.root = root;
			section = root;
			valueList = null!;
			parentSections = new();
		}
		internal ValOrErr<CfgRoot, ErrMsg<LoadError>> Result { get; private set; }
		/// <summary>
		/// Returns false if error or end encountered, true otherwise.
		/// </summary>
		internal bool Handle(ReadResult rr)
		{
			switch (rr.Token)
			{
				case SectionCfgToken.Value:
					CfgValue value = new(rr.Key, rr.Content);
					if (section.TryAdd(value) != AddError.Ok)
					{
						parentSections.Push(section);
						Result = new(new ErrMsg<LoadError>(LoadError.DuplicateKey, DuplicateKeyErrorMsg(parentSections, rr.Key.KeyString)));
						return false;
					}
					return true;
				case SectionCfgToken.Comment:
					break;
				case SectionCfgToken.StartList:
					valueList = new(rr.Key, new List<string>());
					if (section.TryAdd(valueList) != AddError.Ok)
					{
						parentSections.Push(section);
						Result = new(new ErrMsg<LoadError>(LoadError.DuplicateKey, DuplicateKeyErrorMsg(parentSections, rr.Key.KeyString)));
						return false;
					}
					break;
				case SectionCfgToken.ListValue:
					valueList.Values.Add(rr.Content);
					break;
				case SectionCfgToken.EndList:
					break;
				case SectionCfgToken.StartSection:
					parentSections.Push(section);
					CfgSection newSection = new(rr.Key, root.KeyComparer);
					if (section.TryAdd(newSection) != AddError.Ok)
					{
						Result = new(new ErrMsg<LoadError>(LoadError.DuplicateKey, DuplicateKeyErrorMsg(parentSections, rr.Key.KeyString)));
						return false;
					}
					section = newSection;
					break;
				case SectionCfgToken.EndSection:
					section = parentSections.Pop();
					break;
				case SectionCfgToken.End:
					Result = new(root);
					return false;
				default:
				case SectionCfgToken.Error:
					Result = new(new ErrMsg<LoadError>(LoadError.MalformedStream, rr.Content));
					return false;
			}
			return true;
		}
		private static string DuplicateKeyErrorMsg(Stack<ICfgObjectParent> parentSections, string currentKey)
		{
			// And, we want to write things in order of bottom to top.
			// So, we reverse the iteration order, then skip the first object, which will be the CfgRoot object.
			// Then we only have the CfgSections left and can join them all together, and append the current key onto the end.
			return string.Concat("Duplicate key \"", string.Join(KeySep, parentSections.Reverse().Skip(1).Select(x => ((CfgSection)x).Key.KeyString)), KeySep, currentKey, "\" was found");
		}
	}
}

