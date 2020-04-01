using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMapper.Configuration.Internal;

namespace AutoMapper.Configuration
{
    internal static class PrimitiveExtensions
    {
        public static void ForAll<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var feature in enumerable)
            {
                action(feature);
            }
        }

        public static bool IsNonStringEnumerable(this Type type) => type != typeof(string) && type.IsEnumerableType();

        public static bool IsSetType(this Type type)
            => type.ImplementsGenericInterface(typeof(ISet<>));

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            => PrimitiveHelper.GetOrDefault(dictionary, key);

        public static MethodInfo GetInheritedMethod(this Type type, string name)
            => PrimitiveHelper.GetInheritedMethod(type, name);

        public static MemberInfo GetFieldOrProperty(this Type type, string name)
            => PrimitiveHelper.GetFieldOrProperty(type, name);

        public static bool IsNullableType(this Type type)
            => PrimitiveHelper.IsNullableType(type);

        public static Type GetTypeOfNullable(this Type type)
            => PrimitiveHelper.GetTypeOfNullable(type);

        public static bool IsCollectionType(this Type type)
            => PrimitiveHelper.IsCollectionType(type);

        public static bool IsEnumerableType(this Type type)
            => PrimitiveHelper.IsEnumerableType(type);

        public static bool IsQueryableType(this Type type)
            => PrimitiveHelper.IsQueryableType(type);

        public static bool IsListType(this Type type)
            => PrimitiveHelper.IsListType(type);

        public static bool IsDictionaryType(this Type type)
            => PrimitiveHelper.IsDictionaryType(type);

        public static bool IsReadOnlyDictionaryType(this Type type)
            => PrimitiveHelper.IsReadOnlyDictionaryType(type);

        public static bool ImplementsGenericInterface(this Type type, Type interfaceType)
            => PrimitiveHelper.ImplementsGenericInterface(type, interfaceType);

        public static bool IsGenericType(this Type type, Type genericType)
            => PrimitiveHelper.IsGenericType(type, genericType);

        public static Type GetIEnumerableType(this Type type)
            => PrimitiveHelper.GetIEnumerableType(type);

        public static Type GetDictionaryType(this Type type)
            => PrimitiveHelper.GetDictionaryType(type);

        public static Type GetReadOnlyDictionaryType(this Type type)
            => PrimitiveHelper.GetReadOnlyDictionaryType(type);

        public static Type GetGenericInterface(this Type type, Type genericInterface)
            => PrimitiveHelper.GetGenericInterface(type, genericInterface);

        public static Type GetGenericElementType(this Type type)
            => PrimitiveHelper.GetGenericElementType(type);
    }
}