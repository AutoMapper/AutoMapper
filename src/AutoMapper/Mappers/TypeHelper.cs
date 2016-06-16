namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Configuration;

    public static class TypeHelper
    {
        public static Type GetElementType(Type enumerableType)
        {
            return GetElementTypes(enumerableType, null)[0];
        }
        public static Type[] GetElementTypes(Type enumerableType, ElemntTypeFlags flags = ElemntTypeFlags.None)
        {
            return GetElementTypes(enumerableType, null, flags);
        }

        public static Type GetElementType(Type enumerableType, IEnumerable enumerable)
        {
            return GetElementTypes(enumerableType, enumerable)[0];
        }
        public static Type[] GetElementTypes(Type enumerableType, IEnumerable enumerable, ElemntTypeFlags flags = ElemntTypeFlags.None)
        {
            if (enumerableType.HasElementType)
            {
                return new []{enumerableType.GetElementType()};
            }

            if (flags.HasFlag(ElemntTypeFlags.BreakKeyValuePair) && enumerableType.IsGenericType() && enumerableType.IsDictionaryType())
            {
                return enumerableType.GetTypeInfo().GenericTypeArguments;
            }

            if (enumerableType.IsGenericType() &&
                enumerableType.GetGenericTypeDefinition() == typeof (IEnumerable<>))
            {
                return enumerableType.GetTypeInfo().GenericTypeArguments;
            }

            Type ienumerableType = GetIEnumerableType(enumerableType);
            if (ienumerableType != null)
            {
                return ienumerableType.GetTypeInfo().GenericTypeArguments;
            }

            if (typeof (IEnumerable).IsAssignableFrom(enumerableType))
            {
                var first = enumerable?.Cast<object>().FirstOrDefault();

                return new []{first?.GetType() ?? typeof (object)};
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

    public enum ElemntTypeFlags
    {
        None = 0,
        BreakKeyValuePair = 1
    }
}