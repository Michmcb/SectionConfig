namespace SectionConfig.IO
{
	using System;

	/// <summary>
	/// The result of reading a token.
	/// </summary>
	public readonly struct ReadResult
	{
		private readonly CfgKey key;
		private readonly string content;
		/// <summary>
		/// Creates a new instance, which has no key or content.
		/// </summary>
		/// <param name="token">Type of token read.</param>
		public ReadResult(SectionCfgToken token)
		{
			Token = token;
			key = default;
			content = string.Empty;
		}
		/// <summary>
		/// Creates a new instance, which has just read a <see cref="CfgKey"/>.
		/// </summary>
		/// <param name="key">The key.</param>
		public ReadResult(CfgKey key)
		{
			Token = SectionCfgToken.Key;
			this.key = key;
			content = string.Empty;
		}
		/// <summary>
		/// Creates a new instance, which has just read some <paramref name="content"/>.
		/// </summary>
		/// <param name="token">Type of token read.</param>
		/// <param name="content">The text of the token.</param>
		public ReadResult(SectionCfgToken token, string content)
		{
			Token = token;
			key = default;
			this.content = content;
		}
		/// <summary>
		/// The type of token that was read.
		/// </summary>
		public SectionCfgToken Token { get; }
		/// <summary>
		/// If <see cref="Token"/> is <see cref="SectionCfgToken.Key"/>, then returns the key.
		/// If <see cref="Token"/> is not <see cref="SectionCfgToken.Key"/>, throws <see cref="InvalidOperationException"/>.
		/// </summary>
		/// <returns>A <see cref="CfgKey"/>.</returns>
		public CfgKey GetKey()
		{
			return Token == SectionCfgToken.Key
				? key
				: throw new InvalidOperationException("The token type is not Key, so you need to call GetContent");
		}
		/// <summary>
		/// If <see cref="Token"/> is not <see cref="SectionCfgToken.Key"/>, then returns the content.
		/// If <see cref="Token"/> is <see cref="SectionCfgToken.Key"/>, throws <see cref="InvalidOperationException"/>.
		/// </summary>
		/// <returns>The content read.</returns>
		public string GetContent()
		{
			return Token != SectionCfgToken.Key
				? content
				: throw new InvalidOperationException("The token type is Key, so you need to call GetKey");
		}
	}
}
