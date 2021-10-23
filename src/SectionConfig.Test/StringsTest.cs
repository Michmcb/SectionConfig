namespace SectionConfig.Test
{
	using System;
	using Xunit;

	public static class StringsTest
	{
		[Fact]
		public static void Default()
		{
			Strings strs = default;
			Assert.Single(strs);
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
			Assert.Single(strs);
			Assert.Single(strs);
			Assert.Equal("Single", strs.ToString());
			Assert.Equal("Single", strs[0]);
			Assert.Collection(strs, x => Assert.Equal("Single", x));
			Assert.Throws<IndexOutOfRangeException>(() => strs[1]);
		}
		[Fact]
		public static void ArrayLengthOne()
		{
			Strings strs = new(new string[] { "Single" });
			Assert.Single(strs);
			Assert.Single(strs);
			Assert.Equal("Single", strs.ToString());
			Assert.Equal("Single", strs[0]);
			Assert.Collection(strs, x => Assert.Equal("Single", x));
			Assert.Throws<IndexOutOfRangeException>(() => strs[1]);
		}
		[Fact]
		public static void Array()
		{
			Strings strs = new(new string[] { "One", "Two", "Three" });
			Assert.Equal(3, strs.Count);
			Assert.Equal("One", strs.ToString());
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
}
