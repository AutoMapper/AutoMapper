using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Internal
{
    public static class TypeExtensions
    {
        public const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        public const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        public static void CheckIsDerivedFrom(this Type derivedType, Type baseType)
        {
            if (!baseType.IsAssignableFrom(derivedType) && !derivedType.IsGenericTypeDefinition && !baseType.IsGenericTypeDefinition)
            {
                throw new ArgumentOutOfRangeException(nameof(derivedType), $"{derivedType} is not derived from {baseType}.");
            }
        }

        public static bool IsDynamic(this Type type) => typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type);

        public static bool IsNonStringEnumerable(this Type type) => type != typeof(string) && type.IsEnumerableType();

        public static bool IsSetType(this Type type) => type.IsGenericType(typeof(ISet<>)) || type.IsGenericType(typeof(HashSet<>));

        public static IEnumerable<Type> BaseClassesAndInterfaces(this Type type)
        {
            var currentType = type;
            while (currentType != null)
            {
                yield return currentType;
                currentType = currentType.BaseType;
            }
            foreach (var interfaceType in type.GetInterfaces())
            {
                yield return interfaceType;
            }
        }

        public static PropertyInfo GetInheritedProperty(this Type type, string name) =>
            type.BaseClassesAndInterfaces().Select(t => t.GetProperty(name, InstanceFlags)).FirstOrDefault(p => p != null);

        public static FieldInfo GetInheritedField(this Type type, string name) =>
            type.BaseClassesAndInterfaces().Select(t => t.GetField(name, InstanceFlags)).FirstOrDefault(f => f != null);

        public static MethodInfo GetInheritedMethod(this Type type, string name) =>
            type.BaseClassesAndInterfaces().Select(t => t.GetMethod(name, InstanceFlags)).FirstOrDefault(m => m != null)
            ?? throw new ArgumentOutOfRangeException(nameof(name), $"Cannot find member {name} of type {type}.");

        public static MemberInfo GetFieldOrProperty(this Type type, string name)
            => type.GetInheritedProperty(name) ?? (MemberInfo)type.GetInheritedField(name) ?? throw new ArgumentOutOfRangeException(nameof(name), $"Cannot find member {name} of type {type}.");

        public static bool IsNullableType(this Type type) => type.IsGenericType(typeof(Nullable<>));

        public static bool IsCollectionType(this Type type) => type.ImplementsGenericInterface(typeof(ICollection<>));

        public static Type GetCollectionType(this Type type) => type.GetGenericInterface(typeof(ICollection<>));

        public static bool IsEnumerableType(this Type type) => typeof(IEnumerable).IsAssignableFrom(type);

        public static bool IsListType(this Type type) => typeof(IList).IsAssignableFrom(type);

        public static bool IsDictionaryType(this Type type) => type.GetDictionaryType() != null;

        public static bool IsReadOnlyDictionaryType(this Type type) => type.IsGenericType(typeof(IReadOnlyDictionary<,>)) || type.IsGenericType(typeof(ReadOnlyDictionary<,>));

        public static bool ImplementsGenericInterface(this Type type, Type interfaceType) => type.GetGenericInterface(interfaceType) != null;

        public static bool IsGenericType(this Type type, Type genericType) => type.IsGenericType && type.GetGenericTypeDefinition() == genericType;

        public static Type GetIEnumerableType(this Type type) => type.GetGenericInterface(typeof(IEnumerable<>));

        public static Type GetDictionaryType(this Type type) => type.GetGenericInterface(typeof(IDictionary<,>));

        public static Type GetReadOnlyDictionaryType(this Type type) => type.GetGenericInterface(typeof(IReadOnlyDictionary<,>));

        public static Type GetGenericInterface(this Type type, Type genericInterface)
        {
            if (type.IsGenericType(genericInterface))
            {
                return type;
            }
            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType(genericInterface))
                {
                    return interfaceType;
                }
            }
            return null;
        }

        public static Type GetTypeDefinitionIfGeneric(this Type type) => type.IsGenericType ? type.GetGenericTypeDefinition() : type;

        public static IEnumerable<ConstructorInfo> GetDeclaredConstructors(this Type type) => type.GetConstructors(InstanceFlags);

        public static Type[] GetGenericParameters(this Type type) => type.GetGenericTypeDefinition().GetTypeInfo().GenericTypeParameters;

        public static IEnumerable<Type> GetTypeInheritance(this Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }

        public static MethodInfo GetStaticMethod(this Type type, string name) => type.GetMethod(name, StaticFlags);

        public static IEnumerable<PropertyInfo> PropertiesWithAnInaccessibleSetter(this Type type) => type.GetRuntimeProperties().Where(pm => pm.HasAnInaccessibleSetter());

        /// <summary>
        /// if targetType is oldType, method will return newType
        /// if targetType is not oldType, method will return targetType
        /// if targetType is generic type with oldType arguments, method will replace all oldType arguments on newType
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="oldType"></param>
        /// <param name="newType"></param>
        /// <returns></returns>
        public static Type ReplaceItemType(this Type targetType, Type oldType, Type newType)
        {
            if (targetType == oldType)
                return newType;

            if (targetType.IsGenericType)
            {
                var genSubArgs = targetType.GetTypeInfo().GenericTypeArguments;
                var newGenSubArgs = new Type[genSubArgs.Length];
                for (var i = 0; i < genSubArgs.Length; i++)
                    newGenSubArgs[i] = ReplaceItemType(genSubArgs[i], oldType, newType);
                return targetType.GetGenericTypeDefinition().MakeGenericType(newGenSubArgs);
            }

            return targetType;
        }
    }
}