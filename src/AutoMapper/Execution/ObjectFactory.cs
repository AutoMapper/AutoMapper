using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.Execution
{
    using static Expression;
    using static ExpressionFactory;
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ObjectFactory
    {
        private static readonly LockingConcurrentDictionary<Type, Func<object>> CtorCache = new LockingConcurrentDictionary<Type, Func<object>>(GenerateConstructor);
        public static object CreateInstance(Type type) => CtorCache.GetOrAdd(type)();
        private static Func<object> GenerateConstructor(Type type)
        {
            var constructor = GenerateConstructorExpression(type);
            if (type.IsValueType)
            {
                constructor = Convert(constructor, typeof(object));
            }
            return Lambda<Func<object>>(constructor).Compile();
        }
        public static object CreateInterfaceProxy(Type interfaceType) => CreateInstance(ProxyGenerator.GetProxyType(interfaceType));
        public static Expression GenerateConstructorExpression(Type type) => type switch
        {
            { IsValueType: true } => Default(type),
            Type stringType when stringType == typeof(string) => Constant(string.Empty),
            { IsInterface: true } => CreateInterfaceExpression(type),
            { IsAbstract: true } => InvalidType(type, $"Cannot create an instance of abstract type {type}."),
            _ => CallConstructor(type)
        };
        private static Expression CallConstructor(Type type)
        {
            var defaultCtor = type.GetConstructor(TypeExtensions.InstanceFlags, null, Type.EmptyTypes, null);
            if (defaultCtor != null)
            {
                return New(defaultCtor);
            }
            //find a ctor with only optional args
            var ctorWithOptionalArgs = type.GetDeclaredConstructors().FirstOrDefault(c => c.GetParameters().All(p => p.IsOptional));
            if (ctorWithOptionalArgs == null)
            {
                return InvalidType(type, $"{type} needs to have a constructor with 0 args or only optional args.");
            }
            //get all optional default values
            var args = ctorWithOptionalArgs.GetParameters().Select(p=>ToType(p.GetDefaultValue(), p.ParameterType));
            //create the ctor expression
            return New(ctorWithOptionalArgs, args);
        }
        private static Expression CreateInterfaceExpression(Type type) =>
            type.IsGenericType(typeof(IDictionary<,>)) ? CreateCollection(type, typeof(Dictionary<,>)) : 
            type.IsGenericType(typeof(IReadOnlyDictionary<,>)) ? CreateReadOnlyCollection(type, typeof(ReadOnlyDictionary<,>)) : 
            type.IsGenericType(typeof(ISet<>)) ? CreateCollection(type, typeof(HashSet<>)) : 
            type.IsEnumerableType() ? CreateCollection(type, typeof(List<>), GetIEnumerableArguments(type)) : 
            InvalidInterfaceType(type);
        private static Type[] GetIEnumerableArguments(Type type) => type.GetIEnumerableType()?.GenericTypeArguments ?? new[] { typeof(object) };
        private static Expression CreateCollection(Type type, Type collectionType, Type[] genericArguments = null) => 
            ToType(New(collectionType.MakeGenericType(genericArguments ?? type.GenericTypeArguments)), type);
        private static Expression CreateReadOnlyCollection(Type type, Type collectionType)
        {
            var listType = collectionType.MakeGenericType(type.GenericTypeArguments);
            var ctor = listType.GetConstructors()[0];
            var innerType = ctor.GetParameters()[0].ParameterType;
            return ToType(New(ctor, GenerateConstructorExpression(innerType)), type);
        }
        private static Expression InvalidInterfaceType(Type type) => InvalidType(type, $"Cannot create an instance of interface type {type}.");
        private static Expression InvalidType(Type type, string message) => Throw(Constant(new ArgumentException(message, "type")), type);
    }
}