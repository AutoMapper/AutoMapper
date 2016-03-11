using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Mappers
{
    public static class TypeHelper
    {
        private static ConcurrentDictionary<Type, IObjectMapper> _enumerableMappers = new ConcurrentDictionary<Type, IObjectMapper>();
        private static ConcurrentDictionary<Type, Type> _elementTypes = new ConcurrentDictionary<Type, Type>();
        private static readonly Func<Type, Type> _findElementType = FindElementType;
        private static readonly Func<Type, IObjectMapper> _createMapper = CreateMapper;

        public static IObjectMapper GetEnumerableMapper(Type enumerableType)
        {
            return _enumerableMappers.GetOrAdd(enumerableType, _createMapper);
        }

        public static Type GetElementType(Type enumerableType)
        {
            if(enumerableType.HasElementType)
            {
                return enumerableType.GetElementType();
            }
            return _elementTypes.GetOrAdd(enumerableType, _findElementType);
        }

        private static IObjectMapper CreateMapper(Type enumerableType)
        {
            var elementType = GetElementType(enumerableType);
            var enumerableMapperType = typeof(EnumerableMapper<,>).MakeGenericType(enumerableType, elementType);
            return (IObjectMapper)Activator.CreateInstance(enumerableMapperType);
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