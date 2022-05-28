namespace SectionConfig.Test
{
	using System;
	using System.Collections;
	using Xunit;
	using static SectionConfig.Strings;

	// We suppress this warning because we do want to actually make sure the Count property returns the correct number, as well as there being just 1 item in the collection
#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.
	public static class StringsTest
	{
		[Fact]
		public static void Default()
		{
			Strings strs = default;
			Assert.Single(strs);
			Assert.Null(strs.ToString());
			Assert.Null(strs[0]);
			Assert.Collection(strs, x => Assert.Null(x));
			Assert.Throws<IndexOutOfRangeException>(() => strs[1]);
		}
		[Fact]
		public static void OneString()
		{
			Strings strs = new("Single");
			Assert.Equal(1, strs.Count);
			Assert.Single(strs);
			Assert.Equal("Single", strs.ToString());
			Assert.Equal("Single", strs.ToString(','));
			Assert.Equal("Single", strs.ToString(" and "));
			Assert.Equal("Single", strs[0]);
			Assert.Collection(strs, x => Assert.Equal("Single", x));
			Assert.Throws<IndexOutOfRangeException>(() => strs[1]);

			SingleEnumerator iter = Assert.IsType<SingleEnumerator>(strs.GetEnumerator());
			Assert.IsType<SingleEnumerator>(((IEnumerable)strs).GetEnumerator());
			for (int i = 0; i < 2; i++)
			{
				Assert.Null(iter.Current);
				Assert.True(iter.MoveNext());
				Assert.Equal("Single", iter.Current);
				Assert.Equal("Single", ((IEnumerator)iter).Current);
				Assert.False(iter.MoveNext());
				Assert.Null(iter.Current);
				Assert.False(iter.MoveNext());
				Assert.Null(iter.Current);
				iter.Reset();
			}
		}
		[Fact]
		public static void ArrayLengthZero()
		{
			Strings strs = new(Array.Empty<string>());
			Assert.Equal(0, strs.Count);
			Assert.Empty(strs);
			Assert.Null(strs.ToString());
			Assert.Null(strs.ToString(','));
			Assert.Null(strs.ToString(" and "));
			Assert.Throws<IndexOutOfRangeException>(() => strs[0]);
		}
		[Fact]
		public static void ArrayLengthOne()
		{
			Strings strs = new(new string[] { "Single" });
			Assert.Equal(1, strs.Count);
			Assert.Single(strs);
			Assert.Equal("Single", strs.ToString());
			Assert.Equal("Single", strs[0]);
			Assert.Collection(strs, x => Assert.Equal("Single", x));
			Assert.Throws<IndexOutOfRangeException>(() => strs[1]);
		}
		[Fact]
		public static void ArrayLengthThree()
		{
			Strings strs = new(new string[] { "One", "Two", "Three" });
			Assert.Equal(3, strs.Count);
			Assert.Equal("One", strs.ToString());
			Assert.Equal("One,Two,Three", strs.ToString(','));
			Assert.Equal("One and Two and Three", strs.ToString(" and "));
			Assert.Equal("One", strs[0]);
			Assert.Equal("Two", strs[1]);
			Assert.Equal("Three", strs[2]);
			Assert.Throws<IndexOutOfRangeException>(() => strs[3]);
			Assert.Collection(strs,
				x => Assert.Equal("One", x),
				x => Assert.Equal("Two", x),
				x => Assert.Equal("Three", x));
		}
	}
#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
}
