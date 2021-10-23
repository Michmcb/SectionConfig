#if NETSTANDARD2_1
namespace System.Runtime.CompilerServices
{
	internal static class Unsafe
	{
		internal static T As<T>(object? o)
		{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
			return (T)o;
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
		}
	}
}
#endif
#if NETSTANDARD2_0
#pragma warning disable IDE0060 // Remove unused parameter
namespace System.Diagnostics.CodeAnalysis
{
	[AttributeUsage(AttributeTargets.ReturnValue, Inherited = false)]
	internal sealed class NotNullIfNotNullAttribute : Attribute
	{
		internal NotNullIfNotNullAttribute(string parameterName) { }
	}
	[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
	internal sealed class NotNullWhenAttribute : Attribute
	{
		internal NotNullWhenAttribute(bool returnValue) { }
	}
}
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