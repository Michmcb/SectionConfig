namespace SectionConfig.Test
{
	using SectionConfig;
	using System;

	public static class Extensions
	{
		public static TVal ValueOrException<TVal, TErr>(this ValOrErr<TVal, TErr> ve) where TVal : class where TErr : struct
		{
			return ve.Value ?? throw new Exception("Value was null. Error is:" + ve.Error.ToString());
		}
		public static TErr ErrorOrException<TVal, TErr>(this ValOrErr<TVal, TErr> ve) where TVal : class where TErr : struct
		{
			return ve.Value != null ? throw new Exception("Value was not null") : ve.Error;
		}
	}
}
