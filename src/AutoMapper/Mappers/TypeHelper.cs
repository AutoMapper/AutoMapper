using System;
using System.Collections;
using System.Collections.Generic;

namespace AutoMapper.Mappers
{
	public static class TypeHelper
	{
		public static Type GetElementType(Type enumerableType)
		{
			if (enumerableType.HasElementType)
			{
				return enumerableType.GetElementType();
			}

			if (enumerableType.IsGenericType && enumerableType.GetGenericTypeDefinition().Equals(typeof (IEnumerable<>)))
			{
				return enumerableType.GetGenericArguments()[0];
			}

			Type ienumerableType = enumerableType.GetInterface("IEnumerable`1");
			if (ienumerableType != null)
			{
				return ienumerableType.GetGenericArguments()[0];
			}

			if (typeof (IEnumerable).IsAssignableFrom(enumerableType))
			{
				return typeof (object);
			}

			throw new ArgumentException(String.Format("Unable to find the element type for type '{0}'.", enumerableType), "enumerableType");
		}
	}
}