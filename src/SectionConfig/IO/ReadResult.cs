namespace SectionConfig.IO
{
	/// <summary>
	/// The result of reading a token.
	/// </summary>
	public readonly struct ReadResult
	{
		/// <summary>
		/// Creates a new instance, which has no key or content.
		/// </summary>
		/// <param name="token">Type of token read.</param>
		public ReadResult(SectionCfgToken token)
		{
			Token = token;
			Key = default;
			Content = string.Empty;
		}
		/// <summary>
		/// Creates a new instance, which has a key but no content.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="token">Type of token read.</param>
		public ReadResult(SectionCfgToken token, CfgKey key)
		{
			Token = token;
			Key = key;
			Content = string.Empty;
		}
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="token">Type of token read.</param>
		/// <param name="content">The text of the token.</param>
		public ReadResult(SectionCfgToken token, CfgKey key, string content)
		{
			Token = token;
			Key = key;
			Content = content;
		}
		/// <summary>
		/// The type of token that was read.
		/// </summary>
		public SectionCfgToken Token { get; }
		/// <summary>
		/// The key of the token, if any.
		/// This property should be ignored when <see cref="Token"/> is <see cref="SectionCfgToken.Comment"/>, <see cref="SectionCfgToken.End"/>, or <see cref="SectionCfgToken.Error"/>.
		/// When <see cref="Token"/> is <see cref="SectionCfgToken.StartList"/> or <see cref="SectionCfgToken.EndList"/>, this is the key of the list.
		/// When <see cref="Token"/> is <see cref="SectionCfgToken.StartSection"/> or <see cref="SectionCfgToken.EndSection"/>, this is the key of the section.
		/// </summary>
		public CfgKey Key { get; }
		/// <summary>
		/// The content read. This will never be null, but it may be <see cref="string.Empty"/>.
		/// </summary>
		public string Content { get; }
	}
}
