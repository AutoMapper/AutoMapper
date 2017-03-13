using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Configuration
{
    internal static class PrimitiveExtensions
    {
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            dictionary.TryGetValue(key, out TValue value);
            return value;
        }

        public static MethodInfo GetInheritedMethod(this Type type, string name) => GetMember(type, name) as MethodInfo;

        public static MemberInfo GetFieldOrProperty(this Type type, string name)
        {
            var memberInfo = GetMember(type, name);
            if(memberInfo == null)
            {
                throw new ArgumentOutOfRangeException(nameof(name), "Cannot find a field or property named " + name);
            }
            return memberInfo;
        }

        private static MemberInfo GetMember(Type type, string name)
        {
            return 
                new[] { type }.Concat(type.GetTypeInfo().ImplementedInterfaces)
                .SelectMany(i => i.GetMember(name))
                .FirstOrDefault();
        }

        public static bool IsNullableType(this Type type) => type.IsGenericType(typeof(Nullable<>));

        public static Type GetTypeOfNullable(this Type type) => type.GetTypeInfo().GenericTypeArguments[0];

        public static bool IsCollectionType(this Type type) => type.ImplementsGenericInterface(typeof(ICollection<>));

        public static bool IsEnumerableType(this Type type) => typeof(IEnumerable).IsAssignableFrom(type);

        public static bool IsQueryableType(this Type type) => typeof(IQueryable).IsAssignableFrom(type);

        public static bool IsListType(this Type type) => typeof(IList).IsAssignableFrom(type);

        public static bool IsListOrDictionaryType(this Type type) => type.IsListType() || type.IsDictionaryType();

        public static bool IsDictionaryType(this Type type) => type.ImplementsGenericInterface(typeof(IDictionary<,>));

        public static bool ImplementsGenericInterface(this Type type, Type interfaceType)
        {
            return type.IsGenericType(interfaceType) 
                || type.GetTypeInfo().ImplementedInterfaces.Any(@interface => @interface.IsGenericType(interfaceType));
        }

        public static bool IsGenericType(this Type type, Type genericType) => type.IsGenericType() && type.GetGenericTypeDefinition() == genericType;

        public static Type GetIEnumerableType(this Type type) => type.GetGenericInterface(typeof(IEnumerable<>));

        public static Type GetDictionaryType(this Type type) => type.GetGenericInterface(typeof(IDictionary<,>));

        public static Type GetGenericInterface(this Type type, Type genericInterface)
        {
            return type.IsGenericType(genericInterface) 
                ? type 
                : type.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(t=>t.IsGenericType(genericInterface));
        }

        public static Type GetGenericElementType(this Type type) 
            => type.HasElementType ? type.GetElementType() : type.GetTypeInfo().GenericTypeArguments[0];
    }
}