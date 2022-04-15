namespace SectionConfig.IO
{
	/// <summary>
	/// The state of a <see cref="SectionCfgReader"/>
	/// </summary>
	public enum WriteStreamState
	{
		/// <summary>
		/// Ready to read or write a Key or close section
		/// </summary>
		Start,
		/// <summary>
		/// Just read or wrote a key
		/// </summary>
		AfterKey,
		/// <summary>
		/// Just read or wrote a section open
		/// </summary>
		SectionOpen,
		/// <summary>
		/// Reading or writing a list
		/// </summary>
		List,
		/// <summary>
		/// Just read or wrote a section close
		/// </summary>
		SectionClose,
		/// <summary>
		/// Encountered an error and cannot do anything further
		/// </summary>
		Error,
		/// <summary>
		/// End of stream
		/// </summary>
		End,
	}
}
