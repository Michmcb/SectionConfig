namespace SectionConfig
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;

	/// <summary>
	/// Merges many <see cref="CfgRoot"/>s into a single <see cref="CfgRoot"/>.
	/// </summary>
	public sealed class CfgMerger
	{
		private MergeBehaviour sectionMerging;
		private MergeBehaviour valueMerging;
		private MergeBehaviour valueListMerging;
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="keyComparer">The equality comparer to use for keys.</param>
		/// <param name="sectionMerging">How to merge <see cref="CfgSection"/>. By default, <see cref="MergeBehaviour.TakeBoth"/>.</param>
		/// <param name="valueMerging">How to merge <see cref="CfgValue"/>. By default, <see cref="MergeBehaviour.Fail"/>.</param>
		/// <param name="valueListMerging">How to merge <see cref="CfgValueList"/>. By default, <see cref="MergeBehaviour.Fail"/>.</param>
		/// <param name="mergeValuesAndValueLists">Whether or not to merge <see cref="CfgValue"/> and <see cref="CfgValueList"/>. By default, false.</param>
		public CfgMerger(IEqualityComparer<string> keyComparer, MergeBehaviour sectionMerging = MergeBehaviour.TakeBoth, MergeBehaviour valueMerging = MergeBehaviour.Fail, MergeBehaviour valueListMerging = MergeBehaviour.Fail, bool mergeValuesAndValueLists = false)
		{
			KeyComparer = keyComparer;
			if (IsInvalid(sectionMerging)) { throw new ArgumentOutOfRangeException(nameof(sectionMerging), "Not a valid enum value"); }
			if (IsInvalid(valueMerging)) { throw new ArgumentOutOfRangeException(nameof(valueMerging), "Not a valid enum value"); }
			if (IsInvalid(valueListMerging)) { throw new ArgumentOutOfRangeException(nameof(valueListMerging), "Not a valid enum value"); }
			this.sectionMerging = sectionMerging;
			this.valueMerging = valueMerging;
			this.valueListMerging = valueListMerging;
			MergeValuesAndValueLists = mergeValuesAndValueLists;
		}
		/// <summary>
		/// The key comparer to use.
		/// </summary>
		public IEqualityComparer<string> KeyComparer { get; set; }
		/// <summary>
		/// <see cref="MergeBehaviour.Fail"/> will cause a failure if a duplicate section is found.
		/// <see cref="MergeBehaviour.TakeBoth"/> will take the elements of both sections.
		/// <see cref="MergeBehaviour.TakeFirst"/> will only keep elements of the existing section.
		/// <see cref="MergeBehaviour.TakeLast"/> will only keep elements of the incoming section.
		/// </summary>
		public MergeBehaviour SectionMerging
		{
			get => sectionMerging;
			set
			{
				if (IsInvalid(value)) { throw new ArgumentOutOfRangeException(nameof(value), "Not a valid enum value"); }
				sectionMerging = value;
			}
		}
		/// <summary>
		/// <see cref="MergeBehaviour.Fail"/> will cause a failure if a duplicate value is found.
		/// <see cref="MergeBehaviour.TakeBoth"/> will change the values into a value list, which contains both values. If using this value, it's suggested to set <see cref="MergeValuesAndValueLists"/> to true.
		/// <see cref="MergeBehaviour.TakeFirst"/> will keep the existing value.
		/// <see cref="MergeBehaviour.TakeLast"/> will keep the incoming value.
		/// </summary>
		public MergeBehaviour ValueMerging
		{
			get => valueMerging;
			set
			{
				if (IsInvalid(value)) { throw new ArgumentOutOfRangeException(nameof(value), "Not a valid enum value"); }
				valueMerging = value;
			}
		}
		/// <summary>
		/// <see cref="MergeBehaviour.Fail"/> will cause a failure if a duplicate value is found.
		/// <see cref="MergeBehaviour.TakeBoth"/> adds the values in the incoming list to the existing list.
		/// <see cref="MergeBehaviour.TakeFirst"/> will keep the existing value list.
		/// <see cref="MergeBehaviour.TakeLast"/> will keep the incoming value list.
		/// </summary>
		public MergeBehaviour ValueListMerging
		{
			get => valueListMerging;
			set
			{
				if (IsInvalid(value)) { throw new ArgumentOutOfRangeException(nameof(value), "Not a valid enum value"); }
				valueListMerging = value;
			}
		}
		/// <summary>
		/// If true, then any values found that conflict with a value list will be added to the value list.
		/// This is useful when <see cref="ValueMerging"/> is set to <see cref="MergeBehaviour.TakeBoth"/>.
		/// </summary>
		public bool MergeValuesAndValueLists { get; set; }
		/// <summary>
		/// Merges all of the provided <paramref name="rootObjects"/> into a new <see cref="CfgRoot"/>.
		/// </summary>
		/// <param name="rootObjects">The array of <see cref="ICfgObjectParent"/> to merge.</param>
		/// <returns>Either a new <see cref="CfgRoot"/>, or an error.</returns>
		public ValOrErr<CfgRoot, ErrMsg<MergeError>> MergeAll(params ICfgObjectParent[] rootObjects)
		{
			return MergeAll((IEnumerable<ICfgObjectParent>)rootObjects);
		}
		/// <summary>
		/// Merges all of the provided <paramref name="rootObjects"/> into a new <see cref="CfgRoot"/>.
		/// </summary>
		/// <param name="rootObjects">The <see cref="ICfgObjectParent"/>s to merge.</param>
		/// <returns>Either a new <see cref="CfgRoot"/>, or an error.</returns>
		public ValOrErr<CfgRoot, ErrMsg<MergeError>> MergeAll(IEnumerable<ICfgObjectParent> rootObjects)
		{
			CfgRoot root = new(KeyComparer);
			foreach (ICfgObjectParent otherRoot in rootObjects)
			{
				ErrMsg<MergeError> m = MergeDonorIntoRecipient(root, otherRoot);
				if (m.Code != MergeError.Ok)
				{
					return new(m);
				}
			}
			return new(root);
		}
		/// <summary>
		/// Recursively merges all elements from <paramref name="donor"/> into <paramref name="recipient"/>.
		/// </summary>
		/// <param name="recipient">The <see cref="ICfgObjectParent"/> that will receive elements. This is considered the first element.</param>
		/// <param name="donor">The <see cref="ICfgObjectParent"/> from which elements are taken. This is considered the last element.</param>
		public ErrMsg<MergeError> MergeDonorIntoRecipient(ICfgObjectParent recipient, ICfgObjectParent donor)
		{
			foreach (ICfgObject incoming in donor.Elements.Values)
			{
				if (recipient.Elements.TryGetValue(incoming.Key.KeyString, out ICfgObject? existing))
				{
					// This is a duplicate so we need to resolve it
					ErrMsg<MergeError> m = HandleDuplicate(recipient, existing, incoming);
					if (m.Code != MergeError.Ok)
					{
						return m;
					}
				}
				else
				{
					// No duplicate, just add it, but we need to create a copy to do so
					// None of these adds should fail, because we know that the recipient doesn't have that key, and we just created a new element, which are the only two failure cases.
					AddError ae;
					switch (incoming.Type)
					{
						case CfgType.Value:
							ae = recipient.TryAdd(new CfgValue(incoming.Key, incoming.ToValue().Value));
							Debug.Assert(ae == AddError.Ok, "A newly-created CfgValue was added to a recipient which does not have that key, and it failed!");
							break;
						case CfgType.ValueList:
							ae = recipient.TryAdd(new CfgValueList(incoming.Key, new List<string>(incoming.ToValueList().Values)));
							Debug.Assert(ae == AddError.Ok, "A newly-created CfgValueList was added to a recipient which does not have that key, and it failed!");
							break;
						case CfgType.Section:
							CfgSection section = new(incoming.Key, KeyComparer);
							ae = recipient.TryAdd(section);
							Debug.Assert(ae == AddError.Ok, "A newly-created CfgSection was added to a recipient which does not have that key, and it failed!");
							ErrMsg<MergeError> m = MergeDonorIntoRecipient(section, incoming.ToSection());
							if (m.Code != MergeError.Ok)
							{
								return m;
							}
							break;
					}
				}
			}
			return default;
		}
		/// <summary>
		/// Merges <paramref name="existing"/> and <paramref name="incoming"/> into <paramref name="target"/> according to the configured rules.
		/// </summary>
		/// <param name="target">The <see cref="ICfgObjectParent"/> into which <paramref name="incoming"/> is being merged.</param>
		/// <param name="existing">The object that already exists in <paramref name="target"/>.</param>
		/// <param name="incoming">The object coming from a different <see cref="ICfgObjectParent"/>.</param>
		/// <returns><see cref="MergeError.Ok"/> on success, any other error code otherwise.</returns>
		/// <exception cref="InvalidOperationException">If any <see cref="CfgType"/> values are not valid enum values.</exception>
		private ErrMsg<MergeError> HandleDuplicate(ICfgObjectParent target, ICfgObject existing, ICfgObject incoming)
		{
			AddError ae;
			if (existing.Type != incoming.Type)
			{
				if (MergeValuesAndValueLists)
				{
					if (existing.IsValueList(out CfgValueList? existingList) && incoming.IsValue(out CfgValue? incomingValue))
					{
						existingList.Values.Add(incomingValue.Value);
						return default;
					}
					else if (incoming.IsValueList(out CfgValueList? incomingList) && existing.IsValue(out CfgValue? existingValue))
					{
						target.Remove(existing.Key);
						List<string> newList = new(incomingList.Values.Count + 1);
						newList.Add(existingValue.Value);
						newList.AddRange(incomingList.Values);
						ae = target.TryAdd(new CfgValueList(incomingList.Key, newList));
						Debug.Assert(ae == AddError.Ok, "A newly-created CfgValueList was added to a recipient which does not have that key, and it failed!");
						return default;
					}
				}
				return new(MergeError.MismatchingTypes, string.Concat("Merging the duplicate key ", existing.Key.KeyString, " failed, because they are different types. Existing is ", existing.Type.ToString(), " whereas incoming is ", incoming.Type.ToString()));
			}
			switch (existing.Type)
			{
				case CfgType.Value:
					switch (ValueMerging)
					{
						case MergeBehaviour.Fail:
							return new(MergeError.DuplicateValue, string.Concat("Merging the duplicate value with key ", existing.Key.KeyString, " failed because no duplicates are allowed"));
						case MergeBehaviour.TakeFirst:
							return default;
						case MergeBehaviour.TakeLast:
							target.Remove(existing.Key);
							ae = target.TryAdd(new CfgValue(incoming.Key, incoming.ToValue().Value));
							Debug.Assert(ae == AddError.Ok, "A newly-created CfgValue was added to a recipient which does not have that key, and it failed!");
							return default;
						case MergeBehaviour.TakeBoth:
							target.Remove(existing.Key);
							ae = target.TryAdd(new CfgValueList(existing.Key, new List<string>()
								{
									existing.ToValue().Value,
									incoming.ToValue().Value,
								}));
							Debug.Assert(ae == AddError.Ok, "A newly-created CfgValueList was added to a recipient which does not have that key, and it failed!");
							return default;
						default: throw new InvalidOperationException(nameof(ValueMerging) + " is not a valid enum value");
					}
				case CfgType.ValueList:
					switch (ValueListMerging)
					{
						case MergeBehaviour.Fail:
							return new(MergeError.DuplicateValueList, string.Concat("Merging the duplicate value list with key ", existing.Key.KeyString, " failed because no duplicates are allowed"));
						case MergeBehaviour.TakeFirst:
							return default;
						case MergeBehaviour.TakeLast:
							target.Remove(existing.Key);
							ae = target.TryAdd(new CfgValueList(incoming.Key, new List<string>(incoming.ToValueList().Values)));
							Debug.Assert(ae == AddError.Ok, "A newly-created CfgValueList was added to a recipient which does not have that key, and it failed!");
							return default;
						case MergeBehaviour.TakeBoth:
							target.Remove(existing.Key);
							IList<string> eList = existing.ToValueList().Values;
							IList<string> iList = incoming.ToValueList().Values;
							List<string> newList = new(eList.Count + iList.Count);
							newList.AddRange(eList);
							newList.AddRange(iList);
							ae = target.TryAdd(new CfgValueList(existing.Key, newList));
							Debug.Assert(ae == AddError.Ok, "A newly-created CfgValueList was added to a recipient which does not have that key, and it failed!");
							return default;
						default: throw new InvalidOperationException(nameof(ValueListMerging) + " is not a valid enum value");
					}
				case CfgType.Section:
					switch (SectionMerging)
					{
						case MergeBehaviour.Fail:
							return new(MergeError.DuplicateSection, string.Concat("Merging the duplicate section with key ", existing.Key.KeyString, " failed because no duplicates are allowed"));
						case MergeBehaviour.TakeFirst:
							return default;
						case MergeBehaviour.TakeLast:
							target.Remove(existing.Key);
							CfgSection newSection = new(incoming.Key, KeyComparer);
							ae = target.TryAdd(newSection);
							Debug.Assert(ae == AddError.Ok, "A newly-created CfgSection was added to a recipient which does not have that key, and it failed!");
							return MergeDonorIntoRecipient(newSection, incoming.ToSection());
						case MergeBehaviour.TakeBoth:
							return MergeDonorIntoRecipient(existing.ToSection(), incoming.ToSection());
						default: throw new InvalidOperationException(nameof(SectionMerging) + " is not a valid enum value");
					}
				default: throw new InvalidOperationException(string.Concat("The key ", existing.Key.KeyString, " has an invalid " + nameof(CfgType)));
			}
		}
		private static bool IsInvalid(MergeBehaviour mb)
		{
			return mb switch
			{
				MergeBehaviour.Fail => false,
				MergeBehaviour.TakeFirst => false,
				MergeBehaviour.TakeLast => false,
				MergeBehaviour.TakeBoth => false,
				_ => true,
			};
		}
	}
}
