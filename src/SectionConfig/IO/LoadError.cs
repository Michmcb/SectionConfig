namespace SectionConfig.IO
{
	/// <summary>
	/// Error code for loading from a stream.
	/// </summary>
	public enum LoadError
	{
		/// <summary>
		/// Loaded ok.
		/// </summary>
		Ok,
		/// <summary>
		/// A duplicate key was found in the stream.
		/// </summary>
		DuplicateKey,
		/// <summary>
		/// The stream was not syntactically correct.
		/// </summary>
		MalformedStream,
	}
}
