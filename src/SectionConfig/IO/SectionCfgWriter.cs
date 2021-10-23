﻿namespace SectionConfig.IO
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	/// <summary>
	/// Writes section config data to a stream, forward-only.
	/// </summary>
	public sealed class SectionCfgWriter : IDisposable
	{
		private static readonly char[] crlf = new char[] { '\n', '\r' };
		private static readonly char[] cr = new char[] { '\r' };
		private static readonly char[] lf = new char[] { '\n' };
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
		public SectionCfgWriter(string path, bool append, Encoding encoding, ReadOnlyMemory<char> indentation = default, NewLine newLine = IO.NewLine.Platform, Quoting quoting = Quoting.DoubleIfNeeded, Multiline multiline = Multiline.Auto)
			: this(new StreamWriter(path, append, encoding), indentation, newLine, quoting, multiline, true) { }
		/// <summary>
		/// Writes to <paramref name="writer"/>.
		/// </summary>
		/// <param name="writer">The stream to write to.</param>
		/// <param name="indentation">The indentation to use per level of indentation. Must be all whitespace.</param>
		/// <param name="newLine">The newline style to use</param>
		/// <param name="quoting">Quoting style to use for values.</param>
		/// <param name="multiline">Multiline style to use for values.</param>
		/// <param name="closeOutput">If true, will dispose of <paramref name="writer"/> when this instance is disposed of. If false, disposing this instance will do nothing.</param>
		public SectionCfgWriter(TextWriter writer, ReadOnlyMemory<char> indentation = default, NewLine newLine = IO.NewLine.Platform, Quoting quoting = Quoting.DoubleIfNeeded, Multiline multiline = Multiline.Auto, bool closeOutput = true)
		{
			// Dfeault indentation to tabs, otherwise use that they gave us (but it has to be all whitespace)
			Indentation = indentation.Length == 0
				? (new char[1] { '\t' })
				: indentation.Span.IsWhiteSpace()
					? indentation
					: throw new ArgumentException("Indentation must be entirely whitespace", nameof(indentation));
			Writer = writer;
			NewLine = newLine switch
			{
				IO.NewLine.Lf => lf,
				IO.NewLine.CrLf => crlf,
				IO.NewLine.Cr => cr,
				_ => Environment.NewLine.AsMemory(),
			};
			Quoting = quoting;
			Multiline = multiline;
			CloseOutput = closeOutput;
			State = StreamState.Start;
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
		/// If true, <see cref="Writer"/> will be closed when this is disposed. Otherwise, it will not.
		/// </summary>
		public bool CloseOutput { get; set; }
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
		public StreamState State { get; private set; }
		/// <summary>
		/// Equivalent to calling <see cref="WriteKey(CfgKey)"/> followed by <see cref="WriteValue(in ReadOnlySpan{char})"/>.
		/// </summary>
		/// <param name="key">The key to write.</param>
		/// <param name="val">The value to write.</param>
		public void WriteKeyValue(CfgKey key, in ReadOnlySpan<char> val)
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
			if (State != StreamState.Start)
			{
				throw new InvalidOperationException(string.Concat("Can't write key \"", key.KeyString, "\", because the current state is ", State.ToString(), " "));
			}
			WriteIndentation(IndentationLevel);
			Writer.Write(key.KeyString);
			State = StreamState.AfterKey;
		}
		/// <summary>
		/// Writes a value. Throws <see cref="InvalidOperationException"/> if not in the correct state.
		/// There are no invalid values for <paramref name="val"/>.
		/// </summary>
		/// <param name="val">The value to write.</param>
		public void WriteValue(in ReadOnlySpan<char> val)
		{
			if (State != StreamState.AfterKey)
			{
				throw new InvalidOperationException(string.Concat("Can't write value \"", new string(val), "\", because the current state is ", State.ToString(), " "));
			}
			Writer.Write(CfgSyntax.KeyEnd);
			// If they asked to always quote, then we do that
			if (Quoting >= Quoting.AlwaysDouble)
			{
				Writer.Write(' ');
				WriteRawQuoted(val, Quoting);
			}
			// Otherwise, we only need to quote if the content dictates we must.
			else if (NeedsQuoting(val))
			{
				Writer.Write(' ');
				WriteRawQuoted(val, Quoting);
			}
			// Now we know the string does not need to be quoted due to bad characters. We may need to write it multiline, though.
			else
			{
				switch (Multiline)
				{
					default:
					case Multiline.Auto:
						// If we have a newline in the string, behave the same as Always
						if (val.IndexOfAny(crlf) != -1)
						{
							goto case Multiline.AlwaysIfPossible;
						}
						// If not, then we won't.
						else
						{
							Writer.Write(' ');
							Writer.Write(val);
						}
						break;
					case Multiline.Never:
						// If we're never writing it multiline, then we'll have to write it quoted if the string has any newlines in it
						Writer.Write(' ');
						if (val.IndexOfAny(crlf) == -1)
						{
							Writer.Write(val);
						}
						else
						{
							WriteRawQuoted(val, Quoting);
						}
						break;
					case Multiline.AlwaysIfPossible:
						// Easy, always write multiline
						Writer.Write(NewLine);
						WriteIndentation(IndentationLevel + 1);
						WriteRawMultiline(val, IndentationLevel + 1);
						break;
				}
			}
			State = StreamState.Start;
			Writer.Write(NewLine);
		}
		/// <summary>
		/// Opens a value list.
		/// </summary>
		/// <returns>A token used to write values and close the value list.</returns>
		public WriteValueListToken WriteOpenValueList()
		{
			if (State != StreamState.AfterKey)
			{
				throw new InvalidOperationException(string.Concat("Can't write open value list because the current state is ", State.ToString(), " "));
			}
			Writer.Write(": {");
			Writer.Write(NewLine);
			++IndentationLevel;
			State = StreamState.List;
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
			if (State != StreamState.AfterKey)
			{
				throw new InvalidOperationException(string.Concat("Can't write open section because the current state is ", State.ToString(), " "));
			}
			Writer.Write(" {");
			Writer.Write(NewLine);
			++IndentationLevel;
			State = StreamState.Start;
			int id = nextId++;
			tokenIds.Push(id);
			return new(this, id);
		}
		internal void WriteListValue(in ReadOnlySpan<char> val)
		{
			if (State != StreamState.List)
			{
				throw new InvalidOperationException(string.Concat("Can't write list value \"", new string(val), "\", because the current state is ", State.ToString(), " "));
			}
			bool mustQuote = NeedsQuoting(val);
			WriteIndentation(IndentationLevel);
			if (mustQuote)
			{
				// Write the string quoted
				WriteRawQuoted(val, Quoting);
			}
			else
			{
				if (val.IndexOfAny(crlf) != -1)
				{
					// TODO allow writing multi-line strings within a list
					WriteRawQuoted(val, Quoting);
				}
				else
				{
					Writer.Write(val);
				}
			}
			Writer.Write(NewLine);
		}
		internal void WriteCloseValueList(int id)
		{
			if (State != StreamState.List)
			{
				throw new InvalidOperationException(string.Concat("Can't write close value list because the current state is ", State.ToString(), " "));
			}
			if (!tokenIds.TryPop(out int popped) || popped != id)
			{
				throw new InvalidOperationException(string.Concat("Already closed this value list"));
			}
			WriteIndentation(--IndentationLevel);
			Writer.Write(CfgSyntax.EndSectionOrList);
			Writer.Write(NewLine);
			State = StreamState.Start;
		}
		internal void WriteCloseSection(int id)
		{
			if (State != StreamState.Start)
			{
				throw new InvalidOperationException(string.Concat("Can't write close section because the current state is ", State.ToString(), " "));
			}
			if (!tokenIds.TryPop(out int popped) || popped != id)
			{
				throw new InvalidOperationException(string.Concat("Already closed this section"));
			}
			WriteIndentation(--IndentationLevel);
			Writer.Write(CfgSyntax.EndSectionOrList);
			Writer.Write(NewLine);
			--SectionLevel;
			State = StreamState.Start;
		}
		private void WriteRawQuoted(in ReadOnlySpan<char> val, Quoting quoting)
		{
			char q = quoting switch
			{
				Quoting.SingleIfNeeded or Quoting.AlwaysSingle => '\'',
				_ => '\"',
			};
			Writer.Write(q);
			foreach (char c in val)
			{
				Writer.Write(c);
				if (c == q) { Writer.Write(c); }
			}
			Writer.Write(q);
		}
		private void WriteRawMultiline(in ReadOnlySpan<char> val, int indentationLevel)
		{
			for (int i = 0; i < val.Length; i++)
			{
				char c = val[i];
				Writer.Write(c);
				switch (c)
				{
					case '\r':
						// But if it isn't we write the indentation first, then the next char.
						// Might be \r\n, so check the next char
						if (++i < val.Length)
						{
							c = val[i];
							// If it's \n then we write that, and then write the indentation
							if (c == '\n')
							{
								Writer.Write(c);
								WriteIndentation(indentationLevel);
							}
							else
							{
								WriteIndentation(indentationLevel);
								Writer.Write(c);
							}
						}
						break;
					case '\n':
						// Newline, so write the indentation
						WriteIndentation(indentationLevel);
						break;
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
		private static bool NeedsQuoting(ReadOnlySpan<char> val)
		{
			// If the string is length 0, or starts with { # " ' or has leading/trailing whitespce, must be quoted
			// Or if they explicitly asked for it to be quoted
			if (val.Length == 0)
			{
				return true;
			}
			else
			{
				char c = val[0];
				switch (c)
				{
					case CfgSyntax.StartSectionOrList:
					case CfgSyntax.EndSectionOrList:
					case CfgSyntax.Comment:
					case CfgSyntax.DoubleQuote:
					case CfgSyntax.SingleQuote:
						return true;
					default:
						if (char.IsWhiteSpace(c))
						{
							return true;
						}
						else if (char.IsWhiteSpace(val[^1]))
						{
							return true;
						}
						break;
				}
			}
			return false;
		}
		/// <summary>
		/// If <see cref="CloseOutput"/> is true, then disposes <see cref="Writer"/>.
		/// Otherwise, does nothing.
		/// </summary>
		public void Dispose()
		{
			if (CloseOutput)
			{
				Writer.Dispose();
			}
		}
	}
}