namespace SectionConfig
{
	using System;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// A key wtih a single value.
	/// </summary>
	public sealed class CfgValue : ICfgObject
	{
		internal bool hasParent;
		/// <summary>
		/// Creates a new instance with the provided key and value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public CfgValue(CfgKey key, string value)
		{
			Key = key;
			Value = value;
		}
		/// <summary>
		/// The key.
		/// </summary>
		public CfgKey Key { get; }
		/// <summary>
		/// The string value.
		/// </summary>
		public string Value { get; set; }
		/// <summary>
		/// Returns <see cref="CfgType.Value"/>.
		/// </summary>
		public CfgType Type => CfgType.Value;
		/// <summary>
		/// Returns null.
		/// </summary>
		public CfgSection? AsSection()
		{
			return null;
		}
		/// <summary>
		/// Returns this.
		/// </summary>
		public CfgValue? AsValue()
		{
			return this;
		}
		/// <summary>
		/// Returns null.
		/// </summary>
		public CfgValueList? AsValueList()
		{
			return null;
		}
		/// <summary>
		/// Returns false.
		/// </summary>
		/// <param name="section">Will be null.</param>
		public bool IsSection([NotNullWhen(true)] out CfgSection? section)
		{
			section = null;
			return false;
		}
		/// <summary>
		/// Returns true.
		/// </summary>
		/// <param name="value">Will be this.</param>
		public bool IsValue([NotNullWhen(true)] out CfgValue? value)
		{
			value = this;
			return true;
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
		/// Throws an <see cref="InvalidCastException"/>.
		/// </summary>
		public CfgSection ToSection()
		{
			throw new InvalidCastException("Requested " + nameof(CfgSection) + " but this is a " + nameof(CfgValue));
		}
		/// <summary>
		/// Returns this.
		/// </summary>
		public CfgValue ToValue()
		{
			return this;
		}
		/// <summary>
		/// Throws an <see cref="InvalidCastException"/>.
		/// </summary>
		public CfgValueList ToValueList()
		{
			throw new InvalidCastException("Requested " + nameof(CfgValueList) + " but this is a " + nameof(CfgValue));
		}
		/// <summary>
		/// Calls <paramref name="value"/>.
		/// </summary>
		public void MatchType(Action<CfgValue> value, Action<CfgValueList> list, Action<CfgSection> section)
		{
			value(this);
		}
	}
}
