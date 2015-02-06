namespace AutoMapper.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class TypeExtensions
    {
        public static IEnumerable<ConstructorInfo> GetDeclaredConstructors(this Type type)
        {
#if NETFX_CORE || MONODROID || MONOTOUCH || __IOS__ || ASPNET50 || ASPNETCORE50
            return type.GetTypeInfo().DeclaredConstructors;
#else
            return type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
#endif
        }

        public static IEnumerable<MemberInfo> GetDeclaredMembers(this Type type)
        {
#if NETFX_CORE || MONODROID || MONOTOUCH || __IOS__ || ASPNET50 || ASPNETCORE50
            return type.GetTypeInfo().DeclaredMembers;
#else
            return type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
#endif
        }

        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type)
        {
#if NETFX_CORE || MONODROID || MONOTOUCH || __IOS__ || ASPNET50 || ASPNETCORE50
            return type.GetTypeInfo().DeclaredMethods;
#else
            return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
#endif
        }

        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type type)
        {
#if NETFX_CORE || MONODROID || MONOTOUCH || __IOS__ || ASPNET50 || ASPNETCORE50
            return type.GetTypeInfo().DeclaredProperties;
#else
            return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
#endif
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
#if ASPNETCORE50 || NETFX_CORE
            return type.GetTypeInfo().Assembly;
#else
            return type.Assembly;
#endif
        }

        public static Type BaseType(this Type type)
        {
#if ASPNETCORE50 || NETFX_CORE
            return type.GetTypeInfo().BaseType;
#else
            return type.BaseType;
#endif
        }

#if ASPNETCORE50 || NETFX_CORE
        public static object[] GetCustomAttributes(this Type type, Type attributeType, bool inherit)
        {
            return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray();
        }
#endif

        public static bool IsAbstract(this Type type)
        {
#if ASPNETCORE50 || NETFX_CORE
            return type.GetTypeInfo().IsAbstract;
#else
            return type.IsAbstract;
#endif
        }

        public static bool IsClass(this Type type)
        {
#if ASPNETCORE50 || NETFX_CORE
            return type.GetTypeInfo().IsClass;
#else
            return type.IsClass;
#endif
        }

        public static bool IsEnum(this Type type)
        {
#if ASPNETCORE50 || NETFX_CORE
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        public static bool IsGenericType(this Type type)
        {
#if ASPNETCORE50 || NETFX_CORE
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        public static bool IsGenericTypeDefinition(this Type type)
        {
#if ASPNETCORE50 || NETFX_CORE
            return type.GetTypeInfo().IsGenericTypeDefinition;
#else
            return type.IsGenericTypeDefinition;
#endif
        }

        public static bool IsInterface(this Type type)
        {
#if ASPNETCORE50 || NETFX_CORE
            return type.GetTypeInfo().IsInterface;
#else
            return type.IsInterface;
#endif
        }

        public static bool IsPrimitive(this Type type)
        {
#if ASPNETCORE50 || NETFX_CORE
            return type.GetTypeInfo().IsPrimitive;
#else
            return type.IsPrimitive;
#endif
        }

        public static bool IsSealed(this Type type)
        {
#if ASPNETCORE50 || NETFX_CORE
            return type.GetTypeInfo().IsSealed;
#else
            return type.IsSealed;
#endif
        }

        public static bool IsValueType(this Type type)
        {
#if ASPNETCORE50 || NETFX_CORE
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }


#if NETFX_CORE
        public static MethodInfo GetGetMethod(this PropertyInfo propertyInfo, bool ignored)
        {
            return propertyInfo.SetMethod;
        }

        public static MethodInfo[] GetAccessors(this PropertyInfo propertyInfo)
        {
            return new[] { propertyInfo.GetMethod, propertyInfo.SetMethod };
        }

        public static MethodInfo GetSetMethod(this PropertyInfo propertyInfo, bool ignored)
        {
            return propertyInfo.SetMethod;
        }

        public static Type[] GetTypes(this Assembly assembly)
        {
            return assembly.ExportedTypes.ToArray();
        }

        public static PropertyInfo GetProperty(this Type type, string name)
        {
            return type.GetTypeInfo().GetDeclaredProperty(name);
        }

        public static MethodInfo GetMethod(this Type type, string name)
        {
            return type.GetTypeInfo().GetDeclaredMethod(name);
        }

        public static MethodInfo GetMethod(this Type type, string name, Type[] parameterTypes)
        {
            return type.GetTypeInfo().GetDeclaredMethods(name).FirstOrDefault(mi => mi.GetParameters().Select(pi => pi.ParameterType).ToArray().SequenceEqual(parameterTypes));
        }

        public static FieldInfo GetField(this Type type, string name)
        {
            return type.GetTypeInfo().GetDeclaredField(name);
        }

        public static MemberInfo[] GetMember(this Type type, string name)
        {
            return type.GetTypeInfo().DeclaredMembers
                .Where(m => m.Name == name).ToArray()
            ;
        }

        public static bool IsInstanceOfType(this Type type, object o)
        {
            return o != null && type.IsAssignableFrom(o.GetType());
        }

        public static bool IsAssignableFrom(this Type type, Type other)
        {
            return type.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo());
        }

        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments;
        }

        public static ConstructorInfo[] GetConstructors(this Type type)
        {
            return type.GetTypeInfo().DeclaredConstructors.ToArray();
        }


        public static Type[] GetInterfaces(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces.ToArray();
        }

#endif
    }
}