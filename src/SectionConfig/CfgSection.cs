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
		/// The key.
		/// </summary>
		public CfgKey Key { get; }
		/// <summary>
		/// The <see cref="ICfgObject"/>s that this contains.
		/// </summary>
		public IReadOnlyDictionary<string, ICfgObject> Elements => _elements;
		/// <summary>
		/// Removes the <see cref="ICfgObject"/> with the provided <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key to remove.</param>
		/// <returns>True if the object was removed, false otherwise.</returns>
		public bool Remove(string key)
		{
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
			if (section.hasParent) return AddError.AlreadyHasParent;
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
			if (list.hasParent) return AddError.AlreadyHasParent;
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
			if (value.hasParent) return AddError.AlreadyHasParent;
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
		/// <summary>
		/// Searches down through <see cref="CfgSection"/>, and returns the <see cref="ICfgObject"/> found, if any.
		/// For example if you pass [Section, Child], it would try to find "Section", and then if it that is a <see cref="CfgSection"/>, searches in that for something with the key "Child".
		/// If <paramref name="keys"/> is empty, returns null.
		/// </summary>
		/// <param name="keys">The keys to search for.</param>
		/// <returns>An <see cref="ICfgObject"/> if found, otherwise null.</returns>
		public ICfgObject? Find(IEnumerable<string> keys)
		{
			ICfgObject? result = this;
			foreach (string key in keys)
			{
				// To try and search for another child, the current object needs to be a section, and it needs to hold something with that key
				// On the last iteration, it's fine if result is not a section
				if (!result.IsSection(out CfgSection? sec) || !sec._elements.TryGetValue(key, out result))
				{
					// Either not a section or not found, so return null
					return null;
				}
				// Otherwise we're all good, keep going
			}
			// If result is still equal to this, then that means we never iterated at all.
			// To keep behaviour consistent with CfgRoot (which cannot return itself because it does not implement ICfgObject), we return null on an empty IEnumerable.
			return result == this ? null : result;
		}
	}
}
