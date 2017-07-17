using System;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.Execution
{
    using static Expression;
    using static ExpressionBuilder;

    public static class DelegateFactory
    {
        private static readonly LockingConcurrentDictionary<Type, Func<object>> CtorCache = new LockingConcurrentDictionary<Type, Func<object>>(GenerateConstructor);

        public static Func<object> CreateCtor(Type type) => CtorCache.GetOrAdd(type);

        private static Func<object> GenerateConstructor(Type type)
        {
            var ctorExpr = GenerateConstructorExpression(type);

            return Lambda<Func<object>>(Convert(ctorExpr, typeof(object))).Compile();
        }

        public static Expression GenerateConstructorExpression(Type type, ProfileMap configuration) =>
            configuration.AllowNullDestinationValues
                ? GenerateConstructorExpression(type)
                : GenerateNonNullConstructorExpression(type);

        public static Expression GenerateNonNullConstructorExpression(Type type) => type.IsValueType()
            ? Default(type)
            : (type == typeof(string)
                ? Constant(string.Empty)
                : GenerateConstructorExpression(type)
            );

        public static Expression GenerateConstructorExpression(Type type)
        {
            if (type.IsValueType())
            {
                return Default(type);
            }

            if (type == typeof(string))
            {
                return Constant(null, typeof(string));
            }

            if (type.IsAbstract())
            {
                return InvalidType(type, $"Cannot create an instance of abstract type {type}.");
            }

            var constructors = type
                .GetDeclaredConstructors()
                .Where(ci => !ci.IsStatic);

            //find a ctor with only optional args
            var ctorWithOptionalArgs = constructors.FirstOrDefault(c => c.GetParameters().All(p => p.IsOptional));
            if (ctorWithOptionalArgs == null)
            {
                return InvalidType(type, $"{type} needs to have a constructor with 0 args or only optional args.");
            }
            //get all optional default values
            var args = ctorWithOptionalArgs
                .GetParameters()
                .Select(p => Constant(p.GetDefaultValue(), p.ParameterType)).ToArray();

            //create the ctor expression
            return New(ctorWithOptionalArgs, args);
        }

        private static Expression InvalidType(Type type, string message)
        {
            var ex = new ArgumentException(message, "type");
            return Block(Throw(Constant(ex)), Constant(null, type));
        }
    }
}