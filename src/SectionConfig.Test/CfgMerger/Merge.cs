namespace SectionConfig.Test.CfgMerger
{
	using SectionConfig;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Reflection;
	using Xunit;

	public static class Merge
	{
		[Fact]
		public static void NoConflicts()
		{
			CfgRoot r1 = new(StringComparer.Ordinal);
			CfgRoot r2 = new(StringComparer.Ordinal);
			r1.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value1"));
			r1.TryAdd(new CfgValueList(CfgKey.Create("List1"), new List<string>() { "Value2", "Value3", "Value4" }));
			r1.TryAdd(new CfgSection(CfgKey.Create("Section1"), StringComparer.Ordinal));

			r2.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value5"));
			r2.TryAdd(new CfgValueList(CfgKey.Create("List2"), new List<string>() { "Value6", "Value7", "Value8" }));
			r2.TryAdd(new CfgSection(CfgKey.Create("Section2"), StringComparer.Ordinal));

			CfgMerger m = new(StringComparer.Ordinal, MergeBehaviour.Fail, MergeBehaviour.Fail, MergeBehaviour.Fail, false);
			IReadOnlyDictionary<string, ICfgObject> elems = m.MergeAll(r1, r2).ValueOrException().Elements;

			Assert.Equal(6, elems.Count);
			Assert.Equal("Value1", Assert.Contains("Key1", elems).ToValue().Value);
			Assert.Collection(Assert.Contains("List1", elems).ToValueList().Values,
				x => Assert.Equal("Value2", x),
				x => Assert.Equal("Value3", x),
				x => Assert.Equal("Value4", x));
			Assert.Empty(Assert.Contains("Section1", elems).ToSection().Elements);

			Assert.Equal("Value5", Assert.Contains("Key2", elems).ToValue().Value);
			Assert.Collection(Assert.Contains("List2", elems).ToValueList().Values,
				x => Assert.Equal("Value6", x),
				x => Assert.Equal("Value7", x),
				x => Assert.Equal("Value8", x));
			Assert.Empty(Assert.Contains("Section2", elems).ToSection().Elements);
		}
		[Fact]
		public static void MismatchingTypes_ValueAndSection_Conflict()
		{
			CfgRoot r1 = new(StringComparer.Ordinal);
			CfgRoot r2 = new(StringComparer.Ordinal);

			r1.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value1"));
			r2.TryAdd(new CfgSection(CfgKey.Create("Key1"), StringComparer.Ordinal));
			CfgMerger m = new(StringComparer.Ordinal, MergeBehaviour.TakeBoth, MergeBehaviour.TakeBoth, MergeBehaviour.TakeBoth, true);
			ErrMsg<MergeError> error = m.MergeAll(r1, r2).ErrorOrException();

			Assert.Equal(MergeError.MismatchingTypes, error.Code);
			Assert.Equal("Merging the duplicate key Key1 failed, because they are different types. Existing is Value whereas incoming is Section", error.Message);
		}
		[Fact]
		public static void MismatchingTypes_ValueListAndSection_Conflict()
		{
			CfgRoot r1 = new(StringComparer.Ordinal);
			CfgRoot r2 = new(StringComparer.Ordinal);

			r1.TryAdd(new CfgValueList(CfgKey.Create("Key1"), new List<string>() { "Value1" }));
			r2.TryAdd(new CfgSection(CfgKey.Create("Key1"), StringComparer.Ordinal));
			CfgMerger m = new(StringComparer.Ordinal, MergeBehaviour.TakeBoth, MergeBehaviour.TakeBoth, MergeBehaviour.TakeBoth, true);
			ErrMsg<MergeError> error = m.MergeAll(r1, r2).ErrorOrException();

			Assert.Equal(MergeError.MismatchingTypes, error.Code);
			Assert.Equal("Merging the duplicate key Key1 failed, because they are different types. Existing is ValueList whereas incoming is Section", error.Message);
		}
		[Fact]
		public static void SectionConflictTakeBoth()
		{
			CfgRoot r1 = new(StringComparer.Ordinal);
			CfgRoot r2 = new(StringComparer.Ordinal);
			CfgRoot r3 = new(StringComparer.Ordinal);

			CfgSection s = new(CfgKey.Create("Section"), StringComparer.Ordinal);
			s.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value1"));
			s.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value2"));
			r1.TryAdd(s);

			s = new(CfgKey.Create("Section"), StringComparer.Ordinal);
			s.TryAdd(new CfgValue(CfgKey.Create("Key3"), "Value3"));
			s.TryAdd(new CfgValue(CfgKey.Create("Key4"), "Value4"));
			r2.TryAdd(s);

			s = new(CfgKey.Create("Section"), StringComparer.Ordinal);
			s.TryAdd(new CfgValue(CfgKey.Create("Key5"), "Value5"));
			s.TryAdd(new CfgValue(CfgKey.Create("Key6"), "Value6"));
			r3.TryAdd(s);

			CfgMerger m = new(StringComparer.Ordinal, MergeBehaviour.TakeBoth, MergeBehaviour.Fail, MergeBehaviour.Fail, false);
			IReadOnlyDictionary<string, ICfgObject> elems = m.MergeAll(r1, r2, r3).ValueOrException().Elements;

			Assert.Equal(1, elems.Count);
			s = Assert.Contains("Section", elems).ToSection();
			Assert.Equal(6, s.Elements.Count);
			Assert.Equal("Value1", Assert.Contains("Key1", s.Elements).ToValue().Value);
			Assert.Equal("Value2", Assert.Contains("Key2", s.Elements).ToValue().Value);
			Assert.Equal("Value3", Assert.Contains("Key3", s.Elements).ToValue().Value);
			Assert.Equal("Value4", Assert.Contains("Key4", s.Elements).ToValue().Value);
			Assert.Equal("Value5", Assert.Contains("Key5", s.Elements).ToValue().Value);
			Assert.Equal("Value6", Assert.Contains("Key6", s.Elements).ToValue().Value);
		}
		[Fact]
		public static void SectionConflictTakeFirst()
		{
			CfgRoot r1 = new(StringComparer.Ordinal);
			CfgRoot r2 = new(StringComparer.Ordinal);
			CfgRoot r3 = new(StringComparer.Ordinal);

			CfgSection s = new(CfgKey.Create("Section"), StringComparer.Ordinal);
			s.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value1"));
			s.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value2"));
			r1.TryAdd(s);

			s = new(CfgKey.Create("Section"), StringComparer.Ordinal);
			s.TryAdd(new CfgValue(CfgKey.Create("Key3"), "Value3"));
			s.TryAdd(new CfgValue(CfgKey.Create("Key4"), "Value4"));
			r2.TryAdd(s);

			s = new(CfgKey.Create("Section"), StringComparer.Ordinal);
			s.TryAdd(new CfgValue(CfgKey.Create("Key5"), "Value5"));
			s.TryAdd(new CfgValue(CfgKey.Create("Key6"), "Value6"));
			r3.TryAdd(s);

			CfgMerger m = new(StringComparer.Ordinal, MergeBehaviour.TakeFirst, MergeBehaviour.Fail, MergeBehaviour.Fail, false);
			IReadOnlyDictionary<string, ICfgObject> elems = m.MergeAll(r1, r2, r3).ValueOrException().Elements;

			Assert.Equal(1, elems.Count);
			s = Assert.Contains("Section", elems).ToSection();
			Assert.Equal(2, s.Elements.Count);
			Assert.Equal("Value1", Assert.Contains("Key1", s.Elements).ToValue().Value);
			Assert.Equal("Value2", Assert.Contains("Key2", s.Elements).ToValue().Value);
		}
		[Fact]
		public static void SectionConflictTakeLast()
		{
			CfgRoot r1 = new(StringComparer.Ordinal);
			CfgRoot r2 = new(StringComparer.Ordinal);
			CfgRoot r3 = new(StringComparer.Ordinal);

			CfgSection s = new(CfgKey.Create("Section"), StringComparer.Ordinal);
			s.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value1"));
			s.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value2"));
			r1.TryAdd(s);

			s = new(CfgKey.Create("Section"), StringComparer.Ordinal);
			s.TryAdd(new CfgValue(CfgKey.Create("Key3"), "Value3"));
			s.TryAdd(new CfgValue(CfgKey.Create("Key4"), "Value4"));
			r2.TryAdd(s);

			s = new(CfgKey.Create("Section"), StringComparer.Ordinal);
			s.TryAdd(new CfgValue(CfgKey.Create("Key5"), "Value5"));
			s.TryAdd(new CfgValue(CfgKey.Create("Key6"), "Value6"));
			r3.TryAdd(s);

			CfgMerger m = new(StringComparer.Ordinal, MergeBehaviour.TakeLast, MergeBehaviour.Fail, MergeBehaviour.Fail, false);
			IReadOnlyDictionary<string, ICfgObject> elems = m.MergeAll(r1, r2, r3).ValueOrException().Elements;

			Assert.Equal(1, elems.Count);
			s = Assert.Contains("Section", elems).ToSection();
			Assert.Equal(2, s.Elements.Count);
			Assert.Equal("Value5", Assert.Contains("Key5", s.Elements).ToValue().Value);
			Assert.Equal("Value6", Assert.Contains("Key6", s.Elements).ToValue().Value);
		}
		[Fact]
		public static void SectionConflictFail()
		{
			CfgRoot r1 = new(StringComparer.Ordinal);
			CfgRoot r2 = new(StringComparer.Ordinal);
			CfgRoot r3 = new(StringComparer.Ordinal);

			CfgSection s = new(CfgKey.Create("Section"), StringComparer.Ordinal);
			s.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value1"));
			s.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value2"));
			r1.TryAdd(s);

			s = new(CfgKey.Create("Section"), StringComparer.Ordinal);
			s.TryAdd(new CfgValue(CfgKey.Create("Key3"), "Value3"));
			s.TryAdd(new CfgValue(CfgKey.Create("Key4"), "Value4"));
			r2.TryAdd(s);

			s = new(CfgKey.Create("Section"), StringComparer.Ordinal);
			s.TryAdd(new CfgValue(CfgKey.Create("Key5"), "Value5"));
			s.TryAdd(new CfgValue(CfgKey.Create("Key6"), "Value6"));
			r3.TryAdd(s);

			CfgMerger m = new(StringComparer.Ordinal, MergeBehaviour.Fail, MergeBehaviour.Fail, MergeBehaviour.Fail, false);

			ErrMsg<MergeError> error = m.MergeAll(r1, r2, r3).ErrorOrException();
			Assert.Equal(MergeError.DuplicateSection, error.Code);
			Assert.Equal("Merging the duplicate section with key Section failed because no duplicates are allowed", error.Message);
			Assert.Equal("Merging the duplicate section with key Section failed because no duplicates are allowed", error.ToString());
		}
		[Fact]
		public static void ValueConflictTakeBoth_CollectLists()
		{
			CfgRoot r1 = new(StringComparer.Ordinal);
			CfgRoot r2 = new(StringComparer.Ordinal);
			CfgRoot r3 = new(StringComparer.Ordinal);

			r1.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value1"));
			r1.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value2"));

			r2.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value3"));
			r2.TryAdd(new CfgValueList(CfgKey.Create("Key2"), new string[] { "Value4" }));

			r3.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value5"));
			r3.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value6"));

			CfgMerger m = new(StringComparer.Ordinal, MergeBehaviour.Fail, MergeBehaviour.TakeBoth, MergeBehaviour.Fail, true);
			IReadOnlyDictionary<string, ICfgObject> elems = m.MergeAll(r1, r2, r3).ValueOrException().Elements;

			Assert.Equal(2, elems.Count);
			Assert.Collection(Assert.Contains("Key1", elems).ToValueList().Values,
				x => Assert.Equal("Value1", x),
				x => Assert.Equal("Value3", x),
				x => Assert.Equal("Value5", x));
			Assert.Collection(Assert.Contains("Key2", elems).ToValueList().Values,
				x => Assert.Equal("Value2", x),
				x => Assert.Equal("Value4", x),
				x => Assert.Equal("Value6", x));
		}
		[Fact]
		public static void ValueConflictTakeBoth_DoNotCollectLists_NoTypeConflicts()
		{
			CfgRoot r1 = new(StringComparer.Ordinal);
			CfgRoot r2 = new(StringComparer.Ordinal);

			r1.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value1"));
			r1.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value2"));

			r2.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value3"));
			r2.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value4"));

			CfgMerger m = new(StringComparer.Ordinal, MergeBehaviour.Fail, MergeBehaviour.TakeBoth, MergeBehaviour.Fail, false);
			IReadOnlyDictionary<string, ICfgObject> elems = m.MergeAll(r1, r2).ValueOrException().Elements;

			Assert.Equal(2, elems.Count);
			Assert.Collection(Assert.Contains("Key1", elems).ToValueList().Values,
				x => Assert.Equal("Value1", x),
				x => Assert.Equal("Value3", x));
			Assert.Collection(Assert.Contains("Key2", elems).ToValueList().Values,
				x => Assert.Equal("Value2", x),
				x => Assert.Equal("Value4", x));
		}
		[Fact]
		public static void ValueConflictTakeBoth_DoNotCollectLists_WithTypeConflicts()
		{
			CfgRoot r1 = new(StringComparer.Ordinal);
			CfgRoot r2 = new(StringComparer.Ordinal);
			CfgRoot r3 = new(StringComparer.Ordinal);

			r1.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value1"));
			r1.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value2"));

			r2.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value3"));
			r2.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value4"));

			r3.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value5"));
			r3.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value6"));

			CfgMerger m = new(StringComparer.Ordinal, MergeBehaviour.Fail, MergeBehaviour.TakeBoth, MergeBehaviour.Fail, false);
			ErrMsg<MergeError> error = m.MergeAll(r1, r2, r3).ErrorOrException();

			Assert.Equal(MergeError.MismatchingTypes, error.Code);
			Assert.Equal("Merging the duplicate key Key1 failed, because they are different types. Existing is ValueList whereas incoming is Value", error.Message);
		}
		[Fact]
		public static void ValueConflictTakeFirst()
		{
			CfgRoot r1 = new(StringComparer.Ordinal);
			CfgRoot r2 = new(StringComparer.Ordinal);
			CfgRoot r3 = new(StringComparer.Ordinal);

			r1.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value1"));
			r1.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value2"));

			r2.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value3"));
			r2.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value4"));

			r3.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value5"));
			r3.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value6"));

			CfgMerger m = new(StringComparer.Ordinal, MergeBehaviour.Fail, MergeBehaviour.TakeFirst, MergeBehaviour.Fail, false);
			IReadOnlyDictionary<string, ICfgObject> elems = m.MergeAll(r1, r2, r3).ValueOrException().Elements;

			Assert.Equal(2, elems.Count);
			Assert.Equal("Value1", Assert.Contains("Key1", elems).ToValue().Value);
			Assert.Equal("Value2", Assert.Contains("Key2", elems).ToValue().Value);
		}
		[Fact]
		public static void ValueConflictTakeLast()
		{
			CfgRoot r1 = new(StringComparer.Ordinal);
			CfgRoot r2 = new(StringComparer.Ordinal);
			CfgRoot r3 = new(StringComparer.Ordinal);

			r1.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value1"));
			r1.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value2"));

			r2.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value3"));
			r2.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value4"));

			r3.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value5"));
			r3.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value6"));

			CfgMerger m = new(StringComparer.Ordinal, MergeBehaviour.Fail, MergeBehaviour.TakeLast, MergeBehaviour.Fail, false);
			IReadOnlyDictionary<string, ICfgObject> elems = m.MergeAll(r1, r2, r3).ValueOrException().Elements;

			Assert.Equal(2, elems.Count);
			Assert.Equal("Value5", Assert.Contains("Key1", elems).ToValue().Value);
			Assert.Equal("Value6", Assert.Contains("Key2", elems).ToValue().Value);
		}
		[Fact]
		public static void ValueConflictFail()
		{
			CfgRoot r1 = new(StringComparer.Ordinal);
			CfgRoot r2 = new(StringComparer.Ordinal);
			CfgRoot r3 = new(StringComparer.Ordinal);

			r1.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value1"));
			r1.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value2"));

			r2.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value3"));
			r2.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value4"));

			r3.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value5"));
			r3.TryAdd(new CfgValue(CfgKey.Create("Key2"), "Value6"));

			CfgMerger m = new(StringComparer.Ordinal, MergeBehaviour.Fail, MergeBehaviour.Fail, MergeBehaviour.Fail, false);
			ErrMsg<MergeError> error = m.MergeAll(r1, r2, r3).ErrorOrException();
			Assert.Equal(MergeError.DuplicateValue, error.Code);
			Assert.Equal("Merging the duplicate value with key Key1 failed because no duplicates are allowed", error.Message);
		}
		[Fact]
		public static void ValueListConflictTakeBoth()
		{
			CfgRoot r1 = new(StringComparer.Ordinal);
			CfgRoot r2 = new(StringComparer.Ordinal);
			CfgRoot r3 = new(StringComparer.Ordinal);

			r1.TryAdd(new CfgValueList(CfgKey.Create("Key1"), new List<string>() { "Value1a", "Value1b" }));
			r1.TryAdd(new CfgValueList(CfgKey.Create("Key2"), new List<string>() { "Value2a", "Value2b" }));

			r2.TryAdd(new CfgValueList(CfgKey.Create("Key1"), new List<string>() { "Value3a", "Value3b" }));
			r2.TryAdd(new CfgValueList(CfgKey.Create("Key2"), new List<string>() { "Value4a", "Value4b" }));

			r3.TryAdd(new CfgValueList(CfgKey.Create("Key1"), new List<string>() { "Value5a", "Value5b" }));
			r3.TryAdd(new CfgValueList(CfgKey.Create("Key2"), new List<string>() { "Value6a", "Value6b" }));

			CfgMerger m = new(StringComparer.Ordinal, MergeBehaviour.Fail, MergeBehaviour.Fail, MergeBehaviour.TakeBoth, false);
			IReadOnlyDictionary<string, ICfgObject> elems = m.MergeAll(r1, r2, r3).ValueOrException().Elements;

			Assert.Equal(2, elems.Count);
			Assert.Collection(Assert.Contains("Key1", elems).ToValueList().Values,
				x => Assert.Equal("Value1a", x),
				x => Assert.Equal("Value1b", x),
				x => Assert.Equal("Value3a", x),
				x => Assert.Equal("Value3b", x),
				x => Assert.Equal("Value5a", x),
				x => Assert.Equal("Value5b", x));
			Assert.Collection(Assert.Contains("Key2", elems).ToValueList().Values,
				x => Assert.Equal("Value2a", x),
				x => Assert.Equal("Value2b", x),
				x => Assert.Equal("Value4a", x),
				x => Assert.Equal("Value4b", x),
				x => Assert.Equal("Value6a", x),
				x => Assert.Equal("Value6b", x));
		}
		[Fact]
		public static void ValueListConflictTakeFirst()
		{
			CfgRoot r1 = new(StringComparer.Ordinal);
			CfgRoot r2 = new(StringComparer.Ordinal);
			CfgRoot r3 = new(StringComparer.Ordinal);

			r1.TryAdd(new CfgValueList(CfgKey.Create("Key1"), new List<string>() { "Value1a", "Value1b" }));
			r1.TryAdd(new CfgValueList(CfgKey.Create("Key2"), new List<string>() { "Value2a", "Value2b" }));

			r2.TryAdd(new CfgValueList(CfgKey.Create("Key1"), new List<string>() { "Value3a", "Value3b" }));
			r2.TryAdd(new CfgValueList(CfgKey.Create("Key2"), new List<string>() { "Value4a", "Value4b" }));

			r3.TryAdd(new CfgValueList(CfgKey.Create("Key1"), new List<string>() { "Value5a", "Value5b" }));
			r3.TryAdd(new CfgValueList(CfgKey.Create("Key2"), new List<string>() { "Value6a", "Value6b" }));

			CfgMerger m = new(StringComparer.Ordinal, MergeBehaviour.Fail, MergeBehaviour.Fail, MergeBehaviour.TakeFirst, false);
			IReadOnlyDictionary<string, ICfgObject> elems = m.MergeAll(r1, r2, r3).ValueOrException().Elements;

			Assert.Equal(2, elems.Count);
			Assert.Collection(Assert.Contains("Key1", elems).ToValueList().Values,
				x => Assert.Equal("Value1a", x),
				x => Assert.Equal("Value1b", x));
			Assert.Collection(Assert.Contains("Key2", elems).ToValueList().Values,
				x => Assert.Equal("Value2a", x),
				x => Assert.Equal("Value2b", x));
		}
		[Fact]
		public static void ValueListConflictTakeLast()
		{
			CfgRoot r1 = new(StringComparer.Ordinal);
			CfgRoot r2 = new(StringComparer.Ordinal);
			CfgRoot r3 = new(StringComparer.Ordinal);

			r1.TryAdd(new CfgValueList(CfgKey.Create("Key1"), new List<string>() { "Value1a", "Value1b" }));
			r1.TryAdd(new CfgValueList(CfgKey.Create("Key2"), new List<string>() { "Value2a", "Value2b" }));

			r2.TryAdd(new CfgValueList(CfgKey.Create("Key1"), new List<string>() { "Value3a", "Value3b" }));
			r2.TryAdd(new CfgValueList(CfgKey.Create("Key2"), new List<string>() { "Value4a", "Value4b" }));

			r3.TryAdd(new CfgValueList(CfgKey.Create("Key1"), new List<string>() { "Value5a", "Value5b" }));
			r3.TryAdd(new CfgValueList(CfgKey.Create("Key2"), new List<string>() { "Value6a", "Value6b" }));

			CfgMerger m = new(StringComparer.Ordinal, MergeBehaviour.Fail, MergeBehaviour.Fail, MergeBehaviour.TakeLast, false);
			IReadOnlyDictionary<string, ICfgObject> elems = m.MergeAll(r1, r2, r3).ValueOrException().Elements;

			Assert.Equal(2, elems.Count);
			Assert.Collection(Assert.Contains("Key1", elems).ToValueList().Values,
				x => Assert.Equal("Value5a", x),
				x => Assert.Equal("Value5b", x));
			Assert.Collection(Assert.Contains("Key2", elems).ToValueList().Values,
				x => Assert.Equal("Value6a", x),
				x => Assert.Equal("Value6b", x));
		}
		[Fact]
		public static void ValueListConflictFail()
		{
			CfgRoot r1 = new(StringComparer.Ordinal);
			CfgRoot r2 = new(StringComparer.Ordinal);
			CfgRoot r3 = new(StringComparer.Ordinal);

			r1.TryAdd(new CfgValueList(CfgKey.Create("Key1"), new List<string>() { "Value1a", "Value1b" }));
			r1.TryAdd(new CfgValueList(CfgKey.Create("Key2"), new List<string>() { "Value2a", "Value2b" }));

			r2.TryAdd(new CfgValueList(CfgKey.Create("Key1"), new List<string>() { "Value3a", "Value3b" }));
			r2.TryAdd(new CfgValueList(CfgKey.Create("Key2"), new List<string>() { "Value4a", "Value4b" }));

			r3.TryAdd(new CfgValueList(CfgKey.Create("Key1"), new List<string>() { "Value5a", "Value5b" }));
			r3.TryAdd(new CfgValueList(CfgKey.Create("Key2"), new List<string>() { "Value6a", "Value6b" }));

			CfgMerger m = new(StringComparer.Ordinal, MergeBehaviour.Fail, MergeBehaviour.Fail, MergeBehaviour.Fail, false);
			ErrMsg<MergeError> error = m.MergeAll(r1, r2, r3).ErrorOrException();
			Assert.Equal(MergeError.DuplicateValueList, error.Code);
			Assert.Equal("Merging the duplicate value list with key Key1 failed because no duplicates are allowed", error.Message);
		}
		[Fact]
		public static void InvalidEnumValues()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new CfgMerger(StringComparer.Ordinal, (MergeBehaviour)999, MergeBehaviour.Fail, MergeBehaviour.Fail, false));
			Assert.Throws<ArgumentOutOfRangeException>(() => new CfgMerger(StringComparer.Ordinal, MergeBehaviour.TakeBoth, (MergeBehaviour)999, MergeBehaviour.Fail, false));
			Assert.Throws<ArgumentOutOfRangeException>(() => new CfgMerger(StringComparer.Ordinal, MergeBehaviour.TakeBoth, MergeBehaviour.Fail, (MergeBehaviour)999, false));

			CfgMerger m = new(StringComparer.Ordinal, MergeBehaviour.TakeBoth, MergeBehaviour.Fail, MergeBehaviour.Fail, false);
			Assert.Throws<ArgumentOutOfRangeException>(() => m.SectionMerging = (MergeBehaviour)999);
			Assert.Throws<ArgumentOutOfRangeException>(() => m.ValueMerging = (MergeBehaviour)999);
			Assert.Throws<ArgumentOutOfRangeException>(() => m.ValueListMerging = (MergeBehaviour)999);

			// It's not really possible to set them to something invalid then try to merge, but we'll hack it with reflection and set them just to make it fail during a merge

			Type typeCfgMerger = typeof(CfgMerger);
			FieldInfo? field = typeCfgMerger.GetField("sectionMerging", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.NotNull(field);
			field!.SetValue(m, 100);

			CfgRoot r1 = new(StringComparer.Ordinal);
			CfgRoot r2 = new(StringComparer.Ordinal);

			CfgSection s = new(CfgKey.Create("Section"), StringComparer.Ordinal);
			s.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value1"));
			s.TryAdd(new CfgValueList(CfgKey.Create("Key2"), new string[] { "Value2" }));
			r1.TryAdd(s);

			s = new(CfgKey.Create("Section"), StringComparer.Ordinal);
			s.TryAdd(new CfgValue(CfgKey.Create("Key1"), "Value3"));
			s.TryAdd(new CfgValueList(CfgKey.Create("Key2"), new string[] { "Value4" }));
			r2.TryAdd(s);

			Assert.Throws<InvalidOperationException>(() => m.MergeAll(r1, r2));

			m.SectionMerging = MergeBehaviour.TakeBoth;
			field = typeCfgMerger.GetField("valueMerging", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.NotNull(field);
			field!.SetValue(m, 100);

			Assert.Throws<InvalidOperationException>(() => m.MergeAll(r1, r2));

			m.ValueMerging = MergeBehaviour.TakeBoth;
			field = typeCfgMerger.GetField("valueListMerging", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.NotNull(field);
			field!.SetValue(m, 100);

			Assert.Throws<InvalidOperationException>(() => m.MergeAll(r1, r2));
		}
		[Fact]
		public static void Properties()
		{
			CfgMerger m = new(StringComparer.Ordinal, MergeBehaviour.Fail, MergeBehaviour.Fail, MergeBehaviour.Fail, false);
			m.SectionMerging = MergeBehaviour.TakeBoth;
			m.ValueMerging = MergeBehaviour.TakeFirst;
			m.ValueListMerging = MergeBehaviour.TakeLast;
			Assert.Equal(MergeBehaviour.TakeBoth, m.SectionMerging);
			Assert.Equal(MergeBehaviour.TakeFirst, m.ValueMerging);
			Assert.Equal(MergeBehaviour.TakeLast, m.ValueListMerging);
		}
	}
}
