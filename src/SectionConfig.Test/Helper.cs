namespace SectionConfig.Test
{
	using SectionConfig.IO;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using Xunit;
	using Xunit.Sdk;

	public static class Helper
	{
		public static void AssertSpanEqual(ReadOnlySpan<char> expected, ReadOnlySpan<char> actual)
		{
			int i = 0;
			for (; i < Math.Min(expected.Length, actual.Length); i++)
			{
				if (expected[i] != actual[i])
				{
					break;
				}
			}
			if (i == expected.Length && i == actual.Length)
			{
				return;
			}
			throw new EqualException(new string(expected), new string(actual), i, i);
		}
		public static void TestCfgBufferReader(string str, IEnumerable<BufReadResult> expected)
		{
			// We do the entire lot at once so we don't need to worry about NeedMoreData tokens
			{
				CfgBufferReader cbr = new(str, isFinalBlock: true);
				int i = 0;
				foreach (BufReadResult er in expected)
				{
					Assert.Equal(er.Token, cbr.Read());
					Assert.Equal(er.Key, cbr.Key);
					AssertSpanEqual(er.Content.Span, cbr.Content);
					AssertSpanEqual(er.LeadingNewLine.Span, cbr.LeadingNewLine);
					i++;
				}
			}
			// Make sure that passing in a default state works too
			{
				CfgBufferReader cbr = new(str, isFinalBlock: true, state: default);
				int i = 0;
				foreach (BufReadResult er in expected)
				{
					Assert.Equal(er.Token, cbr.Read());
					Assert.Equal(er.Key, cbr.Key);
					AssertSpanEqual(er.Content.Span, cbr.Content);
					AssertSpanEqual(er.LeadingNewLine.Span, cbr.LeadingNewLine);
					i++;
				}
			}

			// CfgLoader takes care of all NeedMoreData, so we can test the buffering here
			// We use a tiny buffer size to be sure that we'll have to resize the buffer
			using (IEnumerator<BufReadResult> e = expected.GetEnumerator())
			{
				CfgLoader.ReadAllBuffered(new StringReader(str), e, (tokens, actualToken, reader) =>
				{
					Assert.True(tokens.MoveNext());
					var expected = tokens.Current;
					Assert.Equal(expected.Token, actualToken);
					Assert.Equal(expected.Key, reader.Key);
					AssertSpanEqual(expected.Content.Span, reader.Content);
					AssertSpanEqual(expected.LeadingNewLine.Span, reader.LeadingNewLine);
					return true;
				}, initialBufferSize: 16);
			}
			// And here, we use a large buffer size so we aren't using stackalloc
			using (IEnumerator<BufReadResult> e = expected.GetEnumerator())
			{
				CfgLoader.ReadAllBuffered(new StringReader(str), e, (tokens, actualToken, reader) =>
				{
					Assert.True(tokens.MoveNext());
					var expected = tokens.Current;
					Assert.Equal(expected.Token, actualToken);
					Assert.Equal(expected.Key, reader.Key);
					AssertSpanEqual(expected.Content.Span, reader.Content);
					AssertSpanEqual(expected.LeadingNewLine.Span, reader.LeadingNewLine);
					return true;
				}, initialBufferSize: CfgStreamReader.DefaultBufferSize);
			}
		}
		public static async Task TestCfgStreamReader(string str, IEnumerable<ReadResult> expectedResults, LoadError expectedError = LoadError.Ok)
		{
			StringComparer cmp = StringComparer.Ordinal;
			ValOrErr<CfgRoot, ErrMsg<LoadError>> expectedLoadResult;
			if (expectedError != LoadError.Ok)
			{
				expectedLoadResult = new(new ErrMsg<LoadError>(expectedError, expectedResults.First(x => x.Token == SectionCfgToken.Error).Content));
			}
			else
			{
				CfgRoot root = new(cmp);
				Stack<ICfgObjectParent> sections = new();
				CfgValueList? list = null;
				sections.Push(root);
				foreach (var er in expectedResults)
				{
					switch (er.Token)
					{
						case SectionCfgToken.Value:
							sections.Peek().TryAdd(new CfgValue(er.Key, er.Content));
							break;
						case SectionCfgToken.StartList:
							list = new(er.Key, new List<string>());
							break;
						case SectionCfgToken.ListValue:
							Assert.NotNull(list);
							list.Values.Add(er.Content);
							break;
						case SectionCfgToken.EndList:
							Assert.NotNull(list);
							sections.Peek().TryAdd(list);
							list = null;
							break;
						case SectionCfgToken.StartSection:
							var s = new CfgSection(er.Key, cmp);
							sections.Peek().TryAdd(s);
							sections.Push(s);
							break;
						case SectionCfgToken.EndSection:
							sections.Pop();
							break;
					}
				}
				expectedLoadResult = new(root);
			}
			// Allocating a small buffer size causes more re-reads and buffer copies, which is good because we want to make sure that works
			using (CfgStreamReader csr = new(new StringReader(str), initialBufferSize: 16))
			{
				int i = 0;
				foreach (var er in expectedResults)
				{
					ReadResult ar = csr.Read();
					Assert.Equal(er.Token, ar.Token);
					Assert.Equal(er.Key, ar.Key);
					Assert.Equal(er.Content, ar.Content);
					i++;
				}
			}
			using (CfgStreamReader csr = new(new StringReader(str), initialBufferSize: 16))
			{
				int i = 0;
				foreach (var er in expectedResults)
				{
					ReadResult ar = await csr.ReadAsync();
					Assert.Equal(er.Token, ar.Token);
					Assert.Equal(er.Key, ar.Key);
					Assert.Equal(er.Content, ar.Content);
					i++;
				}
			}
			using (CfgStreamReader csr = new(new StringReader(str), initialBufferSize: 16))
			{
				var actual = new CfgRootCfgLoader(cmp).TryLoad(csr);
				if (expectedLoadResult.Value == null)
				{
					Assert.Null(actual.Value);
					Assert.Equal(expectedLoadResult.Error.Code, actual.Error.Code);
					Assert.Equal(expectedLoadResult.Error.Message, actual.Error.Message);
				}
				else
				{
					Assert.NotNull(actual.Value);
					CheckCfgEqual(expectedLoadResult.Value, actual.Value);
				}
			}
			using (CfgStreamReader csr = new(new StringReader(str), initialBufferSize: 16))
			{
				var actual = await new CfgRootCfgLoader(cmp).TryLoadAsync(csr);
				if (expectedLoadResult.Value == null)
				{
					Assert.Null(actual.Value);
					Assert.Equal(expectedLoadResult.Error.Code, actual.Error.Code);
					Assert.Equal(expectedLoadResult.Error.Message, actual.Error.Message);
				}
				else
				{
					Assert.NotNull(actual.Value);
					CheckCfgEqual(expectedLoadResult.Value, actual.Value);
				}
			}
			IDictionary<string, Strings>? expectedDict = expectedLoadResult.Value != null ? ToDictionary(expectedLoadResult.Value, new(), new(cmp)) : null;
			using (CfgStreamReader csr = new(new StringReader(str), initialBufferSize: 16))
			{
				ValOrErr<Dictionary<string, Strings>, ErrMsg<LoadError>> actualResult = new DictionaryCfgLoader(cmp).TryLoad(csr);
				CheckDict(actualResult);
			}
			using (CfgStreamReader csr = new(new StringReader(str), initialBufferSize: 16))
			{
				ValOrErr<Dictionary<string, Strings>, ErrMsg<LoadError>> actualResult = await new DictionaryCfgLoader(cmp).TryLoadAsync(csr);
				CheckDict(actualResult);
			}

			void CheckDict(ValOrErr<Dictionary<string, Strings>, ErrMsg<LoadError>> actualResult)
			{
				if (expectedDict == null)
				{
					Assert.Null(actualResult.Value);
					Assert.Equal(expectedLoadResult.Error.Code, actualResult.Error.Code);
					Assert.Equal(expectedLoadResult.Error.Message, actualResult.Error.Message);
				}
				else
				{
					IDictionary<string, Strings>? actualDict = actualResult.Value;
					Assert.NotNull(actualDict);
					foreach (var expectedKvp in expectedDict)
					{
						Assert.Equal(expectedKvp.Value, Assert.Contains(expectedKvp.Key, actualDict));
						actualDict.Remove(expectedKvp.Key);
					}
					Assert.Empty(actualDict);
				}
			}
		}
		private static Dictionary<string, Strings> ToDictionary(ICfgObjectParent root, Stack<string> keys, Dictionary<string, Strings> dict)
		{
			foreach (var e in root.Elements.Values)
			{
				string key = FullKey(keys, e.Key.KeyString);
				e.MatchType(
				value =>
				{
					dict.Add(key, value.Value);
				},
				list =>
				{
					dict.Add(FullKey(keys, list.Key.KeyString), list.Values.ToArray());
				},
				section =>
				{
					keys.Push(section.Key.KeyString);
					ToDictionary(section, keys, dict);
					keys.Pop();
				});
			}
			return dict;
		}
		private static void CheckCfgEqual(ICfgObjectParent expected, ICfgObjectParent actual)
		{
			Assert.Equal(expected.Elements.Count, actual.Elements.Count);
			foreach (var ekvp in expected.Elements)
			{
				var aElement = Assert.Contains(ekvp.Key, actual.Elements);
				var eElement = ekvp.Value;
				Assert.Equal(eElement.Key, aElement.Key);
				Assert.Equal(eElement.Type, aElement.Type);
				switch (eElement.Type)
				{
					case CfgType.Value:
						Assert.Equal(eElement.ToValue().Value, aElement.ToValue().Value);
						break;
					case CfgType.ValueList:
						Assert.Equal((IEnumerable<string>)eElement.ToValueList().Values, (IEnumerable<string>)aElement.ToValueList().Values);
						break;
					case CfgType.Section:
						CheckCfgEqual(eElement.ToSection(), aElement.ToSection());
						break;
				}
			}
		}
		private static string FullKey(Stack<string> keys, string key)
		{
			keys.Push(key);
			string fullKey = string.Join(CfgSyntax.KeyEnd, keys.Reverse());
			keys.Pop();
			return fullKey;
		}
	}
}
