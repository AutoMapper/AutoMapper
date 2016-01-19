namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Internal;

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

            if (enumerableType.IsGenericType() &&
                enumerableType.GetGenericTypeDefinition() == typeof (IEnumerable<>))
            {
                return enumerableType.GetTypeInfo().GenericTypeArguments[0];
            }

            Type ienumerableType = GetIEnumerableType(enumerableType);
            if (ienumerableType != null)
            {
                return ienumerableType.GetTypeInfo().GenericTypeArguments[0];
            }

            if (typeof (IEnumerable).IsAssignableFrom(enumerableType))
            {
                var first = enumerable?.Cast<object>().FirstOrDefault();

                return first?.GetType() ?? typeof (object);
            }

            throw new ArgumentException($"Unable to find the element type for type '{enumerableType}'.", nameof(enumerableType));
        }

        public static Type GetEnumerationType(Type enumType)
        {
            if (enumType.IsNullableType())
            {
                enumType = enumType.GetTypeInfo().GenericTypeArguments[0];
            }

            if (!enumType.IsEnum())
                return null;

            return enumType;
        }

        private static Type GetIEnumerableType(Type enumerableType)
        {
            try
            {
                return enumerableType.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(t => t.Name == "IEnumerable`1");
            }
            catch (AmbiguousMatchException)
            {
                if (enumerableType.BaseType() != typeof (object))
                    return GetIEnumerableType(enumerableType.BaseType());

                return null;
            }
        }
    }
}