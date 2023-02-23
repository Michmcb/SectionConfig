namespace SectionConfig.IO
{
	using System;

	/// <summary>
	/// A token created when a value list is opened. Can be used to close the list. It can also be disposed of, for the same effect.
	/// </summary>
	public readonly struct WriteValueListToken : IDisposable
	{
		private readonly CfgStreamWriter writer;
		private readonly int id;
		internal WriteValueListToken(CfgStreamWriter writer, int id)
		{
			this.writer = writer;
			this.id = id;
		}
		/// <summary>
		/// Closes the value list. Throws <see cref="InvalidOperationException"/> if this value list has already been closed.
		/// </summary>
		/// <exception cref="InvalidOperationException"/>
		public void Close()
		{
			writer.WriteCloseValueList(id);
		}
		/// <summary>
		/// Writes a value to the list.
		/// </summary>
		/// <param name="val">The value to write.</param>
		public void WriteListValue(ReadOnlySpan<char> val)
		{
			writer.WriteListValue(val);
		}
		/// <summary>
		/// Same as <see cref="Close"/>.
		/// </summary>
		public void Dispose()
		{
			Close();
		}
	}
}
