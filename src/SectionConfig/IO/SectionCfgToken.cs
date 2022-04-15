namespace SectionConfig.IO
{
	/// <summary>
	/// The type of token read from a stream.
	/// </summary>
	public enum SectionCfgToken
	{
		/// <summary>
		/// A key and a single value.
		/// </summary>
		Value,
		/// <summary>
		/// A comment. Can appear anywhere between elements.
		/// </summary>
		Comment,
		/// <summary>
		/// A key and the start of an list; expect <see cref="ListValue"/> or <see cref="EndList"/> to follow.
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
		/// A key and the  start of a section.
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
