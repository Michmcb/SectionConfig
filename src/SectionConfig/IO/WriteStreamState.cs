namespace SectionConfig.IO
{
	/// <summary>
	/// The state of a <see cref="CfgStreamWriter"/>
	/// </summary>
	public enum WriteStreamState
	{
		/// <summary>
		/// Ready to write a Key or close section
		/// </summary>
		Start,
		/// <summary>
		/// Just wrote a key
		/// </summary>
		AfterKey,
		/// <summary>
		/// Just wrote a section open
		/// </summary>
		SectionOpen,
		/// <summary>
		/// Writing a list
		/// </summary>
		List,
		/// <summary>
		/// Just wrote a section close
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
