namespace SectionConfig.IO
{
	/// <summary>
	/// The state of a <see cref="CfgStreamReader"/>.
	/// </summary>
	public enum ReadStreamState
	{
		/// <summary>
		/// The default state. Ready to read a Key/Value, Key/Section, Key/List, or Comment.
		/// </summary>
		Section,
		/// <summary>
		/// Reading a list.
		/// </summary>
		List,
		/// <summary>
		/// Encountered an error and cannot read anything further.
		/// </summary>
		Error,
		/// <summary>
		/// End of stream.
		/// </summary>
		End,
	}
}
