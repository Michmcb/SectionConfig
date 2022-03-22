namespace SectionConfig.Test
{
	using SectionConfig.IO;
	using System;
	using Xunit;

	public static class InvalidCfgKeyExceptionTest
	{
		[Fact]
		public static void Ctor()
		{
			Exception inner = new("inner");
			InvalidCfgKeyException ex1 = new();
			InvalidCfgKeyException ex2 = new("msg");
			InvalidCfgKeyException ex3 = new("msg", inner);


			Assert.Equal("Exception of type 'SectionConfig.IO.InvalidCfgKeyException' was thrown.", ex1.Message);
			Assert.Null(ex1.InnerException);

			Assert.Equal("msg", ex2.Message);
			Assert.Null(ex2.InnerException);

			Assert.Equal("msg", ex3.Message);
			Assert.Equal(inner, ex3.InnerException);
		}
	}
}
