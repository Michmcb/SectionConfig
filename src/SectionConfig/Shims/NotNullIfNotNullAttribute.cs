#if NETSTANDARD2_0
namespace System.Diagnostics.CodeAnalysis
{
	[AttributeUsage(AttributeTargets.ReturnValue, Inherited = false)]
	internal sealed class NotNullIfNotNullAttribute : Attribute
	{
		internal NotNullIfNotNullAttribute(string parameterName) { }
	}
}
#endif