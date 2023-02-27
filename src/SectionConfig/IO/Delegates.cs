namespace SectionConfig.IO
{
	/// <summary>
	/// A callback that can handle the result of <see cref="CfgBufferReader.Read"/>.
	/// Returning <see langword="false"/> will stop iteration.
	/// </summary>
	/// <param name="state">State information.</param>
	/// <param name="token">The token that was just read.</param>
	/// <param name="reader">The <see cref="CfgBufferReader"/>.</param>
	public delegate bool HandleBufferToken<T>(T state, CfgBufToken token, CfgBufferReader reader);
}
