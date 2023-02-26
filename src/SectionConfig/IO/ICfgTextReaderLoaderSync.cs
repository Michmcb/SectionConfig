//namespace SectionConfig.IO
//{
//	using System.IO;
//	/// <summary>
//	/// A loader which can produce an object of type <typeparamref name="T"/> from a <see cref="TextReader"/>.
//	/// </summary>
//	/// <typeparam name="T">The type of object produced.</typeparam>
//	public interface ICfgTextReaderLoaderSync<T> where T : class
//	{
//		/// <summary>
//		/// Loads section config data from <paramref name="reader"/>.
//		/// </summary>
//		/// <param name="reader">The reader to read data from.</param>
//		/// <param name="initialBufferSize">The buffer size to use.</param>
//		/// <returns>A <typeparamref name="T"/> with all loaded data, or an <see cref="ErrMsg{TCode}"/> describing an error which prevented the data from being loaded.</returns>
//		ValOrErr<T, ErrMsg<LoadError>> TryLoadTextReader(TextReader reader, int initialBufferSize = 0);
//	}
//}