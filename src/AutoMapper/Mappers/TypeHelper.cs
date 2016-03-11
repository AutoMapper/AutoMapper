using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Mappers
{
    public static class TypeHelper
    {
        private static ConcurrentDictionary<Type, Type> _elementTypes = new ConcurrentDictionary<Type, Type>();
        private static Func<Type, Type> _findElementType = FindElementType;

        public static Type GetElementType(Type enumerableType)
        {
            if(enumerableType.HasElementType)
            {
                return enumerableType.GetElementType();
            }
            return _elementTypes.GetOrAdd(enumerableType, _findElementType);
        }

        private static Type FindElementType(Type enumerableType)
        {
            return GetIEnumerableType(enumerableType)?.GenericTypeArguments[0] ?? typeof(object);
        }

        public static bool IsGenericIEnumerable(this Type type)
        {
            return type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        public static Type GetEnumerationType(Type enumType)
        {
            enumType = Nullable.GetUnderlyingType(enumType) ?? enumType;
            return enumType.IsEnum() ? enumType : null;
        }

        private static Type GetIEnumerableType(Type enumerableType)
        {
            return new[] { enumerableType }.Concat(enumerableType.GetTypeInfo().ImplementedInterfaces)
                    .FirstOrDefault(IsGenericIEnumerable);
        }
    }
}