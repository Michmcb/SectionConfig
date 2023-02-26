namespace SectionConfig.IO
{
	/// <summary>
	/// A loader which can produce an object of type <typeparamref name="T"/> from a <see cref="CfgStreamReader"/>.
	/// </summary>
	/// <typeparam name="T">The type of object produced.</typeparam>
	public interface ICfgLoader<T> : ICfgLoaderSync<T>, ICfgLoaderAsync<T> where T : class { }
}