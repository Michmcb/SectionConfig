namespace SectionConfig.IO
{
	using System;
	using System.IO;
	/// <summary>
	/// Reads section config data from a stream, forward-only.
	/// </summary>
	[Obsolete("Prefer " + nameof(IO.CfgStreamReader) + ", which uses leaveOpen instead of closeInput like .NET streams do.")]
	public sealed class SectionCfgReader : IDisposable
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="reader">The stream to read. Does not have to be seekable.</param>
		/// <param name="closeInput">If true, closes <paramref name="reader"/> on disposal.</param>
		public SectionCfgReader(TextReader reader, bool closeInput = true)
		{
			CfgStreamReader = new(reader, !closeInput);
		}
		/// <summary>
		/// The <see cref="IO.CfgStreamReader"/> that this instance wraps.
		/// </summary>
		public CfgStreamReader CfgStreamReader { get; }
		/// <summary>
		/// The underlying <see cref="TextReader"/> being used.
		/// </summary>
		public TextReader Reader => CfgStreamReader.Reader;
		/// <summary>
		/// <see langword="true"/> to leave <see cref="Reader"/> open after this <see cref="IO.CfgStreamReader"/> is disposed. Otherwise, false.
		/// </summary>
		public bool CloseInput { get => !CfgStreamReader.LeaveOpen; set => CfgStreamReader.LeaveOpen = !value; }
		/// <summary>
		/// The current state of the reader
		/// </summary>
		public ReadStreamState State => CfgStreamReader.State;
		/// <summary>
		/// The current nesting level of sections. Initially it is 0.
		/// </summary>
		public int SectionLevel => CfgStreamReader.SectionLevel;
		/// <summary>
		/// Reads the next token from <see cref="Reader"/>.
		/// </summary>
		/// <returns>The result of reading the next token.</returns>
		public ReadResult Read()
		{
			return CfgStreamReader.Read();
		}
		/// <summary>
		/// If <see cref="CloseInput"/> is true, disposes of <see cref="Reader"/>. Otherwise, does not.
		/// </summary>
		public void Dispose()
		{
			CfgStreamReader.Dispose();
		}
	}
}
