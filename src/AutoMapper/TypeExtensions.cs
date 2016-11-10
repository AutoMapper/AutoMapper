namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
#if NET45
    using System.Reflection.Emit;
#endif

    internal static class TypeExtensions
    {
        public static bool Has<TAttribute>(this Type type) where TAttribute : Attribute
        {
            return type.GetTypeInfo().IsDefined(typeof(TAttribute), inherit: false);
        }

        public static Type GetGenericTypeDefinitionIfGeneric(this Type type)
        {
            return type.IsGenericType() ? type.GetGenericTypeDefinition() : type;
        }

        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments;
        }

        public static Type[] GetGenericParameters(this Type type)
        {
            return type.GetGenericTypeDefinition().GetTypeInfo().GenericTypeParameters;
        }

        public static IEnumerable<ConstructorInfo> GetDeclaredConstructors(this Type type)
        {
            return type.GetTypeInfo().DeclaredConstructors;
        }

#if NET45
        public static Type CreateType(this TypeBuilder type)
        {
            return type.CreateTypeInfo().AsType();
        }
#endif

        public static IEnumerable<MemberInfo> GetDeclaredMembers(this Type type)
        {
            return type.GetTypeInfo().DeclaredMembers;
        }

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

        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type)
        {
            return type.GetTypeInfo().DeclaredMethods;
        }

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

        public static IEnumerable<MethodInfo> GetAllMethods(this Type type)
        {
            return type.GetRuntimeMethods();
        }

        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type type)
        {
            return type.GetTypeInfo().DeclaredProperties;
        }

        public static PropertyInfo GetDeclaredProperty(this Type type, string name)
        {
            return type.GetTypeInfo().GetDeclaredProperty(name);
        }

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
            return (memberInfo as FieldInfo).IsStatic() 
                || (memberInfo as PropertyInfo).IsStatic()
                || ((memberInfo as MethodInfo)?.IsStatic
                ?? false);
        }

        public static bool IsPublic(this PropertyInfo propertyInfo)
        {
            return (propertyInfo?.GetGetMethod(true)?.IsPublic ?? false)
                || (propertyInfo?.GetSetMethod(true)?.IsPublic ?? false);
        }

        public static IEnumerable<PropertyInfo> PropertiesWithAnInaccessibleSetter(this Type type)
        {
            return type.GetDeclaredProperties().Where(pm => pm.HasAnInaccessibleSetter());
        }

        public static bool HasAnInaccessibleSetter(this PropertyInfo property)
        {
            var setMethod = property.GetSetMethod(true);
            return setMethod == null || setMethod.IsPrivate || setMethod.IsFamily;
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

        public static bool IsAssignableFrom(this Type type, Type other)
        {
            return type.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo());
        }

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
            return o != null && type.GetTypeInfo().IsAssignableFrom(o.GetType().GetTypeInfo());
        }

        public static ConstructorInfo[] GetConstructors(this Type type)
        {
            return type.GetTypeInfo().DeclaredConstructors.ToArray();
        }

        public static PropertyInfo[] GetProperties(this Type type)
        {
            return type.GetRuntimeProperties().ToArray();
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
