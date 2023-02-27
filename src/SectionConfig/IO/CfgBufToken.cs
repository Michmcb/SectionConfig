namespace SectionConfig.IO
{
	/// <summary>
	/// The type of token read from a buffer.
	/// </summary>
	public enum CfgBufToken
	{
		/// <summary>
		/// A key.
		/// </summary>
		Key,
		/// <summary>
		/// A single value.
		/// </summary>
		Value,
		/// <summary>
		/// A comment. Can appear anywhere between elements.
		/// </summary>
		Comment,
		/// <summary>
		/// The start of a multiline value; expect <see cref="Value"/> or <see cref="EndMultiline"/> to follow.
		/// </summary>
		StartMultiline,
		/// <summary>
		/// End of a multiline value
		/// </summary>
		EndMultiline,
		/// <summary>
		/// A key and the start of an list; expect <see cref="Value"/> or <see cref="EndList"/> to follow.
		/// </summary>
		StartList,
		/// <summary>
		/// The end of an list.
		/// </summary>
		EndList,
		/// <summary>
		/// A key and the start of a section.
		/// </summary>
		StartSection,
		/// <summary>
		/// The end of a section.
		/// </summary>
		EndSection,
		/// <summary>
		/// More data is required in the provided buffer to be able to accurately parse it.
		/// </summary>
		NeedMoreData,
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
