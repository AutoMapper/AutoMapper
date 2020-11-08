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
    using static Internal.ExpressionFactory;
    using static ElementTypeHelper;
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
        public static Expression GenerateNonNullConstructorExpression(Type type) => 
            type.IsValueType ? Default(type) : (type == typeof(string) ? Constant(string.Empty) : GenerateConstructorExpression(type));
        public static Expression GenerateConstructorExpression(Type type)
        {
            if (type.IsValueType)
            {
                return Default(type);
            }
            if (type == typeof(string))
            {
                return Constant(null, typeof(string));
            }
            if (type.IsInterface)
            {
                return
                    type.IsDictionaryType() ? CreateCollection(type, typeof(Dictionary<,>))
                    : type.IsReadOnlyDictionaryType() ? CreateReadOnlyCollection(type, typeof(ReadOnlyDictionary<,>))
                    : type.IsSetType() ? CreateCollection(type, typeof(HashSet<>))
                    : type.IsEnumerableType() ? CreateCollection(type, typeof(List<>))
                    : InvalidInterfaceType(type);
            }
            if (type.IsAbstract)
            {
                return InvalidType(type, $"Cannot create an instance of abstract type {type}.");
            }
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
            var args = ctorWithOptionalArgs.GetParameters().Select(p => Constant(p.GetDefaultValue(), p.ParameterType));
            //create the ctor expression
            return New(ctorWithOptionalArgs, args);
        }
        private static Expression CreateCollection(Type type, Type collectionType)
        {
            var listType = MakeGenericType(type, collectionType);
            return type.IsAssignableFrom(listType) ? ToType(New(listType), type) : InvalidInterfaceType(type);
        }
        private static Type MakeGenericType(Type type, Type collectionType) =>  collectionType.MakeGenericType(GetElementTypes(type));
        private static Expression CreateReadOnlyCollection(Type type, Type collectionType)
        {
            var listType = MakeGenericType(type, collectionType);
            var ctor = listType.GetConstructors()[0];
            var innerType = ctor.GetParameters()[0].ParameterType;
            return type.IsAssignableFrom(listType) ? ToType(New(ctor, GenerateConstructorExpression(innerType)), type) : InvalidInterfaceType(type);
        }
        private static Expression InvalidInterfaceType(Type type) => InvalidType(type, $"Cannot create an instance of interface type {type}.");
        private static Expression InvalidType(Type type, string message) => Throw(Constant(new ArgumentException(message, "type")), type);
    }
}