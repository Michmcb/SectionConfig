namespace SectionConfig
{
	using System.Collections.Generic;

	/// <summary>
	/// An object which can hold other <see cref="ICfgObject"/>s.
	/// </summary>
	public interface ICfgObjectParent
	{
		/// <summary>
		/// The <see cref="ICfgObject"/>s that this contains.
		/// </summary>
		IReadOnlyDictionary<string, ICfgObject> Elements { get; }
		/// <summary>
		/// Removes the <see cref="ICfgObject"/> with the provided <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key to remove.</param>
		/// <returns>True if the object was removed, false otherwise.</returns>
		bool Remove(CfgKey key);
		/// <summary>
		/// Removes the <see cref="ICfgObject"/> with the provided <paramref name="key"/>.
		/// </summary>
		/// <param name="key">The key to remove.</param>
		/// <returns>True if the object was removed, false otherwise.</returns>
		bool Remove(string key);
		/// <summary>
		/// Attempts to add <paramref name="value"/> to this.
		/// </summary>
		/// <param name="value">The value to add.</param>
		/// <returns>The result of trying to add the value.</returns>
		AddError TryAdd(CfgValue value);
		/// <summary>
		/// Attempts to add <paramref name="list"/> to this.
		/// </summary>
		/// <param name="list">The value list to add.</param>
		/// <returns>The result of trying to add the value list.</returns>
		AddError TryAdd(CfgValueList list);
		/// <summary>
		/// Attempts to add <paramref name="section"/> to this.
		/// </summary>
		/// <param name="section">The section to add.</param>
		/// <returns>The result of trying to add the section.</returns>
		AddError TryAdd(CfgSection section);
		/// <summary>
		/// Returns a <see cref="CfgValue"/> if <paramref name="key"/> exists in <see cref="Elements"/>, and it is a <see cref="CfgValue"/>.
		/// Otherwise, returns null.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>The <see cref="CfgValue"/> or null if it does not exist or is not the correct type.</returns>
		public CfgValue? TryGetValue(string key);
		/// <summary>
		/// Returns a <see cref="CfgValueList"/> if <paramref name="key"/> exists in <see cref="Elements"/>, and it is a <see cref="CfgValueList"/>.
		/// Otherwise, returns null.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>The <see cref="CfgValueList"/> or null if it does not exist or is not the correct type.</returns>
		public CfgValueList? TryGetValueList(string key);
		/// <summary>
		/// Returns a <see cref="CfgSection"/> if <paramref name="key"/> exists in <see cref="Elements"/>, and it is a <see cref="CfgSection"/>.
		/// Otherwise, returns null.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>The <see cref="CfgSection"/> or null if it does not exist or is not the correct type.</returns>
		public CfgSection? TryGetSection(string key);
	}
}
