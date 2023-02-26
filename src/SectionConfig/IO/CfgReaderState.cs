namespace SectionConfig.IO
{
	using System;
	using System.Collections.Generic;
	/// <summary>
	/// Holds the state for a <see cref="CfgBufferReader"/>.
	/// You get this struct from <see cref="CfgBufferReader"/> and pass it to that struct's constructor to resume reading a buffer of data.
	/// </summary>
	public readonly struct CfgReaderState
	{
		internal readonly CfgKey key;
		internal readonly int position;
		internal readonly ReadOnlyMemory<char> keyIndentation;
		internal readonly ReadOnlyMemory<char> mlIndentation;
		internal readonly Stack<CfgKey> sectionKeys;
		internal readonly ReadStreamState state;
		internal CfgReaderState(CfgKey key, int position, ReadOnlyMemory<char> keyIndentation, ReadOnlyMemory<char> mlIndentation, Stack<CfgKey> sectionKeys, ReadStreamState state)
		{
			this.key = key;
			this.position = position;
			this.keyIndentation = keyIndentation;
			this.mlIndentation = mlIndentation;
			this.sectionKeys = sectionKeys;
			this.state = state;
		}
	}
}
