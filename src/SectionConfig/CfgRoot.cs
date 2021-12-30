namespace SectionConfig
{
	using System.Collections.Generic;

	/// <summary>
	/// The root object, which can contain other <see cref="ICfgObject"/>.
	/// </summary>
	public sealed class CfgRoot : ICfgObjectParent
	{
		private readonly Dictionary<string, ICfgObject> _elements;
		/// <summary>
		/// Creates a new instance, with <see cref="Elements"/> using the provided equality comparer.
		/// </summary>
		/// <param name="keyComparer">The equality comparer to use for keys.</param>
		public CfgRoot(IEqualityComparer<string> keyComparer)
		{
			_elements = new(keyComparer);
			KeyComparer = keyComparer;
		}
		/// <summary>
		/// The key comparer this was constructed with.
		/// </summary>
		public IEqualityComparer<string> KeyComparer { get; }
		/// <summary>
		/// The <see cref="ICfgObject"/> that this contains.
		/// </summary>
		public IReadOnlyDictionary<string, ICfgObject> Elements => _elements;
		/// <summary>
		/// Removes the <see cref="ICfgObject"/> with the provided <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key to remove.</param>
		/// <returns>True if the object was removed, false otherwise.</returns>
		public bool Remove(string key)
		{
			// TODO make some Remove methods which accept the object itself
			// We need to set the hasParent flag to false
			if (_elements.TryGetValue(key, out ICfgObject? element))
			{
				_elements.Remove(key);
				switch (element.Type)
				{
					case CfgType.Value:
						element.ToValue().hasParent = false;
						break;
					case CfgType.ValueList:
						element.ToValueList().hasParent = false;
						break;
					case CfgType.Section:
						element.ToSection().hasParent = false;
						break;
				}
				return true;
			}
			else return false;
		}
		/// <summary>
		/// Removes the <see cref="ICfgObject"/> with the provided <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key to remove.</param>
		/// <returns>True if the object was removed, false otherwise.</returns>
		public bool Remove(CfgKey key)
		{
			return Remove(key.KeyString);
		}
		/// <summary>
		/// Attempts to add <paramref name="section"/> to this.
		/// </summary>
		/// <param name="section">The section to add.</param>
		/// <returns>The result of trying to add the section.</returns>
		public AddError TryAdd(CfgSection section)
		{
			if (section.hasParent) return AddError.AlreadyHasDifferentParent;
			if (_elements.ContainsKey(section.Key.KeyString))
			{
				return AddError.KeyAlreadyExists;
			}
			else
			{
				_elements[section.Key.KeyString] = section;
				section.hasParent = true;
				return AddError.Ok;
			}
		}
		/// <summary>
		/// Attempts to add <paramref name="list"/> to this.
		/// </summary>
		/// <param name="list">The value list to add.</param>
		/// <returns>The result of trying to add the value list.</returns>
		public AddError TryAdd(CfgValueList list)
		{
			if (list.hasParent) return AddError.AlreadyHasDifferentParent;
			if (_elements.ContainsKey(list.Key.KeyString))
			{
				return AddError.KeyAlreadyExists;
			}
			else
			{
				_elements[list.Key.KeyString] = list;
				list.hasParent = true;
				return AddError.Ok;
			}
		}
		/// <summary>
		/// Attempts to add <paramref name="value"/> to this.
		/// </summary>
		/// <param name="value">The value to add.</param>
		/// <returns>The result of trying to add the value.</returns>
		public AddError TryAdd(CfgValue value)
		{
			if (value.hasParent) return AddError.AlreadyHasDifferentParent;
			if (_elements.ContainsKey(value.Key.KeyString))
			{
				return AddError.KeyAlreadyExists;
			}
			else
			{
				_elements[value.Key.KeyString] = value;
				value.hasParent = true;
				return AddError.Ok;
			}
		}
		/// <summary>
		/// Returns a <see cref="CfgValue"/> if <paramref name="key"/> exists in <see cref="Elements"/>, and it is a <see cref="CfgValue"/>.
		/// Otherwise, returns null.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>The <see cref="CfgValue"/> or null if it does not exist or is not the correct type.</returns>
		public CfgValue? TryGetValue(string key)
		{
			return Elements.TryGetValue(key, out ICfgObject? obj) ? obj.AsValue() : null;
		}
		/// <summary>
		/// Returns a <see cref="CfgValueList"/> if <paramref name="key"/> exists in <see cref="Elements"/>, and it is a <see cref="CfgValueList"/>.
		/// Otherwise, returns null.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>The <see cref="CfgValueList"/> or null if it does not exist or is not the correct type.</returns>
		public CfgValueList? TryGetValueList(string key)
		{
			return Elements.TryGetValue(key, out ICfgObject? obj) ? obj.AsValueList() : null;
		}
		/// <summary>
		/// Returns a <see cref="CfgSection"/> if <paramref name="key"/> exists in <see cref="Elements"/>, and it is a <see cref="CfgSection"/>.
		/// Otherwise, returns null.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>The <see cref="CfgSection"/> or null if it does not exist or is not the correct type.</returns>
		public CfgSection? TryGetSection(string key)
		{
			return Elements.TryGetValue(key, out ICfgObject? obj) ? obj.AsSection() : null;
		}
	}
}
