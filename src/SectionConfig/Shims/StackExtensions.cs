#if NETSTANDARD2_0
namespace System.Collections.Generic
{
	public static class StackExtensions
	{
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