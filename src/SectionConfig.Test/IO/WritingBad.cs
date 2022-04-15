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
		public static void CantWriteDefaultKey()
		{
			SectionCfgWriter scw = new(StreamWriter.Null, newLine: NewLine.Lf);
			Assert.Throws<ArgumentException>(() => scw.WriteKey(default));
		}
		[Fact]
		public static void CantWriteAfterDisposal()
		{
			SectionCfgWriter scw = new(StreamWriter.Null, newLine: NewLine.Lf);
			scw.Dispose();
			Assert.Throws<InvalidOperationException>(() => scw.WriteKey(default));
			Assert.Throws<InvalidOperationException>(() => scw.WriteValue(default));
			Assert.Throws<InvalidOperationException>(() => scw.WriteComment(default));
			Assert.Throws<InvalidOperationException>(() => scw.WriteKeyOpenSection(default));
			Assert.Throws<InvalidOperationException>(() => scw.WriteKeyOpenValueList(default));
			Assert.Throws<InvalidOperationException>(() => scw.WriteKeyValue(default, default));
		}
		[Fact]
		public static void BadIndentation()
		{
			Assert.Throws<ArgumentException>(() => new SectionCfgWriter(StreamWriter.Null, indentation: "abc".AsMemory(), newLine: NewLine.Lf));
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
			Assert.Throws<InvalidOperationException>(() => vlt.Dispose());
		}
		[Fact]
		public static void BadStateCloseListOutOfOrder()
		{
			using SectionCfgWriter scw = new(StreamWriter.Null, newLine: NewLine.Lf);
			WriteValueListToken vlt1 = scw.WriteKeyOpenValueList(CfgKey.Create("Key"));
			vlt1.Close();
			WriteValueListToken vlt2 = scw.WriteKeyOpenValueList(CfgKey.Create("Key"));
			Assert.Throws<InvalidOperationException>(() => vlt1.Dispose());
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
			Assert.Throws<InvalidOperationException>(() => st1.Dispose());
		}
		[Fact]
		public static void BadStateCloseSectionWithNestedValueListOutOfOrder()
		{
			using SectionCfgWriter scw = new(StreamWriter.Null, newLine: NewLine.Lf);
			WriteSectionToken st = scw.WriteKeyOpenSection(CfgKey.Create("Key"));
			WriteValueListToken vlt = scw.WriteKeyOpenValueList(CfgKey.Create("Key"));
			Assert.Throws<InvalidOperationException>(() => st.Dispose());
		}
		[Fact]
		public static void TryToCloseValueListWithWrongToken()
		{
			using SectionCfgWriter scw = new(StreamWriter.Null, newLine: NewLine.Lf);
			WriteValueListToken vlt1 = scw.WriteKeyOpenValueList(CfgKey.Create("Key1"));
			vlt1.Close();
			WriteValueListToken vlt2 = scw.WriteKeyOpenValueList(CfgKey.Create("Key2"));
			Assert.Throws<InvalidOperationException>(() => vlt1.Close());
		}
	}
}
