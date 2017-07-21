using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper.Configuration;

namespace AutoMapper.Mappers.Internal
{
    public static class ElementTypeHelper
    {
        public static Type GetElementType(Type enumerableType) => GetElementTypes(enumerableType, null)[0];

        public static Type[] GetElementTypes(Type enumerableType, ElementTypeFlags flags = ElementTypeFlags.None) => 
            GetElementTypes(enumerableType, null, flags);

        public static Type GetElementType(Type enumerableType, IEnumerable enumerable) => GetElementTypes(enumerableType, enumerable)[0];

        public static Type[] GetElementTypes(Type enumerableType, IEnumerable enumerable,
            ElementTypeFlags flags = ElementTypeFlags.None)
        {
            if (enumerableType.HasElementType)
            {
                return new[] {enumerableType.GetElementType()};
            }

            var idictionaryType = enumerableType.GetDictionaryType();
            if (idictionaryType != null && flags.HasFlag(ElementTypeFlags.BreakKeyValuePair))
            {
                return idictionaryType.GetTypeInfo().GenericTypeArguments;
            }

            var ienumerableType = enumerableType.GetIEnumerableType();
            if (ienumerableType != null)
            {
                return ienumerableType.GetTypeInfo().GenericTypeArguments;
            }

            if (typeof(IEnumerable).IsAssignableFrom(enumerableType))
            {
                var first = enumerable?.Cast<object>().FirstOrDefault();

                return new[] {first?.GetType() ?? typeof(object)};
            }

            throw new ArgumentException($"Unable to find the element type for type '{enumerableType}'.",
                nameof(enumerableType));
        }

        public static Type GetEnumerationType(Type enumType)
        {
            return !enumType.IsEnum() ? null : enumType;
        }

        internal static IEnumerable<MethodInfo> GetStaticMethods(this Type type)
        {
            return type.GetRuntimeMethods().Where(m => m.IsStatic);
        }
    }

    public enum ElementTypeFlags
    {
        None = 0,
        BreakKeyValuePair = 1
    }
}