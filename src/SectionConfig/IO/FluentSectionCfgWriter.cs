namespace SectionConfig.IO
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Helps with writing a config file in a fluent way.
	/// </summary>
	public sealed class FluentSectionCfgWriter : IDisposable
	{
		private bool cantUse;
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="closeWriter">If true, will dispose of <paramref name="writer"/> when this class is disposed of.</param>
		public FluentSectionCfgWriter(SectionCfgWriter writer, bool closeWriter = true)
		{
			cantUse = false;
			Writer = writer;
			CloseWriter = closeWriter;
		}
		/// <summary>
		/// The writer to write to.
		/// </summary>
		public SectionCfgWriter Writer { get; }
		/// <summary>
		/// If true, disposes of <see cref="Writer"/> when this class is disposed of.
		/// Otherwise disposal of this class does nothing.
		/// </summary>
		public bool CloseWriter { get; set; }
		/// <summary>
		/// Writes a key and value.
		/// </summary>
		/// <param name="key">The key to write.</param>
		/// <param name="value">The value to write.</param>
		/// <returns>this.</returns>
		public FluentSectionCfgWriter Value(CfgKey key, ReadOnlySpan<char> value)
		{
			ThrowIfCantUse();
			Writer.WriteKeyValue(key, value);
			return this;
		}
		/// <summary>
		/// Writes a comment.
		/// </summary>
		/// <param name="comment">The comment to write.</param>
		/// <param name="replaceLineBreaks">If true, any linebreaks in the comment are replaced with <see cref="SectionCfgWriter.NewLine"/>.</param>
		/// <returns>this.</returns>
		public FluentSectionCfgWriter Comment(ReadOnlySpan<char> comment, bool replaceLineBreaks = true)
		{
			ThrowIfCantUse();
			Writer.WriteComment(comment, replaceLineBreaks);
			return this;
		}
		/// <summary>
		/// Writes a key and opens a section, then invokes <paramref name="section"/> to allow writing things in that section.
		/// </summary>
		/// <param name="key">The key to write.</param>
		/// <param name="section">A callback invoked to write further things in the section.</param>
		/// <returns>this.</returns>
		public FluentSectionCfgWriter Section(CfgKey key, Action<FluentSectionCfgWriter> section)
		{
			ThrowIfCantUse();
			cantUse = true;
			WriteSectionToken t = Writer.WriteKeyOpenSection(key);
			section(new(Writer, closeWriter: false));
			t.Close();
			cantUse = false;
			return this;
		}
		/// <summary>
		/// Writes a key and value list, with a single value.
		/// </summary>
		/// <param name="key">The key to write.</param>
		/// <param name="value">The single value to write.</param>
		/// <returns>this.</returns>
		public FluentSectionCfgWriter ValueList(CfgKey key, ReadOnlySpan<char> value)
		{
			ThrowIfCantUse();
			WriteValueListToken t = Writer.WriteKeyOpenValueList(key);
			t.WriteListValue(value);
			t.Close();
			return this;
		}
		/// <summary>
		/// Writes a key and value list, with a single value.
		/// </summary>
		/// <param name="key">The key to write.</param>
		/// <param name="value">The single value to write.</param>
		/// <returns>this.</returns>
		public FluentSectionCfgWriter ValueList(CfgKey key, string value)
		{
			ThrowIfCantUse();
			return ValueList(key, value.AsSpan());
		}
		/// <summary>
		/// Writes a key and value list, with many values.
		/// </summary>
		/// <param name="key">The key to write.</param>
		/// <param name="values">The values to write.</param>
		/// <returns>this.</returns>
		public FluentSectionCfgWriter ValueList(CfgKey key, params string[] values)
		{
			ThrowIfCantUse();
			return ValueList(key, (IEnumerable<string>)values);
		}
		/// <summary>
		/// Writes a key and value list, with many values.
		/// </summary>
		/// <param name="key">The key to write.</param>
		/// <param name="values">The values to write.</param>
		/// <returns>this.</returns>
		public FluentSectionCfgWriter ValueList(CfgKey key, IEnumerable<string> values)
		{
			ThrowIfCantUse();
			WriteValueListToken t = Writer.WriteKeyOpenValueList(key);
			foreach (string v in values)
			{
				t.WriteListValue(v.AsSpan());
			}
			t.Close();
			return this;
		}
		private void ThrowIfCantUse()
		{
			if (cantUse)
			{
				throw new InvalidOperationException("You can't use the outer CfgBuilder whilst you have created an inner CfgBuilder for writing a section.");
			}
		}
		/// <summary>
		/// If <see cref="CloseWriter"/> is true, then calls <see cref="SectionCfgWriter.Dispose"/>. Otherwise, does nothing.
		/// </summary>
		public void Dispose()
		{
			if (CloseWriter)
			{
				Writer.Dispose();
			}
		}
	}
}
