#if NETSTANDARD2_0
namespace System.Diagnostics.CodeAnalysis
{
	[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
	internal sealed class NotNullWhenAttribute : Attribute
	{
		internal NotNullWhenAttribute(bool returnValue) { }
	}
}
#endif