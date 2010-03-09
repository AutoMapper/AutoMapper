using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper
{
	internal static class PrimitiveExtensions
	{
		public static string ToNullSafeString(this object value)
		{
			return value == null ? null : value.ToString();
		}
        
		public static bool IsNullableType(this Type type)
		{
			return type.IsGenericType && (type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
		}

        public static Type GetTypeOfNullable(this Type type)
        {
            return type.GetGenericArguments()[0];
        }

        
		public static bool IsEnumerableType(this Type type)
		{
			return type.GetInterfaces().Contains(typeof (IEnumerable));
		}

		public static bool IsListType(this Type type)
		{
			return type.GetInterfaces().Contains(typeof (IList));
		}

		public static bool IsListOrDictionaryType(this Type type)
		{
			return type.IsListType() || type.IsDictionaryType();
		}

		public static bool IsDictionaryType(this Type type)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
				return true;

			var genericInterfaces = type.GetInterfaces().Where(t => t.IsGenericType);
			var baseDefinitions = genericInterfaces.Select(t => t.GetGenericTypeDefinition());
			return baseDefinitions.Any(t => t == typeof(IDictionary<,>));
		}

		public static Type GetDictionaryType(this Type type)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
				return type;

			var genericInterfaces = type.GetInterfaces().Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>));
			return genericInterfaces.FirstOrDefault();
		}
	}
}
