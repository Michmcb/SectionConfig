namespace SectionConfig.IO
{
	using System;

	/// <summary>
	/// A token created when a section is opened. Can be used to close the section. It can also be disposed of, for the same effect.
	/// </summary>
	public readonly struct WriteSectionToken : IDisposable
	{
		private readonly CfgStreamWriter writer;
		private readonly int id;
		internal WriteSectionToken(CfgStreamWriter writer, int id)
		{
			this.writer = writer;
			this.id = id;
		}
		/// <summary>
		/// Closes the section. Throws <see cref="InvalidOperationException"/> if a section is closed out of order, or if this section has already been closed.
		/// </summary>
		/// <exception cref="InvalidOperationException"/>
		public void Close()
		{
			writer.WriteCloseSection(id);
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
