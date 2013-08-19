using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    public static class TypeHelper
	{
		public static Type GetElementType(Type enumerableType)
		{
			return GetElementType(enumerableType, null);
		}

		public static Type GetElementType(Type enumerableType, IEnumerable enumerable)
		{
			if (enumerableType.HasElementType)
			{
				return enumerableType.GetElementType();
			}

			if (enumerableType.IsGenericType && enumerableType.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>)))
			{
				return enumerableType.GetGenericArguments()[0];
			}

            Type ienumerableType = GetIEnumerableType(enumerableType);
            if (ienumerableType != null)
			{
				return ienumerableType.GetGenericArguments()[0];
			}

			if (typeof(IEnumerable).IsAssignableFrom(enumerableType))
			{
				if (enumerable != null)
				{
					var first = enumerable.Cast<object>().FirstOrDefault();
					if (first != null)
						return first.GetType();
				}
				return typeof(object);
			}

			throw new ArgumentException(String.Format("Unable to find the element type for type '{0}'.", enumerableType), "enumerableType");
		}

		public static Type GetEnumerationType(Type enumType)
		{
			if (enumType.IsNullableType())
			{
				enumType = enumType.GetGenericArguments()[0];
			}

			if (!enumType.IsEnum)
				return null;

			return enumType;
		}

        private static Type GetIEnumerableType(Type enumerableType)
        {
            try
            {
                return enumerableType.GetInterfaces().FirstOrDefault(t => t.Name == "IEnumerable`1");
            }
            catch (System.Reflection.AmbiguousMatchException)
            {
                if (enumerableType.BaseType != typeof(object))
                    return GetIEnumerableType(enumerableType.BaseType);

                return null;
            }
        }
	}
}