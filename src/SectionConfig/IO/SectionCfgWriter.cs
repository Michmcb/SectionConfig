namespace SectionConfig.IO
{
	using System;
	using System.IO;
	using System.Text;
	/// <summary>
	/// Writes section config data to a stream, forward-only.
	/// </summary>
	[Obsolete("Prefer " + nameof(IO.CfgStreamWriter) + ", which uses leaveOpen instead of closeInput like .NET streams do.")]
	public sealed class SectionCfgWriter : IDisposable
	{
		/// <summary>
		/// Equivalent to passing a new <see cref="StreamWriter"/>. It will be closed on disposal.
		/// </summary>
		/// <param name="path">File path to write to.</param>
		/// <param name="append">true to append data, false to overwrite. No effect if file doesn't exist.</param>
		/// <param name="encoding">Character encoding to use.</param>
		/// <param name="indentation">The indentation to use per level of indentation. Must be all whitespace.</param>
		/// <param name="newLine">The newline style to use</param>
		/// <param name="quoting">Quoting style to use for values.</param>
		/// <param name="multiline">Multiline style to use for values.</param>
		public SectionCfgWriter(string path, bool append, Encoding encoding, ReadOnlyMemory<char> indentation = default, NewLine newLine = IO.NewLine.Platform, Quoting quoting = Quoting.DoubleIfNeeded, Multiline multiline = Multiline.Auto)
			: this(new StreamWriter(path, append, encoding), indentation, newLine, quoting, multiline, true) { }
		/// <summary>
		/// Writes to <paramref name="writer"/>.
		/// </summary>
		/// <param name="writer">The stream to write to.</param>
		/// <param name="indentation">The indentation to use per level of indentation. Must be all whitespace. If default/empty, uses a single tab instead.</param>
		/// <param name="newLine">The newline style to use</param>
		/// <param name="quoting">Quoting style to use for values.</param>
		/// <param name="multiline">Multiline style to use for values.</param>
		/// <param name="closeOutput">If true, will dispose of <paramref name="writer"/> when this instance is disposed of. If false, disposing this instance will do nothing.</param>
		public SectionCfgWriter(TextWriter writer, ReadOnlyMemory<char> indentation = default, NewLine newLine = IO.NewLine.Platform, Quoting quoting = Quoting.DoubleIfNeeded, Multiline multiline = Multiline.Auto, bool closeOutput = true)
		{
			CfgStreamWriter = new(writer, indentation, newLine, quoting, multiline, !closeOutput);
		}
		/// <summary>
		/// Creates a new instance which wraps <paramref name="cfgStreamWriter"/>.
		/// </summary>
		/// <param name="cfgStreamWriter">The <see cref="IO.CfgStreamWriter"/> to wrap.</param>
		public SectionCfgWriter(CfgStreamWriter cfgStreamWriter)
		{
			CfgStreamWriter = cfgStreamWriter;
		}
		/// <summary>
		/// The <see cref="IO.CfgStreamWriter"/> that this instance wraps.
		/// </summary>
		public CfgStreamWriter CfgStreamWriter { get; }
		/// <summary>
		/// The underlying <see cref="TextWriter"/> being used.
		/// </summary>
		public TextWriter Writer => CfgStreamWriter.Writer;
		/// <summary>
		/// One level of indentation, applied based on section nesting depth.
		/// </summary>
		public ReadOnlyMemory<char> Indentation => CfgStreamWriter.Indentation;
		/// <summary>
		/// The newline string to use.
		/// </summary>
		public ReadOnlyMemory<char> NewLine => CfgStreamWriter.NewLine;
		/// <summary>
		/// Quoting preference.
		/// </summary>
		public Quoting Quoting { get => CfgStreamWriter.Quoting; set => CfgStreamWriter.Quoting = value; }
		/// <summary>
		/// Multiline preference.
		/// </summary>
		public Multiline Multiline { get => CfgStreamWriter.Multiline; set => CfgStreamWriter.Multiline = value; }
		/// <summary>
		/// If true, <see cref="Writer"/> will be closed when this is disposed. Otherwise, it will not.
		/// </summary>
		public bool CloseOutput { get => !CfgStreamWriter.LeaveOpen; set => CfgStreamWriter.LeaveOpen = !value; }
		/// <summary>
		/// The current level of indentation to apply. This is increased by 1 for every section opened, and decreased by 1 for every section closed.
		/// When writing a value list, indentation applied 1 level more.
		/// </summary>
		public int IndentationLevel { get => CfgStreamWriter.IndentationLevel; set => CfgStreamWriter.IndentationLevel = value; }
		/// <summary>
		/// The current nesting level of sections.
		/// </summary>
		public int SectionLevel => CfgStreamWriter.SectionLevel;
		/// <summary>
		/// The current state.
		/// </summary>
		public WriteStreamState State => CfgStreamWriter.State;
		/// <summary>
		/// Writes a comment. If any linebreaks are present, the comment will be written on multiple lines.
		/// </summary>
		/// <param name="comment">The comment to write.</param>
		/// <param name="replaceLineBreaks">If true, any line-breaks (\r, \n, or \r\n) in the comment are replaced with <see cref="NewLine"/></param>
		public void WriteComment(ReadOnlySpan<char> comment, bool replaceLineBreaks = true)
		{
			CfgStreamWriter.WriteComment(comment, replaceLineBreaks);
		}
		/// <summary>
		/// Equivalent to calling <see cref="WriteKey(CfgKey)"/> followed by <see cref="WriteValue(ReadOnlySpan{char})"/>.
		/// </summary>
		/// <param name="key">The key to write.</param>
		/// <param name="val">The value to write.</param>
		public void WriteKeyValue(CfgKey key, ReadOnlySpan<char> val)
		{
			CfgStreamWriter.WriteKeyValue(key, val);
		}
		/// <summary>
		/// Equivalent to calling <see cref="WriteKey(CfgKey)"/> followed by <see cref="WriteOpenSection"/>.
		/// </summary>
		/// <param name="key">The key to write.</param>
		/// <returns>A token used to close the section.</returns>
		public WriteSectionToken WriteKeyOpenSection(CfgKey key)
		{
			return CfgStreamWriter.WriteKeyOpenSection(key);
		}
		/// <summary>
		/// Equivalent to calling <see cref="WriteKey(CfgKey)"/> followed by <see cref="WriteOpenValueList"/>.
		/// </summary>
		/// <param name="key">The key to write.</param>
		/// <returns>A token used to write values and close the value list.</returns>
		public WriteValueListToken WriteKeyOpenValueList(CfgKey key)
		{
			return CfgStreamWriter.WriteKeyOpenValueList(key);
		}
		/// <summary>
		/// Writes a key. Throws <see cref="InvalidOperationException"/> if not in the correct state, or if <paramref name="key"/> is not valid.
		/// </summary>
		/// <param name="key">The key to write.</param>
		public void WriteKey(CfgKey key)
		{
			CfgStreamWriter.WriteKey(key);
		}
		/// <summary>
		/// Writes a value. Throws <see cref="InvalidOperationException"/> if not in the correct state.
		/// There are no invalid values for <paramref name="val"/>.
		/// </summary>
		/// <param name="val">The value to write.</param>
		public void WriteValue(ReadOnlySpan<char> val)
		{
			CfgStreamWriter.WriteValue(val);
		}
		/// <summary>
		/// Opens a value list.
		/// </summary>
		/// <returns>A token used to write values and close the value list.</returns>
		public WriteValueListToken WriteOpenValueList()
		{
			return CfgStreamWriter.WriteOpenValueList();
		}
		/// <summary>
		/// Opens a section.
		/// </summary>
		/// <returns>A token used to close the section.</returns>
		public WriteSectionToken WriteOpenSection()
		{
			return CfgStreamWriter.WriteOpenSection();
		}
		/// <summary>
		/// If <see cref="CloseOutput"/> is true, then disposes <see cref="Writer"/>.
		/// Otherwise, does nothing.
		/// </summary>
		public void Dispose()
		{
			CfgStreamWriter.Dispose();
		}
	}
}

