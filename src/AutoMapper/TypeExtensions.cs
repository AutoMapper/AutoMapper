using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper
{
#if NET45 || NET40
    using System.Reflection.Emit;
#endif

    internal static class TypeExtensions
    {
        public static bool Has<TAttribute>(this Type type) where TAttribute : Attribute => type.GetTypeInfo().IsDefined(typeof(TAttribute), inherit: false);

        public static Type GetGenericTypeDefinitionIfGeneric(this Type type) => type.IsGenericType() ? type.GetGenericTypeDefinition() : type;

        public static Type[] GetGenericArguments(this Type type) => type.GetTypeInfo().GenericTypeArguments;

        public static Type[] GetGenericParameters(this Type type) => type.GetGenericTypeDefinition().GetTypeInfo().GenericTypeParameters;

        public static IEnumerable<ConstructorInfo> GetDeclaredConstructors(this Type type) => type.GetTypeInfo().DeclaredConstructors;

#if NET45 || NET40
        public static Type CreateType(this TypeBuilder type)
        {
#if NET40
            return type.CreateType();
#else
            return type.CreateTypeInfo().AsType();
#endif
        }
#endif

        public static IEnumerable<MemberInfo> GetDeclaredMembers(this Type type) => type.GetTypeInfo().DeclaredMembers;

        public static IEnumerable<MemberInfo> GetAllMembers(this Type type)
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

        public static MemberInfo[] GetMember(this Type type, string name)
        {
            return type.GetAllMembers().Where(mi => mi.Name == name).ToArray();
        }

        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type) => type.GetTypeInfo().DeclaredMethods;

        public static MethodInfo GetDeclaredMethod(this Type type, string name)
        {
            return type.GetAllMethods().FirstOrDefault(mi => mi.Name == name);
        }

        public static MethodInfo GetDeclaredMethod(this Type type, string name, Type[] parameters)
        {
            return type
                .GetAllMethods()
                .Where(mi => mi.Name == name)
                .Where(mi => mi.GetParameters().Length == parameters.Length)
                .FirstOrDefault(mi => mi.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(parameters));
        }

        public static ConstructorInfo GetDeclaredConstructor(this Type type, Type[] parameters)
        {
            return type
                .GetTypeInfo()
                .DeclaredConstructors
                .Where(mi => mi.GetParameters().Length == parameters.Length)
                .FirstOrDefault(mi => mi.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(parameters));
        }

        public static IEnumerable<MethodInfo> GetAllMethods(this Type type) => type.GetRuntimeMethods();

        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type type) => type.GetTypeInfo().DeclaredProperties;

        public static PropertyInfo GetDeclaredProperty(this Type type, string name) 
            => type.GetTypeInfo().GetDeclaredProperty(name);

        public static object[] GetCustomAttributes(this Type type, Type attributeType, bool inherit) 
            => type.GetTypeInfo().GetCustomAttributes(attributeType, inherit).Cast<object>().ToArray();

        public static bool IsStatic(this FieldInfo fieldInfo) => fieldInfo?.IsStatic ?? false;

        public static bool IsStatic(this PropertyInfo propertyInfo) => propertyInfo?.GetGetMethod(true)?.IsStatic
                                                                       ?? propertyInfo?.GetSetMethod(true)?.IsStatic
                                                                       ?? false;

        public static bool IsStatic(this MemberInfo memberInfo) => (memberInfo as FieldInfo).IsStatic() 
                                                                   || (memberInfo as PropertyInfo).IsStatic()
                                                                   || ((memberInfo as MethodInfo)?.IsStatic
                                                                       ?? false);

        public static bool IsPublic(this PropertyInfo propertyInfo) => (propertyInfo?.GetGetMethod(true)?.IsPublic ?? false)
                                                                       || (propertyInfo?.GetSetMethod(true)?.IsPublic ?? false);

        public static IEnumerable<PropertyInfo> PropertiesWithAnInaccessibleSetter(this Type type)
        {
            return type.GetDeclaredProperties().Where(pm => pm.HasAnInaccessibleSetter());
        }

        public static bool HasAnInaccessibleSetter(this PropertyInfo property)
        {
            var setMethod = property.GetSetMethod(true);
            return setMethod == null || setMethod.IsPrivate || setMethod.IsFamily;
        }

        public static bool IsPublic(this MemberInfo memberInfo) => (memberInfo as FieldInfo)?.IsPublic ?? (memberInfo as PropertyInfo).IsPublic();

        public static bool IsNotPublic(this ConstructorInfo constructorInfo) => constructorInfo.IsPrivate
                                                                                || constructorInfo.IsFamilyAndAssembly
                                                                                || constructorInfo.IsFamilyOrAssembly
                                                                                || constructorInfo.IsFamily;

        public static Assembly Assembly(this Type type) => type.GetTypeInfo().Assembly;

        public static Type BaseType(this Type type) => type.GetTypeInfo().BaseType;

        public static bool IsAssignableFrom(this Type type, Type other) => type.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo());

        public static bool IsAbstract(this Type type) => type.GetTypeInfo().IsAbstract;

        public static bool IsClass(this Type type) => type.GetTypeInfo().IsClass;

        public static bool IsEnum(this Type type) => type.GetTypeInfo().IsEnum;

        public static bool IsGenericType(this Type type) => type.GetTypeInfo().IsGenericType;

        public static bool IsGenericTypeDefinition(this Type type) => type.GetTypeInfo().IsGenericTypeDefinition;

        public static bool IsInterface(this Type type) => type.GetTypeInfo().IsInterface;

        public static bool IsPrimitive(this Type type) => type.GetTypeInfo().IsPrimitive;

        public static bool IsSealed(this Type type) => type.GetTypeInfo().IsSealed;

        public static bool IsValueType(this Type type) => type.GetTypeInfo().IsValueType;

        public static bool IsInstanceOfType(this Type type, object o) => o != null && type.GetTypeInfo().IsAssignableFrom(o.GetType().GetTypeInfo());

        public static ConstructorInfo[] GetConstructors(this Type type) => type.GetTypeInfo().DeclaredConstructors.ToArray();

        public static PropertyInfo[] GetProperties(this Type type) => type.GetRuntimeProperties().ToArray();

#if NET40
        public static MethodInfo GetGetMethod(this PropertyInfo propertyInfo, bool ignored) => propertyInfo.GetGetMethod();

        public static MethodInfo GetSetMethod(this PropertyInfo propertyInfo, bool ignored) => propertyInfo.GetSetMethod();
#else
        public static MethodInfo GetGetMethod(this PropertyInfo propertyInfo, bool ignored) => propertyInfo.GetMethod;

        public static MethodInfo GetSetMethod(this PropertyInfo propertyInfo, bool ignored) => propertyInfo.SetMethod;
        
        public static MethodInfo GetGetMethod(this PropertyInfo propertyInfo) => propertyInfo.GetMethod;

        public static MethodInfo GetSetMethod(this PropertyInfo propertyInfo) => propertyInfo.SetMethod;
#endif

        public static FieldInfo GetField(this Type type, string name) => type.GetRuntimeField(name);
    }
}
