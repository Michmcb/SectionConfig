namespace SectionConfig
{
	/// <summary>
	/// Error code from merging multiple <see cref="CfgRoot"/> into one.
	/// </summary>
	public enum MergeError
	{
		/// <summary>
		/// Merged ok.
		/// </summary>
		Ok,
		/// <summary>
		/// A duplicate value was found and was not merged.
		/// </summary>
		DuplicateValue,
		/// <summary>
		/// A duplicate value list was found and was not merged.
		/// </summary>
		DuplicateValueList,
		/// <summary>
		/// A duplicate section was found and was not merged.
		/// </summary>
		DuplicateSection,
		/// <summary>
		/// A duplicate key was found, and the <see cref="CfgType"/> was different.
		/// </summary>
		MismatchingTypes,
		/// <summary>
		/// Any other error
		/// </summary>
		OtherError,
	}
}
