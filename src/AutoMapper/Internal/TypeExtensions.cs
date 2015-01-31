namespace AutoMapper.Internal
{
    using System;
    using System.Linq;
    using System.Reflection;

    internal static class TypeExtensions
    {
        public static Assembly Assembly(this Type type)
        {
#if ASPNETCORE50
            return type.GetTypeInfo().Assembly;
#else
            return type.Assembly;
#endif
        }

        public static Type BaseType(this Type type)
        {
#if ASPNETCORE50
            return type.GetTypeInfo().BaseType;
#else
            return type.BaseType;
#endif
        }

        public static object[] GetCustomAttributes(this Type type, Type attributeType, bool inherit)
        {
#if ASPNETCORE50
            return type.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray();
#else
            return type.GetCustomAttributes(attributeType, inherit).ToArray();
#endif
        }

        public static bool IsAbstract(this Type type)
        {
#if ASPNETCORE50
            return type.GetTypeInfo().IsAbstract;
#else
            return type.IsAbstract;
#endif
        }

        public static bool IsClass(this Type type)
        {
#if ASPNETCORE50
            return type.GetTypeInfo().IsClass;
#else
            return type.IsClass;
#endif
        }

        public static bool IsEnum(this Type type)
        {
#if ASPNETCORE50
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        public static bool IsGenericType(this Type type)
        {
#if ASPNETCORE50
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        public static bool IsGenericTypeDefinition(this Type type)
        {
#if ASPNETCORE50
            return type.GetTypeInfo().IsGenericTypeDefinition;
#else
            return type.IsGenericTypeDefinition;
#endif
        }

        public static bool IsInterface(this Type type)
        {
#if ASPNETCORE50
            return type.GetTypeInfo().IsInterface;
#else
            return type.IsInterface;
#endif
        }

        public static bool IsPrimitive(this Type type)
        {
#if ASPNETCORE50
            return type.GetTypeInfo().IsPrimitive;
#else
            return type.IsPrimitive;
#endif
        }

        public static bool IsSealed(this Type type)
        {
#if ASPNETCORE50
            return type.GetTypeInfo().IsSealed;
#else
            return type.IsSealed;
#endif
        }

        public static bool IsValueType(this Type type)
        {
#if ASPNETCORE50
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }
    }
}