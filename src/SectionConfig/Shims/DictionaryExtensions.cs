#if NETSTANDARD2_0
namespace SectionConfig.Shims
{
	using System;
	using System.Collections.Generic;
	/// <summary>
	/// Shims for .netstandard 2.0.
	/// </summary>
	public static class DictionaryExtensions
	{
		/// <summary>
		/// Attempts to add the specified key and value to the dictionary.
		/// </summary>
		/// <param name="dict">The dictionary to add the key/value pair to.</param>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">The value of the element to add. It can be null.</param>
		/// <exception cref="ArgumentNullException"/>
		/// <returns><see langword="true"/> if the key/value pair was added to the dictionary successfully; otherwise, <see langword="false"/>.</returns>
		public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
		{
			if (dict.ContainsKey(key))
			{
				return false;
			}
			else
			{
				dict.Add(key, value);
				return true;
			}
		}
	}
}
#endif