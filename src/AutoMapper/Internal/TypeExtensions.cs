namespace AutoMapper.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
#if !PORTABLE
    using System.Reflection.Emit;
#endif

    internal static class TypeExtensions
    {

        public static Type[] GetGenericParameters(this Type type)
        {
            return type.GetGenericTypeDefinition().GetTypeInfo().GenericTypeParameters;
        }

        public static IEnumerable<ConstructorInfo> GetDeclaredConstructors(this Type type)
        {
            return type.GetTypeInfo().DeclaredConstructors;
        }

#if !PORTABLE
        public static Type CreateType(this TypeBuilder type)
        {
            return type.CreateTypeInfo().AsType();
        }
#endif

        public static IEnumerable<MemberInfo> GetDeclaredMembers(this Type type)
        {
            return type.GetTypeInfo().DeclaredMembers;
        }

#if PORTABLE
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
#endif

        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type)
        {
            return type.GetTypeInfo().DeclaredMethods;
        }

#if PORTABLE
        public static MethodInfo GetMethod(this Type type, string name)
        {
            return type.GetAllMethods().FirstOrDefault(mi => mi.Name == name);
        }

        public static MethodInfo GetMethod(this Type type, string name, Type[] parameters)
        {
            //a.Length == b.Length && a.Intersect(b).Count() == a.Length
            return type.GetAllMethods()
                .Where(mi => mi.Name == name)
                .Where(mi => mi.GetParameters().Length == parameters.Length)
                .Where(mi => mi.GetParameters().Select(pi => pi.ParameterType).Intersect(parameters).Count() == parameters.Length)
                .FirstOrDefault();
        }
#endif

        public static IEnumerable<MethodInfo> GetAllMethods(this Type type)
        {
            return type.GetRuntimeMethods();
        }

        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type type)
        {
            return type.GetTypeInfo().DeclaredProperties;
        }

#if PORTABLE
        public static PropertyInfo GetProperty(this Type type, string name)
        {
            return type.GetTypeInfo().DeclaredProperties.FirstOrDefault(mi => mi.Name == name);
        }
#endif

        public static object[] GetCustomAttributes(this Type type, Type attributeType, bool inherit)
        {
            return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray();
        }

        public static bool IsStatic(this FieldInfo fieldInfo)
        {
            return fieldInfo?.IsStatic ?? false;
        }

        public static bool IsStatic(this PropertyInfo propertyInfo)
        {
            return propertyInfo?.GetGetMethod(true)?.IsStatic
                ?? propertyInfo?.GetSetMethod(true)?.IsStatic
                ?? false;
        }

        public static bool IsStatic(this MemberInfo memberInfo)
        {
            return (memberInfo as FieldInfo).IsStatic() || (memberInfo as PropertyInfo).IsStatic();
        }

        public static bool IsPublic(this PropertyInfo propertyInfo)
        {
            return (propertyInfo?.GetGetMethod(true)?.IsPublic ?? false)
                || (propertyInfo?.GetSetMethod(true)?.IsPublic ?? false);
        }

        public static bool IsPublic(this MemberInfo memberInfo)
        {
            return (memberInfo as FieldInfo)?.IsPublic ?? (memberInfo as PropertyInfo).IsPublic();
        }

        public static bool IsNotPublic(this ConstructorInfo constructorInfo)
        {
            return constructorInfo.IsPrivate
                   || constructorInfo.IsFamilyAndAssembly
                   || constructorInfo.IsFamilyOrAssembly
                   || constructorInfo.IsFamily;
        }

        public static Assembly Assembly(this Type type)
        {
            return type.GetTypeInfo().Assembly;
        }

        public static Type BaseType(this Type type)
        {
            return type.GetTypeInfo().BaseType;
        }

#if PORTABLE
        public static bool IsAssignableFrom(this Type type, Type other)
        {
            return type.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo());
        }
#endif

        public static bool IsAbstract(this Type type)
        {
            return type.GetTypeInfo().IsAbstract;
        }

        public static bool IsClass(this Type type)
        {
            return type.GetTypeInfo().IsClass;
        }

        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static bool IsGenericTypeDefinition(this Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition;
        }

        public static bool IsInterface(this Type type)
        {
            return type.GetTypeInfo().IsInterface;
        }

        public static bool IsPrimitive(this Type type)
        {
            return type.GetTypeInfo().IsPrimitive;
        }

        public static bool IsSealed(this Type type)
        {
            return type.GetTypeInfo().IsSealed;
        }

        public static bool IsValueType(this Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        public static bool IsInstanceOfType(this Type type, object o)
        {
            return o != null && type.IsAssignableFrom(o.GetType());
        }

        public static ConstructorInfo[] GetConstructors(this Type type)
        {
            return type.GetTypeInfo().DeclaredConstructors.ToArray();
        }

        public static MethodInfo GetGetMethod(this PropertyInfo propertyInfo, bool ignored)
        {
            return propertyInfo.GetMethod;
        }

        public static MethodInfo GetSetMethod(this PropertyInfo propertyInfo, bool ignored)
        {
            return propertyInfo.SetMethod;
        }

        public static FieldInfo GetField(this Type type, string name)
        {
            return type.GetRuntimeField(name);
        }
    }
}
