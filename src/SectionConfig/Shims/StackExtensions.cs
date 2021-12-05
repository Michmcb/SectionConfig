#if NETSTANDARD2_0
namespace System.Collections.Generic
{
	/// <summary>
	/// Shims for .netstandard 2.0.
	/// </summary>
	public static class StackExtensions
	{
		/// <summary>
		/// If <paramref name="s"/> has at least one item, pops it and returns true. Otherwise returns false.
		/// </summary>
		/// <typeparam name="T">The type.</typeparam>
		/// <param name="s">The stack.</param>
		/// <param name="obj">The popped item.</param>
		/// <returns>True if an item was popped, false otherwise.</returns>
		public static bool TryPop<T>(this Stack<T> s, out T obj)
		{
			if (s.Count > 0)
			{
				obj = s.Pop();
				return true;
			}
			else
			{
				obj = default;
				return false;
			}
		}
	}
}
#endif