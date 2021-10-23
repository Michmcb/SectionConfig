namespace SectionConfig.IO
{
	/// <summary>
	/// The type of token read from a stream.
	/// </summary>
	public enum SectionCfgToken
	{
		/// <summary>
		/// A key that comes before any of: <see cref="StartSection"/>, <see cref="StartList"/>, <see cref="Value"/>.
		/// </summary>
		Key,
		/// <summary>
		/// A single value that comes after <see cref="Key"/>.
		/// </summary>
		Value,
		/// <summary>
		/// A comment. Can appear anywhere between elements, but not inside <see cref="Value"/>.
		/// </summary>
		Comment,
		/// <summary>
		/// The start of an list; expect <see cref="ListValue"/> or <see cref="EndList"/> to follow.
		/// </summary>
		StartList,
		/// <summary>
		/// A value inside of a list.
		/// </summary>
		ListValue,
		/// <summary>
		/// The end of an list.
		/// </summary>
		EndList,
		/// <summary>
		/// The start of a section.
		/// </summary>
		StartSection,
		/// <summary>
		/// The end of a section.
		/// </summary>
		EndSection,
		/// <summary>
		/// The end of a stream.
		/// </summary>
		End,
		/// <summary>
		/// An error was encountered.
		/// </summary>
		Error,
	}
}
