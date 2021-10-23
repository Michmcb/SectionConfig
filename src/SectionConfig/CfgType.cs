namespace SectionConfig
{
	/// <summary>
	/// The concrete type of a <see cref="ICfgObject"/>.
	/// </summary>
	public enum CfgType
	{
		/// <summary>
		/// A <see cref="CfgValue"/>.
		/// </summary>
		Value,
		/// <summary>
		/// A <see cref="CfgValueList"/>.
		/// </summary>
		ValueList,
		/// <summary>
		/// A <see cref="CfgSection"/>.
		/// </summary>
		Section
	}
}
