namespace SectionConfig.IO
{
	/// <summary>
	/// A loader which can produce an object of type <typeparamref name="T"/> from a <see cref="SectionCfgReader"/>.
	/// </summary>
	/// <typeparam name="T">The type of object produced.</typeparam>
	public interface ICfgLoader<T> where T : class
	{
		/// <summary>
		/// Loads section config data from <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <returns>A <typeparamref name="T"/> with all loaded data, or an <see cref="ErrMsg{TCode}"/> describing an error which prevented the data from being loaded.</returns>
		ValOrErr<T, ErrMsg<LoadError>> TryLoad(CfgStreamReader reader);
	}
}
