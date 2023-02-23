namespace SectionConfig.IO
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;

	/// <summary>
	/// Writes section config data to a stream, forward-only.
	/// </summary>
	public sealed class CfgStreamWriter : IDisposable
	{
		private static readonly char[] crlf = new char[] { '\r', '\n' };
		private static readonly char[] lf = new char[] { '\n' };
		private static readonly char[] tab = new char[] { '\t' };
		private readonly Stack<int> tokenIds;
		private int nextId;
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
		public CfgStreamWriter(string path, bool append, Encoding encoding, ReadOnlyMemory<char> indentation = default, NewLine newLine = IO.NewLine.Platform, Quoting quoting = Quoting.DoubleIfNeeded, Multiline multiline = Multiline.Auto)
			: this(new StreamWriter(path, append, encoding), indentation, newLine, quoting, multiline, false) { }
		/// <summary>
		/// Writes to <paramref name="writer"/>.
		/// </summary>
		/// <param name="writer">The stream to write to.</param>
		/// <param name="indentation">The indentation to use per level of indentation. Must be all whitespace. If default/empty, uses a single tab instead.</param>
		/// <param name="newLine">The newline style to use</param>
		/// <param name="quoting">Quoting style to use for values.</param>
		/// <param name="multiline">Multiline style to use for values.</param>
		/// <param name="leaveOpen"><see langword="true"/> to leave <paramref name="writer"/> open after this <see cref="CfgStreamWriter"/> is disposed. Otherwise, <see langword="false"/>.</param>
		public CfgStreamWriter(TextWriter writer, ReadOnlyMemory<char> indentation = default, NewLine newLine = IO.NewLine.Platform, Quoting quoting = Quoting.DoubleIfNeeded, Multiline multiline = Multiline.Auto, bool leaveOpen = true)
		{
			// Default indentation to tabs, otherwise use that they gave us (but it has to be all whitespace)
			Indentation = indentation.Length == 0
				? tab
				: indentation.Span.IsWhiteSpace()
					? indentation
					: throw new ArgumentException("Indentation must be entirely whitespace", nameof(indentation));
			Writer = writer;
			NewLine = newLine switch
			{
				IO.NewLine.Lf => lf,
				IO.NewLine.CrLf => crlf,
				_ => Environment.NewLine.AsMemory(),
			};
			Quoting = quoting;
			Multiline = multiline;
			LeaveOpen = leaveOpen;
			State = WriteStreamState.Start;
			tokenIds = new();
			nextId = 0;
			IndentationLevel = 0;
		}
		/// <summary>
		/// The underlying <see cref="TextWriter"/> being used.
		/// </summary>
		public TextWriter Writer { get; }
		/// <summary>
		/// One level of indentation, applied based on section nesting depth.
		/// </summary>
		public ReadOnlyMemory<char> Indentation { get; }
		/// <summary>
		/// The newline string to use.
		/// </summary>
		public ReadOnlyMemory<char> NewLine { get; }
		/// <summary>
		/// Quoting preference.
		/// </summary>
		public Quoting Quoting { get; set; }
		/// <summary>
		/// Multiline preference.
		/// </summary>
		public Multiline Multiline { get; set; }
		/// <summary>
		/// <see langword="true"/> to leave <see cref="Writer"/> open after this <see cref="CfgStreamWriter"/> is disposed. Otherwise, false.
		/// </summary>
		public bool LeaveOpen { get; set; }
		/// <summary>
		/// The current level of indentation to apply. This is increased by 1 for every section opened, and decreased by 1 for every section closed.
		/// When writing a value list, indentation applied 1 level more.
		/// </summary>
		public int IndentationLevel { get; set; }
		/// <summary>
		/// The current nesting level of sections.
		/// </summary>
		public int SectionLevel { get; private set; }
		/// <summary>
		/// The current state.
		/// </summary>
		public WriteStreamState State { get; private set; }
		/// <summary>
		/// Writes a comment. If any linebreaks are present, the comment will be written on multiple lines.
		/// Can only be done in states: <see cref="WriteStreamState.Start"/>, <see cref="WriteStreamState.SectionOpen"/>, <see cref="WriteStreamState.SectionClose"/>, <see cref="WriteStreamState.List"/>
		/// </summary>
		/// <param name="comment">The comment to write.</param>
		/// <param name="replaceLineBreaks">If true, any line-breaks (\r, \n, or \r\n) in the comment are replaced with <see cref="NewLine"/></param>
		public void WriteComment(ReadOnlySpan<char> comment, bool replaceLineBreaks = true)
		{
			switch (State)
			{
				case WriteStreamState.Start:
				case WriteStreamState.SectionOpen:
				case WriteStreamState.SectionClose:
				case WriteStreamState.List:
					Util.SpanSplit(comment, '\n', (str, offset, length) =>
					{
						ReadOnlySpan<char> line = str.Slice(offset, length);
						// If the string contained an \r\n, then our line will end with \r, so don't write that.
						bool wasCrLf = line[line.Length - 1] == '\r';
						WriteIndentation(IndentationLevel);
						Writer.Write('#');
						if (wasCrLf)
						{
							Writer.Write(line.Slice(0, line.Length - 1));
						}
						else
						{
							Writer.Write(line);
						}

						if (replaceLineBreaks)
						{
							Writer.Write(NewLine.Span);
						}
						else if (wasCrLf)
						{
							Writer.Write(crlf);
						}
						else
						{
							Writer.Write('\n');
						}
					});
					break;
				case WriteStreamState.AfterKey:
				case WriteStreamState.Error:
				case WriteStreamState.End:
					throw new InvalidOperationException(string.Concat("Can't write comment \"",
#if NETSTANDARD2_0
						comment.ToString(),
#else
						comment,
#endif
						"\", because the current state is ", State.ToString()));
			}
		}
		/// <summary>
		/// Writes <see cref="Indentation"/>, followed by <see cref="NewLine"/>.
		/// Can only be done in states: <see cref="WriteStreamState.Start"/>, <see cref="WriteStreamState.SectionOpen"/>, <see cref="WriteStreamState.SectionClose"/>, <see cref="WriteStreamState.List"/>
		/// </summary>
		public void WriteNewLine()
		{
			switch (State)
			{
				case WriteStreamState.Start:
				case WriteStreamState.SectionOpen:
				case WriteStreamState.SectionClose:
				case WriteStreamState.List:
					WriteIndentation(IndentationLevel);
					Writer.Write(NewLine);
					break;
				case WriteStreamState.AfterKey:
				case WriteStreamState.Error:
				case WriteStreamState.End:
					throw new InvalidOperationException(string.Concat("Can't write a new line, because the current state is ", State.ToString()));
			}
		}
		/// <summary>
		/// Equivalent to calling <see cref="WriteKey(CfgKey)"/> followed by <see cref="WriteValue(ReadOnlySpan{char})"/>.
		/// </summary>
		/// <param name="key">The key to write.</param>
		/// <param name="val">The value to write.</param>
		public void WriteKeyValue(CfgKey key, ReadOnlySpan<char> val)
		{
			WriteKey(key);
			WriteValue(val);
		}
		/// <summary>
		/// Equivalent to calling <see cref="WriteKey(CfgKey)"/> followed by <see cref="WriteOpenSection"/>.
		/// </summary>
		/// <param name="key">The key to write.</param>
		/// <returns>A token used to close the section.</returns>
		public WriteSectionToken WriteKeyOpenSection(CfgKey key)
		{
			WriteKey(key);
			return WriteOpenSection();
		}
		/// <summary>
		/// Equivalent to calling <see cref="WriteKey(CfgKey)"/> followed by <see cref="WriteOpenValueList"/>.
		/// </summary>
		/// <param name="key">The key to write.</param>
		/// <returns>A token used to write values and close the value list.</returns>
		public WriteValueListToken WriteKeyOpenValueList(CfgKey key)
		{
			WriteKey(key);
			return WriteOpenValueList();
		}
		/// <summary>
		/// Writes a key. Throws <see cref="InvalidOperationException"/> if not in the correct state, or if <paramref name="key"/> is not valid.
		/// </summary>
		/// <param name="key">The key to write.</param>
		public void WriteKey(CfgKey key)
		{
			if (State != WriteStreamState.Start)
			{
				throw new InvalidOperationException(string.Concat("Can't write key \"", key.KeyString, "\", because the current state is ", State.ToString(), " "));
			}
			if (key.KeyString == null)
			{
				throw new ArgumentException("Don't pass in default(CfgKey), you need to use CfgKey.Create() or CfgKey.TryCreate() to get a valid key.", nameof(key));
			}
			WriteIndentation(IndentationLevel);
			Writer.Write(key.KeyString);
			State = WriteStreamState.AfterKey;
		}
		/// <summary>
		/// Writes a value. Throws <see cref="InvalidOperationException"/> if not in the correct state.
		/// There are no invalid values for <paramref name="val"/>.
		/// </summary>
		/// <param name="val">The value to write.</param>
		public void WriteValue(ReadOnlySpan<char> val)
		{
			if (State != WriteStreamState.AfterKey)
			{
				throw new InvalidOperationException(string.Concat("Can't write value \"",
#if NETSTANDARD2_0
					val.ToString(),
#else
					new string(val),
#endif
					"\", because the current state is ", State.ToString(), " "));
			}
			Writer.Write(CfgSyntax.KeyEnd);

			switch (GetWriteStrategy(val))
			{
				default:
				case WriteStrategy.SingleLine:
					Writer.Write(' ');
					Writer.Write(val);
					break;
				case WriteStrategy.MultiLine:
					Writer.Write(NewLine.Span);
					WriteIndentation(IndentationLevel + 1);
					WriteRawMultiline(val, IndentationLevel + 1);
					break;
				case WriteStrategy.SingleQuotes:
					Writer.Write(' ');
					WriteRawQuoted(val, '\'');
					break;
				case WriteStrategy.DoubleQuotes:
					Writer.Write(' ');
					WriteRawQuoted(val, '"');
					break;
			}

			State = WriteStreamState.Start;
			Writer.Write(NewLine.Span);
		}
		/// <summary>
		/// Opens a value list.
		/// </summary>
		/// <returns>A token used to write values and close the value list.</returns>
		public WriteValueListToken WriteOpenValueList()
		{
			if (State != WriteStreamState.AfterKey)
			{
				throw new InvalidOperationException(string.Concat("Can't write open value list because the current state is ", State.ToString(), " "));
			}
			Writer.Write(": {");
			Writer.Write(NewLine.Span);
			++IndentationLevel;
			State = WriteStreamState.List;
			int id = nextId++;
			tokenIds.Push(id);
			return new(this, id);
		}
		/// <summary>
		/// Opens a section.
		/// </summary>
		/// <returns>A token used to close the section.</returns>
		public WriteSectionToken WriteOpenSection()
		{
			if (State != WriteStreamState.AfterKey)
			{
				throw new InvalidOperationException(string.Concat("Can't write open section because the current state is ", State.ToString()));
			}
			Writer.Write(" {");
			Writer.Write(NewLine.Span);
			++IndentationLevel;
			State = WriteStreamState.Start;
			int id = nextId++;
			tokenIds.Push(id);
			return new(this, id);
		}
		internal void WriteListValue(ReadOnlySpan<char> val)
		{
			if (State != WriteStreamState.List)
			{
				throw new InvalidOperationException(string.Concat("Can't write list value \"",
#if NETSTANDARD2_0
					val.ToString(),
#else
					new string(val),
#endif
					"\", because the current state is ", State.ToString()));
			}
			WriteIndentation(IndentationLevel);
			switch (GetWriteStrategy(val))
			{
				default:
				case WriteStrategy.SingleLine:
					Writer.Write(val);
					break;
				case WriteStrategy.MultiLine:
					// List values can't be multiline yet so write them quoted
					WriteRawQuoted(val, Quoting);
					break;
				case WriteStrategy.SingleQuotes:
					WriteRawQuoted(val, '\'');
					break;
				case WriteStrategy.DoubleQuotes:
					WriteRawQuoted(val, '"');
					break;
			}
			Writer.Write(NewLine.Span);
		}
		internal void WriteCloseValueList(int id)
		{
			if (State != WriteStreamState.List)
			{
				throw new InvalidOperationException(string.Concat("Can't write close value list because the current state is ", State.ToString(), " "));
			}
			if (!tokenIds.TryPop(out int popped) || popped != id)
			{
				throw new InvalidOperationException(string.Concat("Already closed this value list"));
			}
			WriteIndentation(--IndentationLevel);
			Writer.Write(CfgSyntax.EndSectionOrList);
			Writer.Write(NewLine.Span);
			State = WriteStreamState.Start;
		}
		internal void WriteCloseSection(int id)
		{
			if (State != WriteStreamState.Start)
			{
				throw new InvalidOperationException(string.Concat("Can't write close section because the current state is ", State.ToString(), " "));
			}
			if (!tokenIds.TryPop(out int popped) || popped != id)
			{
				throw new InvalidOperationException(string.Concat("Already closed this section"));
			}
			WriteIndentation(--IndentationLevel);
			Writer.Write(CfgSyntax.EndSectionOrList);
			Writer.Write(NewLine.Span);
			--SectionLevel;
			State = WriteStreamState.Start;
		}
		private void WriteRawQuoted(ReadOnlySpan<char> val, Quoting quoting)
		{
			WriteRawQuoted(val, quoting switch
			{
				Quoting.SingleIfNeeded or Quoting.AlwaysSingle => '\'',
				_ => '\"',
			});
		}
		private void WriteRawQuoted(ReadOnlySpan<char> val, char q)
		{
			Writer.Write(q);
			foreach (char c in val)
			{
				Writer.Write(c);
				if (c == q) { Writer.Write(c); }
			}
			Writer.Write(q);
		}
		private void WriteRawMultiline(ReadOnlySpan<char> val, int indentationLevel)
		{
			for (int i = 0; i < val.Length; i++)
			{
				char c = val[i];
				Writer.Write(c);
				// Now check to see if we wrote a newline char. If we did, then we need to write some indentation.
				// This works for both \n and \r\n, because when writing the \r first nothing special happens, then when we write the \n, the indentation is added.
				if (c == '\n')
				{
					WriteIndentation(indentationLevel);
				}
			}
		}
		private void WriteIndentation(int num)
		{
			for (int i = 0; i < num; i++)
			{
				Writer.Write(Indentation);
			}
		}
		/// <summary>
		/// Returns the <see cref="WriteStrategy"/> that will be used for <paramref name="val"/>.
		/// The strategy used depends on the <see cref="Quoting"/> and <see cref="Multiline"/> properties,
		/// as well as any syntax characters, leading/trailing whitespace, and Line Feeds or Carriage Returns within <paramref name="val"/>.
		/// </summary>
		/// <param name="val">The value to get a write strategy for.</param>
		/// <returns></returns>
		internal WriteStrategy GetWriteStrategy(ReadOnlySpan<char> val)
		{
			// First, if quoting is set to always, just return that
			switch (Quoting)
			{
				case Quoting.AlwaysDouble: return WriteStrategy.DoubleQuotes;
				case Quoting.AlwaysSingle: return WriteStrategy.SingleQuotes;
			}
			// Next, if the string is length 0, it needs to be either quoted or multiline.
			// We only prefer writing multiline if there's a strong preference for multiline values.
			if (val.Length == 0)
			{
				return Multiline == Multiline.AlwaysIfPossible
					? WriteStrategy.MultiLine
					: MustBeQuoted(Quoting);
			}
			else
			{
				char c = val[0];
				switch (c)
				{
					// If the string starts with a special syntax character then it must be quoted
					case CfgSyntax.StartSectionOrList:
					case CfgSyntax.EndSectionOrList:
					case CfgSyntax.Comment:
					case CfgSyntax.DoubleQuote:
					case CfgSyntax.SingleQuote:
						return MustBeQuoted(Quoting);
					default:
						// We may be able to write it multiline, here.
						// First of all if we have leading or trailing whitespace which isn't lf/crlf, we have to quote to preserve that whitespace.
						char cEnd = val[val.Length - 1];

						// Don't bother checking crlf for the end, since if it ends with crlf, then it ends with lf
						if (char.IsWhiteSpace(c) && c != '\n' && !val.StartsWith(crlf)
							|| (char.IsWhiteSpace(cEnd) && cEnd != '\n'))
						{
							return MustBeQuoted(Quoting);
						}
						else
						{
							return Multiline switch
							{
								// Always want multiline, so do it multiline
								Multiline.AlwaysIfPossible => WriteStrategy.MultiLine,

								// They don't want multiline, so if we have any crlf then quote it to avoid multiline
								Multiline.Never => val.IndexOfAny(crlf) != -1
									? MustBeQuoted(Quoting)
									: WriteStrategy.SingleLine,

								// Automatic; that means multiline if needed, single line if not needed
								_ => val.IndexOfAny(crlf) != -1
									? WriteStrategy.MultiLine
									: WriteStrategy.SingleLine,
							};
						}
				}
			}

			static WriteStrategy MustBeQuoted(Quoting q)
			{
				return q switch
				{
					Quoting.AlwaysSingle => WriteStrategy.SingleQuotes,
					Quoting.SingleIfNeeded => WriteStrategy.SingleQuotes,
					_ => WriteStrategy.DoubleQuotes,
				};
			}
		}
		/// <summary>
		/// If <see cref="LeaveOpen"/> is true, then disposes <see cref="Writer"/>.
		/// Otherwise, does nothing.
		/// </summary>
		public void Dispose()
		{
			State = WriteStreamState.End;
			if (LeaveOpen == false)
			{
				Writer.Dispose();
			}
		}
	}
}

