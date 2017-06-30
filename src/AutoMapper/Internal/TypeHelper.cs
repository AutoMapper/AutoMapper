using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Internal
{
#if NET45 || NET40
    using System.Reflection.Emit;
#endif

    public static class TypeHelper
    {
        public static bool Has<TAttribute>(Type type) where TAttribute : Attribute => type.GetTypeInfo().IsDefined(typeof(TAttribute), inherit: false);

        public static Type GetGenericTypeDefinitionIfGeneric(Type type) => type.IsGenericType() ? type.GetGenericTypeDefinition() : type;

        public static Type[] GetGenericArguments(Type type) => type.GetTypeInfo().GenericTypeArguments;

        public static Type[] GetGenericParameters(Type type) => type.GetGenericTypeDefinition().GetTypeInfo().GenericTypeParameters;

        public static IEnumerable<ConstructorInfo> GetDeclaredConstructors(Type type) => type.GetTypeInfo().DeclaredConstructors;

#if NET45 || NET40
        public static Type CreateType(TypeBuilder type)
        {
#if NET40
            return type.CreateType();
#else
            return type.CreateTypeInfo().AsType();
#endif
        }
#endif

        public static IEnumerable<MemberInfo> GetDeclaredMembers(Type type) => type.GetTypeInfo().DeclaredMembers;

        public static IEnumerable<MemberInfo> GetAllMembers(Type type)
        {
            while (true)
            {
                foreach (var memberInfo in type.GetTypeInfo().DeclaredMembers)
                {
                    yield return memberInfo;
                }

                type = type.BaseType();

                if (type == null)
                {
                    yield break;
                }
            }
        }

        public static MemberInfo[] GetMember(Type type, string name)
        {
            return type.GetAllMembers().Where(mi => mi.Name == name).ToArray();
        }

        public static IEnumerable<MethodInfo> GetDeclaredMethods(Type type) => type.GetTypeInfo().DeclaredMethods;

        public static MethodInfo GetDeclaredMethod(Type type, string name)
        {
            return type.GetAllMethods().FirstOrDefault(mi => mi.Name == name);
        }

        public static MethodInfo GetDeclaredMethod(Type type, string name, Type[] parameters)
        {
            return type
                .GetAllMethods()
                .Where(mi => mi.Name == name)
                .Where(mi => mi.GetParameters().Length == parameters.Length)
                .FirstOrDefault(mi => mi.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(parameters));
        }

        public static ConstructorInfo GetDeclaredConstructor(Type type, Type[] parameters)
        {
            return type
                .GetTypeInfo()
                .DeclaredConstructors
                .Where(mi => mi.GetParameters().Length == parameters.Length)
                .FirstOrDefault(mi => mi.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(parameters));
        }

        public static IEnumerable<MethodInfo> GetAllMethods(Type type) => type.GetRuntimeMethods();

        public static IEnumerable<PropertyInfo> GetDeclaredProperties(Type type) => type.GetTypeInfo().DeclaredProperties;

        public static PropertyInfo GetDeclaredProperty(Type type, string name)
            => type.GetTypeInfo().GetDeclaredProperty(name);

        public static object[] GetCustomAttributes(Type type, Type attributeType, bool inherit)
            => type.GetTypeInfo().GetCustomAttributes(attributeType, inherit).Cast<object>().ToArray();

        public static bool IsStatic(FieldInfo fieldInfo) => fieldInfo?.IsStatic ?? false;

        public static bool IsStatic(PropertyInfo propertyInfo) => propertyInfo?.GetGetMethod(true)?.IsStatic
                                                                       ?? propertyInfo?.GetSetMethod(true)?.IsStatic
                                                                       ?? false;

        public static bool IsStatic(MemberInfo memberInfo) => (memberInfo as FieldInfo).IsStatic()
                                                                   || (memberInfo as PropertyInfo).IsStatic()
                                                                   || ((memberInfo as MethodInfo)?.IsStatic
                                                                       ?? false);

        public static bool IsPublic(PropertyInfo propertyInfo) => (propertyInfo?.GetGetMethod(true)?.IsPublic ?? false)
                                                                       || (propertyInfo?.GetSetMethod(true)?.IsPublic ?? false);

        public static IEnumerable<PropertyInfo> PropertiesWithAnInaccessibleSetter(Type type)
        {
            return type.GetDeclaredProperties().Where(pm => pm.HasAnInaccessibleSetter());
        }

        public static bool HasAnInaccessibleSetter(PropertyInfo property)
        {
            var setMethod = property.GetSetMethod(true);
            return setMethod == null || setMethod.IsPrivate || setMethod.IsFamily;
        }

        public static bool IsPublic(MemberInfo memberInfo) => (memberInfo as FieldInfo)?.IsPublic ?? (memberInfo as PropertyInfo).IsPublic();

        public static bool IsNotPublic(ConstructorInfo constructorInfo) => constructorInfo.IsPrivate
                                                                                || constructorInfo.IsFamilyAndAssembly
                                                                                || constructorInfo.IsFamilyOrAssembly
                                                                                || constructorInfo.IsFamily;

        public static Assembly Assembly(Type type) => type.GetTypeInfo().Assembly;

        public static Type BaseType(Type type) => type.GetTypeInfo().BaseType;

        public static bool IsAssignableFrom(Type type, Type other) => type.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo());

        public static bool IsAbstract(Type type) => type.GetTypeInfo().IsAbstract;

        public static bool IsClass(Type type) => type.GetTypeInfo().IsClass;

        public static bool IsEnum(Type type) => type.GetTypeInfo().IsEnum;

        public static bool IsGenericType(Type type) => type.GetTypeInfo().IsGenericType;

        public static bool IsGenericTypeDefinition(Type type) => type.GetTypeInfo().IsGenericTypeDefinition;

        public static bool IsInterface(Type type) => type.GetTypeInfo().IsInterface;

        public static bool IsPrimitive(Type type) => type.GetTypeInfo().IsPrimitive;

        public static bool IsSealed(Type type) => type.GetTypeInfo().IsSealed;

        public static bool IsValueType(Type type) => type.GetTypeInfo().IsValueType;

        public static bool IsInstanceOfType(Type type, object o) => o != null && type.GetTypeInfo().IsAssignableFrom(o.GetType().GetTypeInfo());

        public static ConstructorInfo[] GetConstructors(Type type) => type.GetTypeInfo().DeclaredConstructors.ToArray();

        public static PropertyInfo[] GetProperties(Type type) => type.GetRuntimeProperties().ToArray();

#if NET40
        public static MethodInfo GetGetMethod(PropertyInfo propertyInfo, bool ignored) => propertyInfo.GetGetMethod();

        public static MethodInfo GetSetMethod(PropertyInfo propertyInfo, bool ignored) => propertyInfo.GetSetMethod();
#else
        public static MethodInfo GetGetMethod(PropertyInfo propertyInfo, bool ignored) => propertyInfo.GetMethod;

        public static MethodInfo GetSetMethod(PropertyInfo propertyInfo, bool ignored) => propertyInfo.SetMethod;
#endif

        public static FieldInfo GetField(Type type, string name) => type.GetRuntimeField(name);
    }
}