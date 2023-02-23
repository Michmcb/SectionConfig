namespace SectionConfig.Test
{
	using SectionConfig;
	using SectionConfig.IO;
	using System;
	using System.IO;
	using Xunit;

	public static class FluentSectionCfgWriterTest
	{
		[Fact]
		public static void Works()
		{
			using StringWriter str = new();
			FluentSectionCfgWriter builder = new(new CfgStreamWriter(str, newLine: NewLine.Lf));
				builder
					.Value(CfgKey.Create("Key1"), "value")
					.Value(CfgKey.Create("Key2"), "value")
					.Comment("This is the section")
					.NewLine()
					.Section(CfgKey.Create("Section"), section => section
						.Value(CfgKey.Create("Key3"), "value")
						.Value(CfgKey.Create("Key4"), "value"))
					.ValueList(CfgKey.Create("Key5"), "OneString")
					.ValueList(CfgKey.Create("Key6"), "One", "Two");
			builder.Dispose();
			Assert.Equal(WriteStreamState.End, builder.CfgStreamWriter.State);
			string cfg = str.ToString();
			Assert.Equal("Key1: value\nKey2: value\n#This is the section\n\nSection {\n\tKey3: value\n\tKey4: value\n}\nKey5: {\n\tOneString\n}\nKey6: {\n\tOne\n\tTwo\n}\n", str.ToString());
		}
		[Fact]
		public static void CantAbuse()
		{
			using CfgStreamWriter writer = new(TextWriter.Null, newLine: NewLine.Lf);
			FluentSectionCfgWriter builder = new(writer);
			builder
				.Section(CfgKey.Create("Section"), section =>
				{
					Assert.Throws<InvalidOperationException>(() => builder.Value(CfgKey.Create("KeyBad"), "value"));
				});
		}
	}
}
