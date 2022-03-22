namespace SectionConfig
{
	/// <summary>
	/// Error code for adding a <see cref="ICfgObject"/> to a <see cref="CfgSection"/>.
	/// </summary>
	public enum AddError
	{
		/// <summary>
		/// Added ok.
		/// </summary>
		Ok,
		/// <summary>
		/// The key is already used by another <see cref="ICfgObject"/>.
		/// </summary>
		KeyAlreadyExists,
		/// <summary>
		/// The <see cref="ICfgObject"/> added has already been added to a <see cref="CfgSection"/>.
		/// </summary>
		AlreadyHasParent,
	}
}
