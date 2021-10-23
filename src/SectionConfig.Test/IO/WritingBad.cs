namespace SectionConfig.Test.IO
{
	using SectionConfig.IO;
	using System;
	using System.IO;
	using Xunit;

	public static class WritingBad
	{
		[Fact]
		public static void CfgKeyBadValue()
		{
			Assert.False(CfgKey.TryCreate("").HasValue);
			Assert.False(CfgKey.TryCreate(" ").HasValue);
			Assert.False(CfgKey.TryCreate("\t").HasValue);
			Assert.False(CfgKey.TryCreate("Key\nKey").HasValue);
			Assert.False(CfgKey.TryCreate("Key\rKey").HasValue);
			Assert.False(CfgKey.TryCreate("#").HasValue);
			Assert.False(CfgKey.TryCreate("{").HasValue);
			Assert.False(CfgKey.TryCreate("}").HasValue);
			Assert.False(CfgKey.TryCreate(":").HasValue);

			Assert.Throws<InvalidCfgKeyException>(() => CfgKey.Create(""));
			Assert.Throws<InvalidCfgKeyException>(() => CfgKey.Create(" "));
			Assert.Throws<InvalidCfgKeyException>(() => CfgKey.Create("\t"));
			Assert.Throws<InvalidCfgKeyException>(() => CfgKey.Create("Key\nKey"));
			Assert.Throws<InvalidCfgKeyException>(() => CfgKey.Create("Key\rKey"));
			Assert.Throws<InvalidCfgKeyException>(() => CfgKey.Create("#"));
			Assert.Throws<InvalidCfgKeyException>(() => CfgKey.Create("{"));
			Assert.Throws<InvalidCfgKeyException>(() => CfgKey.Create("}"));
			Assert.Throws<InvalidCfgKeyException>(() => CfgKey.Create(":"));
		}
		[Fact]
		public static void BadStateKey()
		{
			using SectionCfgWriter scw = new(StreamWriter.Null, newLine: NewLine.Lf);
			scw.WriteKey(CfgKey.Create("Key"));
			Assert.Throws<InvalidOperationException>(() => scw.WriteKey(CfgKey.Create("Key")));
		}
		[Fact]
		public static void BadStateValue()
		{
			using SectionCfgWriter scw = new(StreamWriter.Null, newLine: NewLine.Lf);
			Assert.Throws<InvalidOperationException>(() => scw.WriteValue("Value"));
		}
		[Fact]
		public static void BadStateOpenValueList()
		{
			using SectionCfgWriter scw = new(StreamWriter.Null, newLine: NewLine.Lf);
			Assert.Throws<InvalidOperationException>(() => scw.WriteOpenValueList());
		}
		[Fact]
		public static void BadStateOpenSectionList()
		{
			using SectionCfgWriter scw = new(StreamWriter.Null, newLine: NewLine.Lf);
			Assert.Throws<InvalidOperationException>(() => scw.WriteOpenSection());
		}
		[Fact]
		public static void BadStateListValue()
		{
			using SectionCfgWriter scw = new(StreamWriter.Null, newLine: NewLine.Lf);
			WriteValueListToken vlt = scw.WriteKeyOpenValueList(CfgKey.Create("Key"));
			vlt.Close();
			Assert.Throws<InvalidOperationException>(() => vlt.WriteListValue("Value"));
		}
		[Fact]
		public static void BadStateCloseListTwice()
		{
			using SectionCfgWriter scw = new(StreamWriter.Null, newLine: NewLine.Lf);
			WriteValueListToken vlt = scw.WriteKeyOpenValueList(CfgKey.Create("Key"));
			vlt.Close();
			Assert.Throws<InvalidOperationException>(() => vlt.Close());
		}
		[Fact]
		public static void BadStateCloseSectionTwice()
		{
			using SectionCfgWriter scw = new(StreamWriter.Null, newLine: NewLine.Lf);
			WriteSectionToken st = scw.WriteKeyOpenSection(CfgKey.Create("Key"));
			st.Close();
			Assert.Throws<InvalidOperationException>(() => st.Close());
		}
		[Fact]
		public static void BadStateCloseSectionOutOfOrder()
		{
			using SectionCfgWriter scw = new(StreamWriter.Null, newLine: NewLine.Lf);
			WriteSectionToken st1 = scw.WriteKeyOpenSection(CfgKey.Create("Key"));
			WriteSectionToken st2 = scw.WriteKeyOpenSection(CfgKey.Create("Key"));
			Assert.Throws<InvalidOperationException>(() => st1.Close());
		}
		[Fact]
		public static void BadStateCloseSectionWithNestedValueListOutOfOrder()
		{
			using SectionCfgWriter scw = new(StreamWriter.Null, newLine: NewLine.Lf);
			WriteSectionToken st = scw.WriteKeyOpenSection(CfgKey.Create("Key"));
			WriteValueListToken vlt = scw.WriteKeyOpenValueList(CfgKey.Create("Key"));
			Assert.Throws<InvalidOperationException>(() => st.Close());
		}
	}
}
