using System;

namespace AutoMapper
{
	internal static class PrimitiveExtensions
	{
		public static string ToNullSafeString(this object value)
		{
			return value == null ? string.Empty : value.ToString();
		}
        
		public static bool IsNullableType(this Type type)
		{
			return type.IsGenericType && (type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
		}
	}
}
