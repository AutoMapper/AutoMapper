using AutoMapper.Configuration;

namespace AutoMapper.Execution
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using static System.Linq.Expressions.Expression;

    public class DelegateFactory
    {
        private static readonly ConcurrentDictionary<Type, Expression> _ctorCache =
            new ConcurrentDictionary<Type, Expression>();

        private static readonly Func<Type, Expression> _generateConstructor;

        static DelegateFactory()
        {
            _generateConstructor = CreateObjectExpression;
        }

        public Expression<LateBoundMethod<object, TValue>> CreateGet<TValue>(MethodInfo method)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
            ParameterExpression argumentsParameter = Expression.Parameter(typeof (object[]), "arguments");

            MethodCallExpression call;
            if (!method.IsDefined(typeof (ExtensionAttribute), false))
            {
                // instance member method
                call = Expression.Call(Expression.Convert(instanceParameter, method.DeclaringType), method,
                    CreateParameterExpressions(method, instanceParameter, argumentsParameter));
            }
            else
            {
                // static extension method
                call = Expression.Call(
                    method,
                    CreateParameterExpressions(method, instanceParameter, argumentsParameter));
            }

            Expression<LateBoundMethod<object, TValue>> lambda = Expression.Lambda<LateBoundMethod<object, TValue>>(
                call,
                instanceParameter,
                argumentsParameter);

            return lambda;
        }

        public Expression<LateBoundPropertyGet<TSource, TValue>> CreateGet<TSource, TValue>(PropertyInfo property)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(TSource), "target");

            Expression member = IfNotNullExpression(Expression.Property(instanceParameter, property));

            Expression<LateBoundPropertyGet<TSource, TValue>> lambda = Expression.Lambda<LateBoundPropertyGet<TSource, TValue>>(member,instanceParameter);

            return lambda;
        }

        public Expression<LateBoundFieldGet<TSource, TValue>> CreateGet<TSource, TValue>(FieldInfo field)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(TSource), "target");

            Expression member = IfNotNullExpression(Expression.Field(instanceParameter, field));

            Expression<LateBoundFieldGet<TSource, TValue>> lambda = Expression.Lambda<LateBoundFieldGet<TSource, TValue>>(member, instanceParameter);

            return lambda;
        }

        public static Expression IfNotNullExpression(MemberExpression member)
        {
            if (member.Expression != null && !member.Expression.Type.IsValueType())
                return Expression.Condition(Expression.Equal(member.Expression, Expression.Default(member.Expression.Type)),
                Expression.Default(member.Type), member);
            return member;
        }

        public Expression<LateBoundFieldSet<TSource, TValue>> CreateSet<TSource, TValue>(FieldInfo field)
        {
            ParameterExpression instanceParameter = Expression.Parameter(field.DeclaringType, "target");
            ParameterExpression valueParameter = Expression.Parameter(field.FieldType, "value");

            MemberExpression member = Expression.Field(instanceParameter, field);
            BinaryExpression assignExpression = Expression.Assign(member, valueParameter);

            Expression<LateBoundFieldSet<TSource, TValue>> lambda = Expression.Lambda<LateBoundFieldSet<TSource, TValue>>(
                assignExpression,
                instanceParameter,
                valueParameter
                );

            return lambda;
        }

        public Expression<LateBoundPropertySet<TSource, TValue>> CreateSet<TSource, TValue>(PropertyInfo property)
        {
            ParameterExpression instanceParameter = Expression.Parameter(property.DeclaringType, "target");
            ParameterExpression valueParameter = Expression.Parameter(property.PropertyType, "value");

            MemberExpression member = Expression.Property(instanceParameter, property);
            BinaryExpression assignExpression = Expression.Assign(member, valueParameter);

            Expression<LateBoundPropertySet<TSource, TValue>> lambda = Expression.Lambda<LateBoundPropertySet<TSource, TValue>>(
                assignExpression,
                instanceParameter,
                valueParameter
                );


            return lambda;
        }

        public static Expression CreateCtorExpression(Type type)
        {
            return _ctorCache.GetOrAdd(type, _generateConstructor);
        }

        private static Expression CreateObjectExpression(Type type)
        {
            return type.IsArray
                ? CreateArrayExpression(type.GetElementType(), 0)
                : type == typeof(string)
                    ? Expression.Constant(string.Empty)
                    : type.IsInterface() && type.IsDictionaryType()
                        ? CreateDictionaryExpression(type)
                        : GenerateConstructorExpression(type);
        }

        private static Expression CreateArrayExpression(Type elementType, int length)
        {
            return Expression.Call(null, typeof(Array).GetDeclaredMethods().First(_ => _.Name == "CreateInstance"),
                Expression.Constant(elementType), Expression.Constant(length));
        }

        private static Expression CreateDictionaryExpression(Type dictionaryType)
        {
            Type keyType = dictionaryType.GetTypeInfo().GenericTypeArguments[0];
            Type valueType = dictionaryType.GetTypeInfo().GenericTypeArguments[1];
            var type = dictionaryType.IsInterface()
                ? typeof(Dictionary<,>).MakeGenericType(keyType, valueType)
                : dictionaryType;
            
            return CreateCtorExpression(type);
        }

        public static Expression GenerateConstructorExpression(Type type)
        {
            //handle valuetypes
            if (!type.IsClass())
            {
                return Expression.Convert(Expression.New(type), typeof(object));
            }

            var constructors = type
                .GetDeclaredConstructors()
                .Where(ci => !ci.IsStatic);

            if (type == typeof(string))
                return Expression.Constant("");

            //find a ctor with only optional args
            var ctorWithOptionalArgs = constructors.FirstOrDefault(c => c.GetParameters().All(p => p.IsOptional));
            if(ctorWithOptionalArgs == null)
            {
                var ex = new ArgumentException(type + " needs to have a constructor with 0 args or only optional args", "type");
                return Block(Throw(Expression.Constant(ex)), Expression.Constant(null));
            }
            //get all optional default values
            var args = ctorWithOptionalArgs
                .GetParameters()
                .Select(p => Expression.Constant(p.GetDefaultValue(), p.ParameterType)).ToArray();

            //create the ctor expression
            return Expression.New(ctorWithOptionalArgs, args);
        }

        private static Expression[] CreateParameterExpressions(MethodInfo method, Expression instanceParameter,
            Expression argumentsParameter)
        {
            var expressions = new List<Expression>();
            var realMethodParameters = method.GetParameters();
            if (method.IsDefined(typeof (ExtensionAttribute), false))
            {
                Type extendedType = method.GetParameters()[0].ParameterType;
                expressions.Add(instanceParameter.ToType(extendedType));
                realMethodParameters = realMethodParameters.Skip(1).ToArray();
            }

            expressions.AddRange(
                realMethodParameters.Select(
                    (parameter, index) => argumentsParameter.Index(index).ToType(parameter.ParameterType)));

            return expressions.ToArray();
        }
    }
}