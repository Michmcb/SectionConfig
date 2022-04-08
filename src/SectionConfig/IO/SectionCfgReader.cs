namespace SectionConfig.IO
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Text;
	/// <summary>
	/// Reads section config data from a stream, forward-only.
	/// </summary>
	public sealed class SectionCfgReader : IDisposable
	{
		private char lastChar;
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="reader">The stream to read. Does not have to be seekable.</param>
		/// <param name="closeInput">If true, closes <paramref name="reader"/> on disposal.</param>
		public SectionCfgReader(TextReader reader, bool closeInput = true)
		{
			Reader = reader;
			CloseInput = closeInput;
		}
		/// <summary>
		/// The underlying <see cref="TextReader"/> being used.
		/// </summary>
		public TextReader Reader { get; }
		/// <summary>
		/// If true, <see cref="Reader"/> will be closed when this is disposed. Otherwise, it will not.
		/// </summary>
		public bool CloseInput { get; set; }
		/// <summary>
		/// The current state of the reader
		/// </summary>
		public StreamState State { get; private set; }
		/// <summary>
		/// The current nesting level of sections. Initially it is 0.
		/// </summary>
		public int SectionLevel { get; private set; }
		/// <summary>
		/// Reads the next token from <see cref="Reader"/>.
		/// </summary>
		/// <returns>The result of reading the next token.</returns>
		public ReadResult Read()
		{
			string? content;
			StringBuilder whitespace;
			char? c;
			// We never set Prev when we read a comment
			switch (State)
			{
				case StreamState.Start:
					// Expecting a key or start/end section or comment or end of stream
					c = NextNonWhitespaceChar();
					// End of the stream
					if (!c.HasValue)
					{
						return SectionLevel == 0
							? EndOfStream()
							: Error(string.Concat("Found end of stream when there were still ", SectionLevel.ToString(), " sections to close"));
					}
					switch (c.Value)
					{
						case CfgSyntax.Comment:
							// Don't care if it's the end of the stream
							return ReadComment();
						case CfgSyntax.EndSectionOrList:
							// We don't update the state, because after closing the section we're back in Start state again, ready for another end section/list or key or comment.
							return --SectionLevel >= 0
								? new(SectionCfgToken.EndSection)
								: Error("Found section close when there was no section to close");
						default:
							// Any other char is the start of a key
							StringBuilder sb = new();
							sb.Append(c.Value);
							CfgKey? key = TryReadKey(sb, out content);
							return key.HasValue
								? new(key.Value)
								: Error(content);
					}
				case StreamState.AfterKey:
					NextSignificantChar nsc1 = NextNonWhitespaceChar(whitespace = new());
					c = nsc1.Next;
					if (!c.HasValue)
					{
						return Value(string.Empty);
					}
					switch (c.Value)
					{
						case CfgSyntax.StartSectionOrList:
							// Start of a list
							State = StreamState.List;
							return new(SectionCfgToken.StartList);
						case CfgSyntax.SingleQuote:
						case CfgSyntax.DoubleQuote:
							// Quoted string, read it
							return TryReadQuotedString(c.Value, out content)
								? Value(content)
								: Error(content);
						default:
							// Any other char is the start of a value
							StringBuilder sb = new();
							sb.Append(c.Value);
							return whitespace.Length > 0 && nsc1.SawNewline
								? Value(ReadUnquotedMultilineString(sb, whitespace.ToString().AsSpan()))
								: Value(ReadTrimmedLine(c.Value));
					}
				case StreamState.SectionOpen:
					State = StreamState.Start;
					++SectionLevel;
					return new(SectionCfgToken.StartSection);
				// We should actually never wind up at this point since we never set the state to SectionClose.
				//case StreamState.SectionClose:
				//	State = StreamState.Start;
				//	--SectionLevel;
				//	return new(SectionCfgToken.EndSection);
				case StreamState.List:
					c = NextNonWhitespaceChar();
					if (!c.HasValue)
					{
						return Error("Encountered end of stream when trying to read List Values");
					}
					switch (c.Value)
					{
						case CfgSyntax.EndSectionOrList:
							State = StreamState.Start;
							return new(SectionCfgToken.EndList);
						case CfgSyntax.Comment:
							// Don't care if it's the end of the stream
							return ReadComment();
						case CfgSyntax.SingleQuote:
						case CfgSyntax.DoubleQuote:
							// Quoted string, read it
							return TryReadQuotedString(c.Value, out content)
								? (new(SectionCfgToken.ListValue, content))
								: Error(content);
						default:
							// Any other char is the start of a value
							return new(SectionCfgToken.ListValue, ReadTrimmedLine(c.Value));
					}
				case StreamState.End:
					// We never allow setting the state to the end of stream unless SectionLevel is 0.
					return new(SectionCfgToken.End, string.Empty);
				default:
				case StreamState.Error:
					return new(SectionCfgToken.Error, "Encountered error, cannot read further");
			}
		}
		private ReadResult EndOfStream()
		{
			State = StreamState.End;
			return new(SectionCfgToken.End);
		}
		private ReadResult Error(string s)
		{
			State = StreamState.Error;
			return new(SectionCfgToken.Error, s);
		}
		private ReadResult Value(string s)
		{
			State = StreamState.Start;
			return new(SectionCfgToken.Value, s);
		}
		private ReadResult ReadComment()
		{
			// ReadLine() chomps the newline, so if we didn't reach the end of the stream then the lastChar we read is maybe a \n
			// It might not be, but for what we use it for, this is close enough
			// Doing this causes a comment right after a Key: #Comment will cause multilines strings to be interpreted correctly
			// Because NextNonWhitespaceChar will check lastChar, and if it's a newline, notes we've seen a newline 
			string? s = Reader.ReadLine();
			if (s != null) lastChar = '\n';
			return new(SectionCfgToken.Comment, s ?? string.Empty);
		}
		/// <summary>
		/// Trims whitespace off the end of <paramref name="sb"/> and returns it.
		/// </summary>
		private static string TrimEndStringBuilder(StringBuilder sb)
		{
			int i = sb.Length - 1;
			for (; i > 0 && char.IsWhiteSpace(sb[i]); --i) ;
			return sb.ToString(0, i + 1);
		}
		/// <summary>
		/// Reads a line, trims whitespace from the end, and prepends <paramref name="firstChar"/>. Or if end of stream, just returns <paramref name="firstChar"/>.
		/// </summary>
		/// <param name="firstChar">Char to prepend.</param>
		/// <returns>The read line, trailing whitespace trimmed.</returns>
		private string ReadTrimmedLine(char firstChar)
		{
			string? s = Reader.ReadLine();
			return s == null ? new string(firstChar, 1) : firstChar + s.TrimEnd();
		}
		/// <summary>
		/// Reads a key. Does not allow newline characters or end of stream.
		/// : or { is considered the end of the key.
		/// </summary>
		/// <param name="sb">Used to build the key.</param>
		/// <param name="errMsg">The error message if something goes wrong.</param>
		/// <returns>true on success, false on failure.</returns>
		private CfgKey? TryReadKey(StringBuilder sb, out string errMsg)
		{
			bool go = true;
			while (go)
			{
				int r = Reader.Read();
				if (r != -1)
				{
					char c = (char)r;
					switch (c)
					{
						case CfgSyntax.StartSectionOrList:
							State = StreamState.SectionOpen;
							go = false;
							break;
						case CfgSyntax.KeyEnd:
							State = StreamState.AfterKey;
							go = false;
							break;
						default:
							// Allow invalid characters for a more descriptive error message later on
							sb.Append(c);
							break;
					}
				}
				else
				{
					errMsg = "Found end of stream when reading Key " + sb.ToString();
					return null;
				}
			}
			string str = TrimEndStringBuilder(sb);
			CfgKey? key = CfgKey.TryCreate(str);
			errMsg = key.HasValue ? string.Empty : CfgKey.InvalidCharDescription + str;
			return key;
		}
		/// <summary>
		/// Reads a quoted string. It will stop reading once it finds a non-doubled instance of <paramref name="q"/>.
		/// Sets <see cref="lastChar"/> to the final character it read unless end of stream is found.
		/// </summary>
		/// <param name="q">The closing quote.</param>
		/// <param name="content">The value, or error message.</param>
		/// <returns>true on success, false on failure.</returns>
		private bool TryReadQuotedString(char q, out string content)
		{
			StringBuilder sb = new();
			while (true)
			{
				int r = Reader.Read();
				if (r != -1)
				{
					char c = (char)r;
					if (c == q)
					{
						// We found maybe the closing quote. Check the next char; if it's the quote then we haven't closed the string yet. If it's not, then we closed the string.
						r = Reader.Read();
						if (r != -1)
						{
							c = (char)r;
							if (c != q)
							{
								lastChar = c;
								content = sb.ToString();
								return true;
							}
							sb.Append(c);
						}
						else
						{
							// End of the stream is fine
							content = sb.ToString();
							return true;
						}
					}
					else
					{
						sb.Append(c);
					}
				}
				else
				{
					content = "Found end of stream when reading quoted string " + sb.ToString();
					return false;
				}
			}
		}
		/// <summary>
		/// Reads a string, including interior linebreaks, until encountering a line that does not start with <paramref name="indentation"/>.
		/// Sets <see cref="lastChar"/> to the final character it read unless end of stream is found.
		/// </summary>
		/// <param name="sb">Used to build the string.</param>
		/// <param name="indentation">The indentation at the start of a line that is used to identify the string is still going.</param>
		/// <returns>The string, including any trailing whitespace.</returns>
		private string ReadUnquotedMultilineString(StringBuilder sb, ReadOnlySpan<char> indentation)
		{
			Debug.Assert(indentation.Length != 0, "Multiline strings always have indentation, so if there's none it should be read as a single-line string");
			bool go = true;
			int chomp = 1;
			while (go)
			{
				int r = Reader.Read();
				if (r != -1)
				{
					char c = (char)r;
					sb.Append(c);

					// If we read a \n or \r\n, we check the indentation. If the indentation matches we keep reading, if it does not, then we stop reading.
					switch (c)
					{
						case '\r':
							r = Reader.Read();
							if (r != -1)
							{
								// If it's an \r\n, then we need to do the same thing as if it were an \n. But append the \n before we do.
								if ((c = (char)r) == '\n')
								{
									// We'll have to chomp off 2 trailing characters (\r\n) in this case if we happen to find the end of the multiline value.
									sb.Append('\n');
									chomp = 2;
									goto case '\n';
								}
								else
								{
									// Was just a lone \r, so just append the next char and continue as normal.
									sb.Append(c);
								}
							}
							else
							{
								// End of stream, so stop reading
								go = false;
							}
							break;
						case '\n':
							// We read one char at a time, and as soon as we see a non-indentation-matching char, bail out set it as the last char read
							for (int i = 0; i < indentation.Length; i++)
							{
								r = Reader.Read();
								if (r != -1)
								{
									if (indentation[i] != (c = (char)r))
									{
										// The character read does not match the defined indentation, so this marks the end of the multiline value.
										// We save c as lastChar (so the next read picks it up), remove the trailing \n or \r\n, and then stop looping so the string returns.

										sb.Remove(sb.Length - chomp, chomp);
										lastChar = c;
										go = false;
										break;
									}
								}
								else
								{
									// We've hit the end of the stream before we managed to match all of the indentation.
									// So remove the trailing \n or \r\n, and then stop looping so the string returns.

									sb.Remove(sb.Length - chomp, chomp);
									go = false;
									break;
								}
							}
							chomp = 1;
							break;
					}
				}
				else
				{
					// End of stream
					go = false;
				}
			}
			return sb.ToString();
		}
		/// <summary>
		/// Skips all whitespace and returns the first non-whitespace character found.
		/// </summary>
		/// <returns>The first non-whitespace character, or null if end of stream was found.</returns>
		private char? NextNonWhitespaceChar()
		{
			if (lastChar != '\0')
			{
				char c = lastChar;
				lastChar = '\0';
				if (!char.IsWhiteSpace(c)) return c;
			}
			while (true)
			{
				int r = Reader.Read();
				if (r != -1)
				{
					char c = (char)r;
					if (!char.IsWhiteSpace(c))
					{
						return c;
					}
				}
				else
				{
					return null;
				}
			}
		}
		/// <summary>
		/// Skips all whitespace and returns the first non-whitespace character found.
		/// It also appends all whitespace (only AFTER a newline) to <paramref name="whitespace"/>.
		/// </summary>
		private NextSignificantChar NextNonWhitespaceChar(StringBuilder whitespace)
		{
			bool b = false;
			// It isn't possible for lastChar to be non-null (\0), because we only ever call this method directly after it gets set back to \0.
			// This leaves a gap in our test coverage, unfortunately. It would be nice if we could modify the code a bit so we can eliminate this check with total confidence.
			if (lastChar != '\0')
			{
				char c = lastChar;
				lastChar = '\0';
				if (!char.IsWhiteSpace(c))
				{
					return new(c, false);
				}
				else
				{
					b = true;
				}
			}
			while (true)
			{
				int r = Reader.Read();
				if (r != -1)
				{
					char c = (char)r;
					if (!char.IsWhiteSpace(c))
					{
						return new(c, b);
					}
					if (c != '\n' && c != '\r')
					{
						whitespace.Append(c);
					}
					else
					{
						b = true;
						whitespace.Clear();
					}
				}
				else
				{
					return new(null, b);
				}
			}
		}
		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls
		/// <summary>
		/// If <see cref="CloseInput"/> is true, disposes of <see cref="Reader"/>. Otherwise, does not.
		/// </summary>
		public void Dispose()
		{
			if (!disposedValue)
			{
				if (CloseInput)
				{
					Reader.Dispose();
				}

				disposedValue = true;
			}
		}
		#endregion
	}
}
