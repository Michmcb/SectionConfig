namespace SectionConfig
{
	/// <summary>
	/// Default behaviours for merging duplicate items.
	/// </summary>
	public enum MergeBehaviour
	{
		/// <summary>
		/// Fails the merge.
		/// </summary>
		Fail,
		/// <summary>
		/// Keeps the existing object and discards the incoming object.
		/// </summary>
		TakeFirst,
		/// <summary>
		/// Keeps the incoming object and discards the existing object.
		/// </summary>
		TakeLast,
		/// <summary>
		/// Keeps both the incoming object and the existing object.
		/// </summary>
		TakeBoth,
	}
}
