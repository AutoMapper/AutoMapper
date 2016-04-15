namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
#if !PORTABLE
    using System.Reflection.Emit;
#endif

    internal static class TypeExtensions
    {
        public static Expression ToObject(this Expression expression)
        {
            return expression.Type == typeof (object) ? expression : Expression.Convert(expression, typeof (object));
        }

        public static Expression ToType(this Expression expression, Type type)
        {
            return expression.Type == type ? expression : Expression.Convert(expression, type);
        }

        /// <param name="type">The type to construct.</param>
        /// <param name="getClosedGenericInterfaceType">
        /// For generic interfaces, the only way to reliably determine the implementing type's generic type arguments
        /// is to know the closed type of the desired interface implementation since there may be multiple implementations
        /// of the same generic interface on this type.
        /// </param>
        public static Func<ResolutionContext, TServiceType> BuildCtor<TServiceType>(this Type type, Func<ResolutionContext, Type> getClosedGenericInterfaceType = null)
        {
            return context =>
            {
                // Warning: do not mutate the parameter @type. It's in a shared closure and will be remembered in subsequent calls to this function.
                // Otherwise all ctors for the same generic type definition will return whatever closed type then first one calculates.
                var concreteType = type;

                if (type.IsGenericTypeDefinition())
                {
                    if (getClosedGenericInterfaceType == null) throw new ArgumentNullException(nameof(getClosedGenericInterfaceType), "For generic interfaces, the desired closed interface type must be known.");
                    var closedInterfaceType = getClosedGenericInterfaceType.Invoke(context);
                    var implementationTypeArguments = type.GetImplementedInterface(closedInterfaceType.GetGenericTypeDefinition(), closedInterfaceType.GenericTypeArguments).GenericTypeArguments;

                    var genericParameters = type.GetTypeInfo().GenericTypeParameters;
                    var deducedTypeArguments = new Type[genericParameters.Length];
                    DeduceGenericArguments(genericParameters, deducedTypeArguments, implementationTypeArguments[0], context.SourceType);
                    DeduceGenericArguments(genericParameters, deducedTypeArguments, implementationTypeArguments[1], context.DestinationType);
                    
                    if (deducedTypeArguments.Any(_ => _ == null)) throw new InvalidOperationException($"One or more type arguments to {type.Name} cannot be determined.");
                    concreteType = type.MakeGenericType(deducedTypeArguments);
                }

                var obj = context.Options.ServiceCtor.Invoke(concreteType);

                return (TServiceType)obj;
            };
        }

        private static void DeduceGenericArguments(Type[] genericParameters, Type[] deducedGenericArguments, Type typeUsingParameters, Type typeUsingArguments)
        {
            if (typeUsingParameters.IsByRef)
            {
                DeduceGenericArguments(genericParameters, deducedGenericArguments, typeUsingParameters.GetElementType(), typeUsingArguments.GetElementType());
                return;
            }

            var index = Array.IndexOf(genericParameters, typeUsingParameters);
            if (index != -1)
            {
                if (deducedGenericArguments[index] == null)
                    deducedGenericArguments[index] = typeUsingArguments;
                else if (deducedGenericArguments[index] != typeUsingArguments)
                    throw new NotImplementedException("Generic variance is not implemented.");
            }
            else if (typeUsingParameters.IsGenericType() && typeUsingArguments.IsGenericType())
            {
                var childArgumentsUsingParameters = typeUsingParameters.GenericTypeArguments;
                var childArgumentsUsingArguments = typeUsingArguments.GenericTypeArguments;
                for (var i = 0; i < childArgumentsUsingParameters.Length; i++)
                    DeduceGenericArguments(genericParameters, deducedGenericArguments, childArgumentsUsingParameters[i], childArgumentsUsingArguments[i]);
            }
        }

        private static Type GetImplementedInterface(this Type implementation, Type interfaceDefinition, params Type[] interfaceGenericArguments)
        {
            return implementation.GetTypeInfo().ImplementedInterfaces.Single(implementedInterface =>
            {
                if (implementedInterface.GetGenericTypeDefinition() != interfaceDefinition) return false;

                var implementedInterfaceArguments = implementedInterface.GenericTypeArguments;
                for (var i = 0; i < interfaceGenericArguments.Length; i++)
                {
                    // This assumes the interface type parameters are not covariant or contravariant
                    if (implementedInterfaceArguments[i].GetGenericTypeDefinitionIfGeneric() != interfaceGenericArguments[i].GetGenericTypeDefinitionIfGeneric()) return false;
                }

                return true;
            });
        }

        public static Type GetGenericTypeDefinitionIfGeneric(this Type type)
        {
            return type.IsGenericType() ? type.GetGenericTypeDefinition() : type;
        }

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
            return type
                .GetAllMethods()
                .Where(mi => mi.Name == name)
                .Where(mi => mi.GetParameters().Length == parameters.Length)
                .FirstOrDefault(mi => mi.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(parameters));
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
