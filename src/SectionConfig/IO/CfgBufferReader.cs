#pragma warning disable IDE0057 // Use range operator - .netstandard2.0 doesn't have it :(
namespace SectionConfig.IO
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Reads section config data from a buffer, forward-only.
	/// To use this struct, first you want to create an instance, and don't pass in any state.
	/// Then, keep calling <see cref="Read"/> until it returns <see cref="CfgBufToken.NeedMoreData"/>.
	/// When it does, call <see cref="CopyLeftoverAndResetPosition(Span{char}, out int)"/> to copy leftover data into a new buffer.
	/// Then, fill the remainder of the new buffer with data from whatever source you are using.
	/// And finally, create a new instance of <see cref="CfgBufferReader"/>, passing in the new buffer and the result of <see cref="GetState"/>, and repeat the above.
	/// </summary>
	public ref struct CfgBufferReader
	{
		private ReadOnlyMemory<char> keyIndentation;
		private ReadOnlyMemory<char> mlIndentation;
		private readonly Stack<CfgKey> sectionKeys;
		private ReadStreamState state;
		/// <summary>
		/// Creates a new instance with initial state.
		/// Use this constructor to create the initial buffer reader.
		/// </summary>
		/// <param name="buffer">The buffer to read from.</param>
		/// <param name="isFinalBlock"><see langword="true"/> if this is the final block of data, <see langword="false"/> otherwise.</param>
		public CfgBufferReader(ReadOnlySpan<char> buffer, bool isFinalBlock)
		{
			Buffer = buffer;
			IsFinalBlock = isFinalBlock;
			sectionKeys = new Stack<CfgKey>();
			state = ReadStreamState.Section;
		}
		/// <summary>
		/// Creates a new instance with the provided <paramref name="state"/>.
		/// Use this constructor to create a reader to continue reading a new buffer, once <see cref="Read"/> returns <see cref="CfgBufToken.NeedMoreData"/>.
		/// </summary>
		/// <param name="buffer">The buffer to read from.</param>
		/// <param name="isFinalBlock"><see langword="true"/> if this is the final block of data, <see langword="false"/> otherwise.</param>
		/// <param name="state">The reader state.</param>
		public CfgBufferReader(ReadOnlySpan<char> buffer, bool isFinalBlock, CfgReaderState state)
		{
			Buffer = buffer;
			IsFinalBlock = isFinalBlock;
			Key = state.key;
			Position = state.position;
			keyIndentation = state.keyIndentation;
			mlIndentation = state.mlIndentation;
			sectionKeys = state.sectionKeys ?? new Stack<CfgKey>();
			this.state = state.state;
		}
		/// <summary>
		/// The buffer to read from.
		/// </summary>
		public ReadOnlySpan<char> Buffer { get; }
		/// <summary>
		/// The current position in the <see cref="Buffer"/>; that is, how many bytes have been consumed.
		/// </summary>
		public int Position { get; private set; }
		/// <summary>
		/// How many chars are leftover in <see cref="Buffer"/>.
		/// This value can be used to ensure you have a large enough buffer when calling <see cref="CopyLeftoverAndResetPosition(Span{char}, out int)"/>.
		/// </summary>
		public int Leftover => Buffer.Length - Position;
		/// <summary>
		/// If <see langword="true"/>, then <see cref="Buffer"/> is assumed to be the last block of data.
		/// Otherwise, assumes more data is available.
		/// </summary>
		public bool IsFinalBlock { get; }
		/// <summary>
		/// The current nesting level of sections. Initially it is 0.
		/// </summary>
		public int SectionLevel => sectionKeys.Count;
		/// <summary>
		/// The last read key. This is set whenever <see cref="Read"/> returns any of:
		/// <see cref="CfgBufToken.Key"/>, <see cref="CfgBufToken.StartList"/>, <see cref="CfgBufToken.EndList"/>, <see cref="CfgBufToken.StartSection"/>, <see cref="CfgBufToken.EndSection"/>.
		/// Practically, this means you don't need to keep track of the key yourself. This property tells you either the current value's key, the current section key, or the current list key.
		/// </summary>
		public CfgKey Key { get; private set; }
		/// <summary>
		/// The last read content. This is, for example, the text of a comment or values.
		/// </summary>
		public ReadOnlySpan<char> Content { get; private set; }
		/// <summary>
		/// The leading newline that was found before a multiline value.
		/// When building a string, what you want to do is first append this, then append <see cref="Content"/>.
		/// That will give you a string that is the full multiline value, without the trailing newline.
		/// </summary>
		public ReadOnlySpan<char> LeadingNewLine { get; private set; }
		/// <summary>
		/// Returns the current <see cref="CfgReaderState"/>.
		/// This state has to be passed to a new instance of <see cref="CfgBufferReader"/> to continue reading.
		/// </summary>
		public CfgReaderState GetState()
		{
			return new CfgReaderState(Key, Position, keyIndentation, mlIndentation, sectionKeys, state);
		}
		/// <summary>
		/// Copies the leftover data in <see cref="Buffer"/> to <paramref name="newBuf"/>, and resets <see cref="Position"/> to 0 if successful.
		/// If <paramref name="newBuf"/> is too small to hold the leftover data (that is, smaller than <see cref="Leftover"/>), then returns the size required.
		/// If copied successfully, returns 0.
		/// After calling this method, fill the remainder of <paramref name="newBuf"/> with the next data (start filling from index <paramref name="copied"/>).
		/// Finally, call <see cref="GetState"/>, as position is reset to 0.
		/// </summary>
		/// <param name="newBuf">The buffer to copy leftover data to. Must be at least <see cref="Leftover"/> in size.</param>
		/// <param name="copied">The number of characters copied to <paramref name="newBuf"/>.</param>
		/// <returns>true on success, false on failure.</returns>
		public bool CopyLeftoverAndResetPosition(Span<char> newBuf, out int copied)
		{
			if (Position == Buffer.Length)
			{
				Position = 0;
				copied = 0;
				return true;
			}
			// If Buffer is 10 long and Position is 6, then
			//       v
			// 0123456789
			// Leftover would be 4 bytes
			ReadOnlySpan<char> leftover = Buffer.Slice(Position);
			if (newBuf.Length < Leftover)
			{
				copied = 0;
				return false;
			}
			leftover.CopyTo(newBuf);
			copied = leftover.Length;
			Position = 0;
			return true;
		}
		private CfgBufToken Token(CfgBufToken t, ReadOnlySpan<char> content = default)
		{
			Content = content;
			return t;
		}
		private CfgBufToken TokenAndKey(CfgBufToken t, CfgKey key, ReadOnlySpan<char> content = default)
		{
			Key = key;
			Content = content;
			return t;
		}
		/// <summary>
		/// Reads the next token from the buffer.
		/// If <see cref="IsFinalBlock"/> is <see langword="false"/>, then this method may return <see cref="CfgBufToken.NeedMoreData"/>. In that case,
		/// you need to call <see cref="CopyLeftoverAndResetPosition(Span{char}, out int)"/>, followed by <see cref="GetState"/>, and pass that new state as well
		/// as a buffer filled with more data to a new instance of <see cref="CfgBufferReader"/>.
		/// If <see cref="IsFinalBlock"/> is <see langword="true"/>, then this method will never <see cref="CfgBufToken.NeedMoreData"/>. Instead, it may return
		/// <see cref="CfgBufToken.End"/>, which signifies the end of the data.
		/// </summary>
		/// <returns>The type of token read.</returns>
		public CfgBufToken Read()
		{
			ReadOnlySpan<char> buf = SliceOrDefault(Buffer, Position);
			switch (state)
			{
				case ReadStreamState.Section:
					{
						LeadingNewLine = default;
						// We used to get the last character we saw and then the next, but with the buffer we won't need to do that anymore
						if (GetNextChar(buf, out int index, out _, out int indentationBeforeKeyStart, out int indentationBeforeKeyLength))
						{
							char c = buf[index];
							// Got the next one!
							switch (c)
							{
								case CfgSyntax.Comment:
									// A comment, so we need to try and read it
									// If we hit the end of the buffer and this isn't the final block then we need to try again, as we can't be sure that we read the entirety of the comment
									ReadOnlySpan<char> comment = SliceOrDefault(buf, index + 1);
									int nl = comment.IndexOfAny('\n', '\r');
									if (nl != -1)
									{
										// Found the newline, so return the comment, minus the newline
										Position += index + nl;
										return Token(CfgBufToken.Comment, comment.Slice(0, nl).TrimEnd('\r'));
									}
									else if (IsFinalBlock)
									{
										// End of stream, return the comment
										// We don't set the state here because we want to be sure that there are 0 sections left to close
										Position = Buffer.Length;
										return Token(CfgBufToken.Comment, comment);
									}
									else
									{
										// Not sure if that is the entirety of the comment or not
										return CfgBufToken.NeedMoreData;
									}
								case CfgSyntax.EndSectionOrList:
									// We don't update the state, because after closing the section we're back in Section state again, ready for another end section/list or key or comment.
									if (sectionKeys.TryPop(out CfgKey skey))
									{
										Position += index + 1;
										return TokenAndKey(CfgBufToken.EndSection, skey);
									}
									else
									{
										return Token(CfgBufToken.Error, "Found section close when there was no section to close".AsSpan());
									}
								default:
									// Any other char is the start of a key
									var bufKey = buf.Slice(index);
									int keyEnd = bufKey.IndexOfAny(CfgSyntax.KeyEnd, CfgSyntax.StartSectionOrList);
									if (keyEnd == -1)
									{
										if (IsFinalBlock)
										{
											Position = Buffer.Length;
											var s = bufKey
#if NETSTANDARD2_0
												.ToString();
#else
												;
#endif
											return Token(CfgBufToken.Error, string.Concat("Found end of stream when reading key: ", s).AsSpan());
										}
										else
										{
											return CfgBufToken.NeedMoreData;
										}
									}
									ReadOnlySpan<char> keyStr = bufKey.Slice(0, keyEnd).TrimEnd();
									CfgKey key;
									{
										CfgKey? k = CfgKey.TryCreate(keyStr);
										if (!k.HasValue)
										{
											var s = keyStr
#if NETSTANDARD2_0
												.ToString();
#else
												;
#endif
											return Token(CfgBufToken.Error, string.Concat(CfgKey.InvalidCharDescription, s).AsSpan());
										}
										key = k.Value;
									}
									// We set the position so we end on the : or { that we just found, so we can check it next iteration
									Position += index + keyEnd;
									keyIndentation = buf.Slice(indentationBeforeKeyStart, indentationBeforeKeyLength).ToString().AsMemory();
									state = ReadStreamState.Key;
									return TokenAndKey(CfgBufToken.Key, key);
							}
						}
						else
						{
							// Couldn't get any next char
							if (IsFinalBlock)
							{
								if (sectionKeys.Count == 0)
								{
									Position = Buffer.Length;
									state = ReadStreamState.End;
									return TokenAndKey(CfgBufToken.End, default);
								}
								else
								{
									state = ReadStreamState.Error;
									return Token(CfgBufToken.Error, string.Concat("Found end of stream when there were still ", sectionKeys.Count.ToString(), " sections to close").AsSpan());
								}
							}
							else
							{
								// Not the final block, we need more data
								return CfgBufToken.NeedMoreData;
							}
						}
					}
				case ReadStreamState.Key:
					{
						if (buf[0] == CfgSyntax.StartSectionOrList)
						{
							// Open section
							Position++;
							sectionKeys.Push(Key);
							state = ReadStreamState.Section;
							return Token(CfgBufToken.StartSection, default);
						}

						// So now we know it was Key:. Get the next char. If it is an open brace, then it's opening a list. Otherwise, it's just a Key: Value
						buf = SliceOrDefault(buf, 1);
						if (GetNextChar(buf, out int index, out bool newLineBeforeValue, out int indentationBeforeValueStart, out int indentationBeforeValueLength))
						{
							switch (buf[index])
							{
								case CfgSyntax.StartSectionOrList:
									// Start of a list
									// Skip 1 plus 1 (sliced off the leading char)
									Position += index + 2;
									state = ReadStreamState.List;
									return Token(CfgBufToken.StartList, default);
								case CfgSyntax.SingleQuote:
								case CfgSyntax.DoubleQuote:
									// Quoted string, read it
									var value = SliceOrDefault(buf, index + 1);
									int q = IndexOfNonDoubledChar(value, buf[index]);
									if (q != -1)
									{
										Position += index + q + 3;
										state = ReadStreamState.Section;
										return Token(CfgBufToken.Value, QuotedString(value.Slice(0, q), buf[index]));
									}
									else if (IsFinalBlock)
									{
										// Didn't find a closing quote
										var s = value
#if NETSTANDARD2_0
											.ToString();
#else
											;
#endif
										return Token(CfgBufToken.Error, string.Concat("Found end of stream when reading quoted string: ", s).AsSpan());
									}
									else
									{
										// Need more data
										return CfgBufToken.NeedMoreData;
									}
								default:
									// Unquoted string
									if (newLineBeforeValue)
									{
										// Newline before the value, which means it might be a multiline value.
										var indentationBeforeValue = buf.Slice(indentationBeforeValueStart, indentationBeforeValueLength);
										if (indentationBeforeValue.Length > keyIndentation.Length
											&& indentationBeforeValue.StartsWith(keyIndentation.Span))
										{
											// A multiline string
											// We don't want to include the leading cr/lf characters, so trim those off
											mlIndentation = indentationBeforeValue.ToString().AsMemory();
											Position += GetLeadingNewLineCharCount(buf) + 1;
											state = ReadStreamState.Multiline;
											return Token(CfgBufToken.StartMultiline, default);
										}
										else
										{
											/*
												* Key:
												* Blah:
												*/
											// Not more indented, so empty value
											Position += index;
											state = ReadStreamState.Section;
											return Token(CfgBufToken.Value, default);
										}
									}
									// No newline, no quotes, single-line
									/*
										* Key: Value
										*/

									var unquotedValue = buf.Slice(index);
									int nl = unquotedValue.IndexOfAny('\n', '\r');
									if (nl != -1)
									{
										// Found the newline, so return the value, minus the newline
										Position += index + nl + 1;
										state = ReadStreamState.Section;
										return Token(CfgBufToken.Value, unquotedValue.Slice(0, nl).TrimEnd());
									}
									else if (IsFinalBlock)
									{
										// End of stream, return the value. We may also return an error next invocation.
										Position = Buffer.Length;
										state = ReadStreamState.Section;
										return Token(CfgBufToken.Value, unquotedValue.TrimEnd());
									}
									else
									{
										// Not sure if that is the entirety of the value or not
										return CfgBufToken.NeedMoreData;
									}
							}
						}
						else if (IsFinalBlock)
						{
							// End of stream, it's just a Key with an empty value
							Position = Buffer.Length;
							state = ReadStreamState.End;
							return Token(CfgBufToken.Value, default);
						}
						else
						{
							// Not sure, need more data
							return CfgBufToken.NeedMoreData;
						}
					}
				case ReadStreamState.Multiline:
					{
						// Once we find a string that doesn't start with our indentation, that's the end of our multiline value
						int trim = GetLeadingNewLineCharCount(buf);
						LeadingNewLine = buf.Slice(0, trim);
						buf = SliceOrDefault(buf, trim);
						if (buf.StartsWith(mlIndentation.Span))
						{
							// We defer appending the newline until here; that way, we don't append any trailing newlines
							// And the config file can still specify a trailing newline by entering an additional indented line
							// We have another line to read in!
							// So, read from the beginning of this line up until the end of the line
							buf = SliceOrDefault(buf, mlIndentation.Length);
							int next = buf.IndexOfAny('\n', '\r');
							if (next == -1)
							{
								// Can't find either, only valid if it's the final block
								if (IsFinalBlock)
								{
									// Don't change the state here; keep it as multiline, so we'll get "EndMultiline" and then "End" - or error, if we haven't closed sections.
									Position = Buffer.Length;
									return Token(CfgBufToken.Value, buf);
								}
								else
								{
									// This MIGHT be the end of the multiline string but if there's more data, we can't be sure
									return CfgBufToken.NeedMoreData;
								}
							}
							else
							{
								// Take the slice, trim the newlines, and continue
								var line = buf.Slice(0, next);
								Position += mlIndentation.Length + line.Length + trim;
								return Token(CfgBufToken.Value, line);
							}
						}
						else
						{
							// Multiline is done
							Position += trim;
							state = ReadStreamState.Section;
							return Token(CfgBufToken.EndMultiline, default);
						}
					}
				case ReadStreamState.List:
					{
						if (GetNextChar(buf, out int index, out _, out _, out _))
						{
							ReadOnlySpan<char> value;
							switch (buf[index])
							{
								case CfgSyntax.EndSectionOrList:
									Position += index + 1;
									state = ReadStreamState.Section;
									return Token(CfgBufToken.EndList, default);
								case CfgSyntax.Comment:
									// We can't have an end of stream here, as there's a list to close
									var comment = SliceOrDefault(buf, index + 1);
									int nl_comment = comment.IndexOfAny('\n', '\r');
									if (nl_comment != -1)
									{
										Position += index + nl_comment + 1;
										// Found the newline, so return the comment, minus the newline
										return Token(CfgBufToken.Comment, comment.Slice(0, nl_comment).TrimEnd('\r'));
									}
									else if (IsFinalBlock)
									{
										// End of stream and there's a list to close, error
										var s = comment
#if NETSTANDARD2_0
											.ToString();
#else
											;
#endif
										return Token(CfgBufToken.Error, string.Concat("Found end of stream when reading a comment inside a list, and the list was not closed properly. The comment is: ", s).AsSpan());
									}
									else
									{
										// Not sure if that is the entirety of the comment or not
										return CfgBufToken.NeedMoreData;
									}
								case CfgSyntax.SingleQuote:
								case CfgSyntax.DoubleQuote:
									// Quoted string, read it
									value = SliceOrDefault(buf, index + 1);
									int q = IndexOfNonDoubledChar(value, buf[index]);
									if (q != -1)
									{
										Position += index + q + 2;
										return Token(CfgBufToken.Value, QuotedString(value.Slice(0, q), buf[index]));
									}
									else if (IsFinalBlock)
									{
										// Didn't find a closing quote
										var s = value
#if NETSTANDARD2_0
											.ToString();
#else
											;
#endif
										return Token(CfgBufToken.Error, string.Concat("Found end of stream when reading a list-contained quoted string: ", s).AsSpan());
									}
									else
									{
										// Need more data
										return CfgBufToken.NeedMoreData;
									}
								default:
									value = buf.Slice(index);
									int nl_value = value.IndexOfAny('\n', '\r');
									if (nl_value != -1)
									{
										Position += index + nl_value + 1;
										return Token(CfgBufToken.Value, value.Slice(0, nl_value).TrimEnd());
									}
									else if (IsFinalBlock)
									{
										// Didn't find a newline char, error as the list is not closed
										var s = value
#if NETSTANDARD2_0
											.ToString();
#else
											;
#endif
										return Token(CfgBufToken.Error, string.Concat("Found end of stream when reading a list-contained unquoted string: ", s).AsSpan());
									}
									else
									{
										// Need more data
										return CfgBufToken.NeedMoreData;
									}
							}
						}
						else
						{
							if (IsFinalBlock)
							{
								state = ReadStreamState.Error;
								return Token(CfgBufToken.Error, "Encountered end of stream when trying to read List Values".AsSpan());
							}
							else
							{
								return CfgBufToken.NeedMoreData;
							}
						}
					}
				case ReadStreamState.End:
					// We never allow setting the state to the end of stream unless SectionLevel is 0, so don't need to check that here.
					return TokenAndKey(CfgBufToken.End, default, default);
				default:
				case ReadStreamState.Error:
					return Token(CfgBufToken.Error, "Encountered error, cannot read further".AsSpan());
			}
		}
		private static int GetLeadingNewLineCharCount(ReadOnlySpan<char> buf)
		{
			if (buf.Length > 0)
			{
				if (buf[0] == '\n')
				{
					return 1;
				}
				else if (buf[0] == '\r')
				{
					return buf.Length > 1 && buf[1] == '\n'
						? 2
						: 1;
				}
			}
			return 0;
		}
		/// <summary>
		/// Returns a slice of <paramref name="s"/> if <paramref name="start"/> is within the bounds of <paramref name="s"/>.
		/// If <paramref name="start"/> is out of bounds, returns an empty span.
		/// </summary>
		private static ReadOnlySpan<char> SliceOrDefault(ReadOnlySpan<char> s, int start)
		{
			return start < s.Length
				? s.Slice(start)
				: default;
		}
		private static int IndexOfNonDoubledChar(ReadOnlySpan<char> s, char q)
		{
			int offset = 0;
			while (true)
			{
				int i = s.IndexOf(q);
				if (i == -1) return -1;
				// If we found the char, and there is another identical char directly after it, then we skip those two chars and keep searching
				if (s.Length > (i + 2) && s[i + 1] == q)
				{
					s = s.Slice(i + 2);
					offset = i + 2;
				}
				else
				{
					return i + offset;
				}
			}
		}
		private static bool GetNextChar(ReadOnlySpan<char> s, out int index, out bool sawNewLine, out int indStart, out int indLen)
		{
			indStart = 0;
			indLen = 0;
			sawNewLine = false;
			for (int i = 0; i < s.Length; i++)
			{
				char c = s[i];
				if (!char.IsWhiteSpace(c))
				{
					index = i;
					return true;
				}
				if (c == '\n' || c == '\r')
				{
					sawNewLine = true;
					indStart++;
					indLen = 0;
				}
				else
				{
					indLen++;
				}
			}
			index = -1;
			return false;
		}
		private static ReadOnlySpan<char> QuotedString(ReadOnlySpan<char> s, char q)
		{
			char[] str = new char[s.Length];
			int k = 0;
			bool sawQuote = false;
			for (int i = 0; i < s.Length; i++)
			{
				char c = s[i];
				if (c == q)
				{
					if (sawQuote)
					{
						str[k++] = c;
						sawQuote = false;
					}
					else
					{
						sawQuote = true;
					}
				}
				else
				{
					str[k++] = c;
				}
			}
			return str.AsSpan().Slice(0, k);
		}
	}
}
#pragma warning restore IDE0057 // Use range operator