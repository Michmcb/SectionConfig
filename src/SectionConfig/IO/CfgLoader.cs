namespace SectionConfig.IO
{
	using System;
	using System.Buffers;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// Helper class for loading config data from various sources.
	/// </summary>
	public static class CfgLoader
	{
		/// <summary>
		/// Loads section config data from the file at <paramref name="path"/>, reading using the <paramref name="encoding"/>.
		/// </summary>
		/// <param name="path">The file to read.</param>
		/// <param name="encoding">Encoding to interpet file as.</param>
		/// <param name="loader">The loader that to invoke.</param>
		/// <returns>A <typeparamref name="T"/> with all loaded data, or an <see cref="ErrMsg{TCode}"/> describing an error which prevented the data from being loaded.</returns>
		public static ValOrErr<T, ErrMsg<LoadError>> TryLoad<T>(string path, Encoding encoding, ICfgLoaderSync<T> loader)
			where T : class
		{
			using CfgStreamReader scr = new(new StreamReader(path, encoding), leaveOpen: false);
			return loader.TryLoad(scr);
		}
		/// <summary>
		/// Loads section config data from <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <param name="loader">The loader that to invoke.</param>
		/// <returns>A <typeparamref name="T"/> with all loaded data, or an <see cref="ErrMsg{TCode}"/> describing an error which prevented the data from being loaded.</returns>
		public static ValOrErr<T, ErrMsg<LoadError>> TryLoad<T>(TextReader reader, ICfgLoaderSync<T> loader)
			where T : class
		{
			using CfgStreamReader scr = new(reader, leaveOpen: false);
			return loader.TryLoad(scr);
		}
		/// <summary>
		/// Loads section config data from <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <param name="loader">The loader that to invoke.</param>
		/// <returns>A <typeparamref name="T"/> with all loaded data, or an <see cref="ErrMsg{TCode}"/> describing an error which prevented the data from being loaded.</returns>
		public static ValOrErr<T, ErrMsg<LoadError>> TryLoad<T>(StreamReader reader, ICfgLoaderSync<T> loader)
			where T : class
		{
			using CfgStreamReader scr = new(reader, leaveOpen: false);
			return loader.TryLoad(scr);
		}
		/// <summary>
		/// Loads section config data from the file at <paramref name="path"/>, reading using the <paramref name="encoding"/>.
		/// </summary>
		/// <param name="path">The file to read.</param>
		/// <param name="encoding">Encoding to interpet file as.</param>
		/// <param name="loader">The loader that to invoke.</param>
		/// <returns>A <typeparamref name="T"/> with all loaded data, or an <see cref="ErrMsg{TCode}"/> describing an error which prevented the data from being loaded.</returns>
		public static async Task<ValOrErr<T, ErrMsg<LoadError>>> TryLoadAsync<T>(string path, Encoding encoding, ICfgLoaderAsync<T> loader)
			where T : class
		{
			using CfgStreamReader scr = new(new StreamReader(path, encoding), leaveOpen: false);
			return await loader.TryLoadAsync(scr);
		}
		/// <summary>
		/// Loads section config data from <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <param name="loader">The loader that to invoke.</param>
		/// <returns>A <typeparamref name="T"/> with all loaded data, or an <see cref="ErrMsg{TCode}"/> describing an error which prevented the data from being loaded.</returns>
		public static async Task<ValOrErr<T, ErrMsg<LoadError>>> TryLoadAsync<T>(TextReader reader, ICfgLoaderAsync<T> loader)
			where T : class
		{
			using CfgStreamReader scr = new(reader, leaveOpen: false);
			return await loader.TryLoadAsync(scr);
		}
		/// <summary>
		/// Loads section config data from <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">The reader to read data from.</param>
		/// <param name="loader">The loader that to invoke.</param>
		/// <returns>A <typeparamref name="T"/> with all loaded data, or an <see cref="ErrMsg{TCode}"/> describing an error which prevented the data from being loaded.</returns>
		public static async Task<ValOrErr<T, ErrMsg<LoadError>>> TryLoadAsync<T>(StreamReader reader, ICfgLoaderAsync<T> loader)
			where T : class
		{
			using CfgStreamReader scr = new(reader, leaveOpen: false);
			return await loader.TryLoadAsync(scr);
		}
	}
}
