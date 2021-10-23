namespace SectionConfig.IO
{
	/// <summary>
	/// Result of loading config.
	/// </summary>
	public readonly struct LoadResult
	{
		/// <summary>
		/// Returns a successful result, with <see cref="LoadError.Ok"/> as <see cref="Error"/> and <see cref="string.Empty"/> as <see cref="ErrMsg"/>.
		/// </summary>
		/// <param name="root">The loaded root.</param>
		public LoadResult(CfgRoot root)
		{
			Root = root;
			Error = LoadError.Ok;
			ErrMsg = string.Empty;
		}
		/// <summary>
		/// Returns a failed result. <see cref="Root"/> will be null.
		/// </summary>
		/// <param name="error">The error code.</param>
		/// <param name="errMsg">The error message.</param>
		public LoadResult(LoadError error, string errMsg)
		{
			Root = null;
			Error = error;
			ErrMsg = errMsg;
		}
		/// <summary>
		/// The root. Null on failure, non-null on success.
		/// </summary>
		public CfgRoot? Root { get; }
		/// <summary>
		/// The error code.
		/// </summary>
		public LoadError Error { get; }
		/// <summary>
		/// The error message. Never null; on success it is <see cref="string.Empty"/>.
		/// </summary>
		public string ErrMsg { get; }
	}
}
