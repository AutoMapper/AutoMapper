using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Configuration.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PrimitiveHelper
    {
        public static TValue GetOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key)
        {
            dictionary.TryGetValue(key, out TValue value);
            return value;
        }

        private static IEnumerable<MemberInfo> GetAllMembers(this Type type) =>
            type.GetTypeInheritance().Concat(type.GetTypeInfo().ImplementedInterfaces).SelectMany(i => i.GetDeclaredMembers());

        public static MemberInfo GetInheritedMember(this Type type, string name) => type.GetAllMembers().FirstOrDefault(mi => mi.Name == name);

        public static MethodInfo GetInheritedMethod(Type type, string name)
            => type.GetInheritedMember(name) as MethodInfo ?? throw new ArgumentOutOfRangeException(nameof(name), $"Cannot find method {name} of type {type}.");

        public static MemberInfo GetFieldOrProperty(Type type, string name) 
            => type.GetInheritedMember(name) ?? throw new ArgumentOutOfRangeException(nameof(name), $"Cannot find member {name} of type {type}.");

        public static bool IsNullableType(Type type) 
            => type.IsGenericType(typeof(Nullable<>));

        public static Type GetTypeOfNullable(Type type) 
            => type.GetTypeInfo().GenericTypeArguments[0];

        public static bool IsCollectionType(Type type) 
            => type.ImplementsGenericInterface(typeof(ICollection<>));

        public static bool IsEnumerableType(Type type) 
            => typeof(IEnumerable).IsAssignableFrom(type);

        public static bool IsQueryableType(Type type) 
            => typeof(IQueryable).IsAssignableFrom(type);

        public static bool IsListType(Type type)
            => typeof(IList).IsAssignableFrom(type);

        public static bool IsDictionaryType(Type type) 
            => type.ImplementsGenericInterface(typeof(IDictionary<,>));

        public static bool IsReadOnlyDictionaryType(Type type) 
            => type.ImplementsGenericInterface(typeof(IReadOnlyDictionary<,>));

        public static bool ImplementsGenericInterface(Type type, Type interfaceType)
        {
            return type.IsGenericType(interfaceType)
                   || type.GetTypeInfo().ImplementedInterfaces.Any(@interface => @interface.IsGenericType(interfaceType));
        }

        public static bool IsGenericType(Type type, Type genericType)
            => type.IsGenericType && type.GetGenericTypeDefinition() == genericType;

        public static Type GetIEnumerableType(Type type)
            => type.GetGenericInterface(typeof(IEnumerable<>));

        public static Type GetDictionaryType(Type type)
            => type.GetGenericInterface(typeof(IDictionary<,>));

        public static Type GetReadOnlyDictionaryType(Type type)
            => type.GetGenericInterface(typeof(IReadOnlyDictionary<,>));

        public static Type GetGenericInterface(Type type, Type genericInterface)
        {
            return type.IsGenericType(genericInterface)
                ? type
                : type.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(t => t.IsGenericType(genericInterface));
        }

        public static Type GetGenericElementType(Type type)
            => type.HasElementType ? type.GetElementType() : type.GetTypeInfo().GenericTypeArguments[0];
    }
}