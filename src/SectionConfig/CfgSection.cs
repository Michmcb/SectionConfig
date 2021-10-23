namespace SectionConfig
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// A section which can contain other <see cref="ICfgObject"/>.
	/// </summary>
	public sealed class CfgSection : ICfgObject, ICfgObjectParent
	{
		internal bool hasParent;
		private readonly Dictionary<string, ICfgObject> _elements;
		/// <summary>
		/// Creates a new instance with the specified <paramref name="key"/>, and the <see cref="Elements"/>
		/// property will compare keys using <paramref name="keyComparer"/>.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="keyComparer"></param>
		public CfgSection(CfgKey key, IEqualityComparer<string> keyComparer)
		{
			Key = key;
			hasParent = false;
			_elements = new(keyComparer);
		}
		/// <summary>
		/// Creates a new instance with the specified <paramref name="key"/>, and the <see cref="Elements"/>
		/// property will compare keys using <paramref name="keyComparer"/>.
		/// Initializes <see cref="Elements"/> with copy of the keys and values from <paramref name="elements"/>.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="keyComparer"></param>
		/// <param name="elements"></param>
		public CfgSection(CfgKey key, IEqualityComparer<string> keyComparer, IReadOnlyDictionary<string, ICfgObject> elements)
		{
			Key = key;
			hasParent = false;
			_elements = new(keyComparer);
			foreach (KeyValuePair<string, ICfgObject> kvp in elements)
			{
				_elements[kvp.Key] = kvp.Value;
			}
		}
		/// <summary>
		/// The key.
		/// </summary>
		public CfgKey Key { get; }
		/// <summary>
		/// The <see cref="ICfgObject"/>s that this contains.
		/// </summary>
		public IReadOnlyDictionary<string, ICfgObject> Elements => _elements;
		/// <summary>
		/// Returns <see cref="CfgType.Section"/>.
		/// </summary>
		public CfgType Type => CfgType.Section;
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
		/// <summary>
		/// Returns this.
		/// </summary>
		public CfgSection? AsSection()
		{
			return this;
		}
		/// <summary>
		/// Returns null.
		/// </summary>
		public CfgValue? AsValue()
		{
			return null;
		}
		/// <summary>
		/// Returns null.
		/// </summary>
		public CfgValueList? AsValueList()
		{
			return null;
		}
		/// <summary>
		/// Returns true.
		/// </summary>
		/// <param name="section">Will be this.</param>
		public bool IsSection([NotNullWhen(true)] out CfgSection? section)
		{
			section = this;
			return true;
		}
		/// <summary>
		/// Returns false.
		/// </summary>
		/// <param name="value">Will be null.</param>
		public bool IsValue([NotNullWhen(true)] out CfgValue? value)
		{
			value = null;
			return false;
		}
		/// <summary>
		/// Returns false.
		/// </summary>
		/// <param name="list">Will be null.</param>
		public bool IsValueList([NotNullWhen(true)] out CfgValueList? list)
		{
			list = null;
			return false;
		}
		/// <summary>
		/// Returns this.
		/// </summary>
		public CfgSection ToSection()
		{
			return this;
		}
		/// <summary>
		/// Throws an <see cref="InvalidCastException"/>.
		/// </summary>
		public CfgValue ToValue()
		{
			throw new InvalidCastException("Requested " + nameof(CfgValue) + " but this is a " + nameof(CfgSection));
		}
		/// <summary>
		/// Throws an <see cref="InvalidCastException"/>.
		/// </summary>
		public CfgValueList ToValueList()
		{
			throw new InvalidCastException("Requested " + nameof(CfgValueList) + " but this is a " + nameof(CfgSection));
		}
	}
}
