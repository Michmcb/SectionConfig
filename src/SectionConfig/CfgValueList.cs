namespace SectionConfig
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// A key with a list of values.
	/// </summary>
	public sealed class CfgValueList : ICfgObject
	{
		internal bool hasParent;
		/// <summary>
		/// Creates a new instance with the provided key and values.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="values">The values.</param>
		public CfgValueList(CfgKey key, IList<string> values)
		{
			Key = key;
			Values = values;
		}
		/// <summary>
		/// The key.
		/// </summary>
		public CfgKey Key { get; }
		/// <summary>
		/// The list of string values.
		/// </summary>
		public IList<string> Values { get; }
		/// <summary>
		/// Returns	<see cref="CfgType.ValueList"/>.
		/// </summary>
		public CfgType Type => CfgType.ValueList;
		/// <summary>
		/// Returns null.
		/// </summary>
		public CfgSection? AsSection()
		{
			return null;
		}
		/// <summary>
		/// Returns null.
		/// </summary>
		public CfgValue? AsValue()
		{
			return null;
		}
		/// <summary>
		/// Returns this.
		/// </summary>
		public CfgValueList? AsValueList()
		{
			return this;
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
		/// Returns false.
		/// </summary>
		/// <param name="value">Will be null.</param>
		public bool IsValue([NotNullWhen(true)] out CfgValue? value)
		{
			value = null;
			return false;
		}
		/// <summary>
		/// Returns true.
		/// </summary>
		/// <param name="list">Will be this.</param>
		public bool IsValueList([NotNullWhen(true)] out CfgValueList? list)
		{
			list = this;
			return true;
		}
		/// <summary>
		/// Throws an <see cref="InvalidCastException"/>.
		/// </summary>
		public CfgSection ToSection()
		{
			throw new InvalidCastException("Requested " + nameof(CfgSection) + " but this is a " + nameof(CfgValueList));
		}
		/// <summary>
		/// Throws an <see cref="InvalidCastException"/>.
		/// </summary>
		public CfgValue ToValue()
		{
			throw new InvalidCastException("Requested " + nameof(CfgValue) + " but this is a " + nameof(CfgValueList));
		}
		/// <summary>
		/// Returns this.
		/// </summary>
		public CfgValueList ToValueList()
		{
			return this;
		}
	}
}
