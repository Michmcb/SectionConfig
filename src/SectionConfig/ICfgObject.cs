namespace SectionConfig
{
	using System;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// One of <see cref="CfgSection"/>, <see cref="CfgValue"/> or <see cref="CfgValueList"/>.
	/// </summary>
	public interface ICfgObject
	{
		/// <summary>
		/// The key.
		/// </summary>
		CfgKey Key { get; }
		// TODO The path doesn't work right. If you build a section with children and then add things to it, then you add that section to another section, path will be wrong. Rather we should only calculate Path when asked, and if a section gets added to something, invalidate all child paths.
		///// <summary>
		///// The path. If this is orphaned or the root section, it is <see cref="string.Empty"/>. Once it's added to a <see cref="CfgSection"/>, it is non-empty.
		///// </summary>
		//string Path { get; }
		/// <summary>
		/// The type.
		/// </summary>
		CfgType Type { get; }
		/// <summary>
		/// If <see cref="ICfgObject"/> is a <see cref="CfgSection"/>, returns itself.
		/// Otherwise throws <see cref="InvalidCastException"/>.
		/// </summary>
		CfgSection ToSection();
		/// <summary>
		/// If <see cref="ICfgObject"/> is a <see cref="CfgValue"/>, returns itself.
		/// Otherwise throws <see cref="InvalidCastException"/>.
		/// </summary>
		CfgValue ToValue();
		/// <summary>
		/// If <see cref="ICfgObject"/> is a <see cref="CfgValueList"/>, returns itself.
		/// Otherwise throws <see cref="InvalidCastException"/>.
		/// </summary>
		CfgValueList ToValueList();
		/// <summary>
		/// If <see cref="ICfgObject"/> is a <see cref="CfgSection"/>, returns itself.
		/// Otherwise returns null.
		/// </summary>
		CfgSection? AsSection();
		/// <summary>
		/// If <see cref="ICfgObject"/> is a <see cref="CfgValue"/>, returns itself.
		/// Otherwise returns null.
		/// </summary>
		CfgValue? AsValue();
		/// <summary>
		/// If <see cref="ICfgObject"/> is a <see cref="CfgValueList"/>, returns itself.
		/// Otherwise returns null.
		/// </summary>
		CfgValueList? AsValueList();
		/// <summary>
		/// If <see cref="ICfgObject"/> is a <see cref="CfgSection"/>, returns true and sets <paramref name="section"/>.
		/// Otherwise returns false.
		/// </summary>
		bool IsSection([NotNullWhen(true)] out CfgSection? section);
		/// <summary>
		/// If <see cref="ICfgObject"/> is a <see cref="CfgValue"/>, returns true and sets <paramref name="value"/>.
		/// Otherwise returns false.
		/// </summary>
		bool IsValue([NotNullWhen(true)] out CfgValue? value);
		/// <summary>
		/// If <see cref="ICfgObject"/> is a <see cref="CfgValueList"/>, returns true and sets <paramref name="list"/>.
		/// Otherwise returns false.
		/// </summary>
		bool IsValueList([NotNullWhen(true)] out CfgValueList? list);
	}
}
