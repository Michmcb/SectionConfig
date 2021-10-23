namespace SectionConfig.IO
{
	/// <summary>
	/// The state of a <see cref="SectionCfgReader"/> or <see cref="SectionCfgWriter"/>.
	/// </summary>
	public enum StreamState
	{
		/// <summary>
		/// Ready to read or write a Key or close section
		/// </summary>
		Start,
		/// <summary>
		/// Just read or write a key
		/// </summary>
		AfterKey,
		/// <summary>
		/// Just read a section open
		/// </summary>
		SectionOpen,
		/// <summary>
		/// Reading or writing a list
		/// </summary>
		List,
		/// <summary>
		/// Just read a section close
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
