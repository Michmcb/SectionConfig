namespace SectionConfig.Test
{
	using SectionConfig;
	using SectionConfig.IO;
	using System;

	public readonly struct BufReadResult
	{
		public BufReadResult(CfgBufToken token) : this(token, default) { }
		public BufReadResult(CfgBufToken token, CfgKey key)
		{
			Token = token;
			Key = key;
		}
		public BufReadResult(CfgBufToken token, CfgKey key, string content) : this(token, key)
		{
			Content = content.AsMemory();
		}
		public BufReadResult(CfgBufToken token, CfgKey key, string content, string leadingNewLine) : this(token, key)
		{
			Content = content.AsMemory();
			LeadingNewLine = leadingNewLine.AsMemory();
		}
		public CfgBufToken Token { get; }
		public CfgKey Key { get; }
		public ReadOnlyMemory<char> Content { get; }
		public ReadOnlyMemory<char> LeadingNewLine { get; }
	}
}
