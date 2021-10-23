namespace SectionConfig.IO
{
	/// <summary>
	/// Defines how to handle quoting strings.
	/// </summary>
	public enum Quoting
	{
		/// <summary>
		/// If not needed, don't apply any quotes. If needed, apply "double quotes".
		/// </summary>
		DoubleIfNeeded,
		/// <summary>
		/// If not needed, don't apply any quotes. If needed, apply 'single quotes'.
		/// </summary>
		SingleIfNeeded,
		/// <summary>
		/// Always apply "double quotes". Causes strings to never be written multiline.
		/// </summary>
		AlwaysDouble,
		/// <summary>
		/// Always apply 'single quotes'. Causes strings to never be written multiline.
		/// </summary>
		AlwaysSingle,
	}
}
