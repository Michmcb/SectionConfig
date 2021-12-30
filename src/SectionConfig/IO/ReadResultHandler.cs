namespace SectionConfig.IO
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	internal sealed class ReadResultHandler
	{
#if NETSTANDARD2_0
		private const string KeySep = ":";
#else
		private const char KeySep = ':';
#endif
		private readonly CfgRoot root;
		private ICfgObjectParent section;
		private CfgValueList valueList;
		private CfgKey key;
		private readonly Stack<ICfgObjectParent> parentSections;
		public ReadResultHandler(CfgRoot root)
		{
			this.root = root;
			section = root;
			key = default;
			valueList = null!;
			parentSections = new();
		}
		public ValOrErr<CfgRoot, ErrMsg<LoadError>> Result { get; private set; }
		/// <summary>
		/// Returns true if end or error encountered, false otherwise.
		/// </summary>
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
						parentSections.Push(section);
						Result = new(new ErrMsg<LoadError>(LoadError.DuplicateKey, DuplicateKeyErrorMsg(parentSections, key.KeyString)));
						return true;
					}
					break;
				case SectionCfgToken.Comment:
					break;
				case SectionCfgToken.StartList:
					valueList = new(key, new List<string>());
					if (section.TryAdd(valueList) != AddError.Ok)
					{
						parentSections.Push(section);
						Result = new(new ErrMsg<LoadError>(LoadError.DuplicateKey, DuplicateKeyErrorMsg(parentSections, key.KeyString)));
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
					CfgSection newSection = new(key, root.KeyComparer);
					if (section.TryAdd(newSection) != AddError.Ok)
					{
						parentSections.Push(section);
						Result = new(new ErrMsg<LoadError>(LoadError.DuplicateKey, DuplicateKeyErrorMsg(parentSections, key.KeyString)));
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
					Result = new(new ErrMsg<LoadError>(LoadError.MalformedStream, rr.GetContent()));
					return true;
			}
			return false;

			static string DuplicateKeyErrorMsg(Stack<ICfgObjectParent> parentSections, string currentKey)
			{
				// The bottom object in the stack is always the CfgRoot object.
				// And, we want to write things in order of bottom to top.
				// So, we reverse the iteration order, then skip the first object, which will be the CfgRoot object.
				// Then we only have the CfgSections left and can join them all together, and append the current key onto the end.
				return string.Concat("Duplicate key \"", string.Join(KeySep, parentSections.Reverse().Skip(1).Select(x => ((CfgSection)x).Key.KeyString)), KeySep, currentKey, "\" was found");
			}
		}
	}
}
