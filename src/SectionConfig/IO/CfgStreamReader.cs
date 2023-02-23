namespace SectionConfig.IO
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Text;

	/// <summary>
	/// Reads section config data from a stream, forward-only.
	/// </summary>
	public sealed class CfgStreamReader : IDisposable
	{
		private char lastChar;
		private string currentLineIdentation;
		private CfgKey listKey;
		private readonly Stack<CfgKey> sectionKeys;
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="reader">The stream to read. Does not have to be seekable.</param>
		/// <param name="leaveOpen"><see langword="true"/> to leave <paramref name="reader"/> open after this <see cref="CfgStreamReader"/> is disposed. Otherwise, <see langword="false"/>.</param>
		public CfgStreamReader(TextReader reader, bool leaveOpen = false)
		{
			Reader = reader;
			LeaveOpen = leaveOpen;
			sectionKeys = new();
			currentLineIdentation = string.Empty;
		}
		/// <summary>
		/// The underlying <see cref="TextReader"/> being used.
		/// </summary>
		public TextReader Reader { get; }
		/// <summary>
		/// <see langword="true"/> to leave <see cref="Reader"/> open after this <see cref="CfgStreamReader"/> is disposed. Otherwise, false.
		/// </summary>
		public bool LeaveOpen { get; set; }
		/// <summary>
		/// The current state of the reader
		/// </summary>
		public ReadStreamState State { get; private set; }
		/// <summary>
		/// The current nesting level of sections. Initially it is 0.
		/// </summary>
		public int SectionLevel => sectionKeys.Count;
		/// <summary>
		/// Reads the next token from <see cref="Reader"/>.
		/// </summary>
		/// <returns>The result of reading the next token.</returns>
		public ReadResult Read()
		{
			string? content;
			char? c;
			switch (State)
			{
				case ReadStreamState.Section:
					// Expecting a key or start/end section or comment or end of stream
					c = GetLastChar() ?? GetNextSignificantChar().Next;
					// End of the stream
					if (!c.HasValue)
					{
						return SectionLevel == 0
							? EndOfStream()
							: Error(default, string.Concat("Found end of stream when there were still ", SectionLevel.ToString(), " sections to close"));
					}
					switch (c.Value)
					{
						case CfgSyntax.Comment:
							// Don't care if it's the end of the stream
							return ReadComment();
						case CfgSyntax.EndSectionOrList:
							// We don't update the state, because after closing the section we're back in Section state again, ready for another end section/list or key or comment.
							return sectionKeys.TryPop(out CfgKey skey)
								? new(SectionCfgToken.EndSection, skey, string.Empty)
								: Error(default, "Found section close when there was no section to close");
						default:
							// Any other char is the start of a key
							StringBuilder sb = new();
							sb.Append(c.Value);
							// Read the rest of the key, so the stream is up to the colon or the brace.
							(CfgKey? key, bool foundBrace) = TryReadKey(sb, out content);

							// First if we found no key, that's an error
							if (key.HasValue == false)
							{
								return Error(default, content);
							}

							// If we found a brace, then we know that we just opened a section, so pass that back.
							// No change in state needs to happen.
							if (foundBrace)
							{
								sectionKeys.Push(key.Value);
								return new ReadResult(SectionCfgToken.StartSection, key.Value, string.Empty);
							}
							Span<char> indentationBeforeKey = stackalloc char[currentLineIdentation.Length];
							currentLineIdentation.AsSpan().CopyTo(indentationBeforeKey);
							// So now we know it was Key:. So, get the next char. If it is an open brace, then it's opening a list. Otherwise, it's just a Key: Value

							NextSignificantChar nsc = GetNextSignificantChar();
							c = nsc.Next;
							// End of stream, it's just a Key with an empty value
							if (!c.HasValue)
							{
								return new ReadResult(SectionCfgToken.Value, key.Value, string.Empty);
							}

							switch (c.Value)
							{
								case CfgSyntax.StartSectionOrList:
									// Start of a list
									State = ReadStreamState.List;
									listKey = key.Value;
									return new(SectionCfgToken.StartList, key.Value, string.Empty);
								case CfgSyntax.SingleQuote:
								case CfgSyntax.DoubleQuote:
									// Quoted string
									return TryReadQuotedString(c.Value, out content)
										? new(SectionCfgToken.Value, key.Value, content)
										: Error(key.Value, content);
								default:
									// If there was a newline, and indentation has not changed, then it's like this...
									// Key1:
									// Key2:
									// And since it's very convenient to be able to have an empty value be denoted by just a blank line
									// So, if the indentation before the key we just read starts with this new line's indentation, we will treat it as 
									if (nsc.SawNewline && StartsWith(value: indentationBeforeKey, startsWith: currentLineIdentation.AsSpan()))
									{
										lastChar = c.Value;
										return new ReadResult(SectionCfgToken.Value, key.Value, string.Empty);
									}
									// Unquoted string. Our char was the start of the value so stick that onto our StringBuilder and read the rest of it.
									// If we saw a newline before we got to read the first char of the value, then that means it was like...
									// Key:
									//		Value
									// Which means that it's unquoted and multiline.
									sb.Clear();
									sb.Append(c.Value);
									return currentLineIdentation.Length > 0 && nsc.SawNewline
										? new ReadResult(SectionCfgToken.Value, key.Value, ReadUnquotedMultilineString(sb, currentLineIdentation.AsSpan()))
										: new ReadResult(SectionCfgToken.Value, key.Value, ReadTrimmedLine(c.Value));
							}
					}
				case ReadStreamState.List:
					c = GetLastChar() ?? GetNextSignificantChar().Next;
					if (!c.HasValue)
					{
						return Error(listKey, "Encountered end of stream when trying to read List Values");
					}
					switch (c.Value)
					{
						case CfgSyntax.EndSectionOrList:
							State = ReadStreamState.Section;
							return new(SectionCfgToken.EndList, listKey, string.Empty);
						case CfgSyntax.Comment:
							// Don't care if it's the end of the stream
							return ReadComment();
						case CfgSyntax.SingleQuote:
						case CfgSyntax.DoubleQuote:
							// Quoted string, read it
							return TryReadQuotedString(c.Value, out content)
								? new ReadResult(SectionCfgToken.ListValue, listKey, content)
								: Error(listKey, content);
						default:
							// Any other char is the start of a value
							return new(SectionCfgToken.ListValue, listKey, ReadTrimmedLine(c.Value));
					}
				case ReadStreamState.End:
					// We never allow setting the state to the end of stream unless SectionLevel is 0, so don't need to check that here.
					return new(SectionCfgToken.End, default, string.Empty);
				default:
				case ReadStreamState.Error:
					return new(SectionCfgToken.Error, default, "Encountered error, cannot read further");
			}
		}
		private static bool StartsWith(ReadOnlySpan<char> value, ReadOnlySpan<char> startsWith)
		{
			if (startsWith.Length <= value.Length)
			{
				for (int i = 0; i < startsWith.Length; i++)
				{
					if (startsWith[i] != value[i]) return false;
				}
				return true;
			}
			return false;
		}
		private ReadResult EndOfStream()
		{
			State = ReadStreamState.End;
			return new(SectionCfgToken.End);
		}
		private ReadResult Error(CfgKey key, string s)
		{
			State = ReadStreamState.Error;
			return new(SectionCfgToken.Error, key, s);
		}
		private ReadResult ReadComment()
		{
			// ReadLine() chomps the newline, so if we didn't reach the end of the stream then the lastChar we read is maybe a \n
			// It might not be, but for what we use it for, this is close enough
			// Doing this causes a comment right after a Key: #Comment will cause multilines strings to be interpreted correctly
			// Because NextNonWhitespaceChar will check lastChar, and if it's a newline, notes we've seen a newline 
			string? s = Reader.ReadLine();
			if (s != null) lastChar = '\n';
			return new(SectionCfgToken.Comment, default, s ?? string.Empty);
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
		private (CfgKey? key, bool foundBrace) TryReadKey(StringBuilder sb, out string errMsg)
		{
			bool go = true;
			bool foundBrace = false;
			while (go)
			{
				int r = Reader.Read();
				if (r != -1)
				{
					char c = (char)r;
					switch (c)
					{
						case CfgSyntax.StartSectionOrList:
							foundBrace = true;
							go = false;
							break;
						case CfgSyntax.KeyEnd:
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
					return (null, false);
				}
			}
			string str = TrimEndStringBuilder(sb);
			CfgKey? key = CfgKey.TryCreate(str);
			errMsg = key.HasValue ? string.Empty : CfgKey.InvalidCharDescription + str;
			return (key, foundBrace);
		}
		/// <summary>
		/// Reads a quoted string. It will stop reading once it finds a non-doubled instance of <paramref name="q"/>.
		/// Sets <see cref="lastChar"/> to the 1 excess character it read unless end of stream is found.
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
		/// If <see cref="lastChar"/> has a value, returns that value and clears it.
		/// Otherwise, returns null.
		/// </summary>
		private char? GetLastChar()
		{
			if (lastChar != '\0')
			{
				char c = lastChar;
				lastChar = '\0';
				if (!char.IsWhiteSpace(c)) return c;
			}
			return null;
		}
		/// <summary>
		/// Skips all whitespace and returns the first non-whitespace character found.
		/// </summary>
		private NextSignificantChar GetNextSignificantChar()
		{
			StringBuilder sb = new();
			bool b = false;
			while (true)
			{
				int r = Reader.Read();
				if (r != -1)
				{
					char c = (char)r;
					if (!char.IsWhiteSpace(c))
					{
						currentLineIdentation = sb.ToString();
						return new(c, b);
					}
					if (c != '\n' && c != '\r')
					{
						sb.Append(c);
					}
					else
					{
						b = true;
						sb.Clear();
					}
				}
				else
				{
					currentLineIdentation = sb.ToString();
					return new(null, b);
				}
			}
		}
#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls
		/// <summary>
		/// If <see cref="LeaveOpen"/> is <see langword="false"/>, disposes of <see cref="Reader"/>. Otherwise, leaves <see cref="Reader"/> open.
		/// </summary>
		public void Dispose()
		{
			if (!disposedValue)
			{
				if (LeaveOpen == false)
				{
					Reader.Dispose();
				}

				disposedValue = true;
			}
		}
#endregion
	}
}
