namespace AutoMapper.Execution
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class DelegateFactory
    {
        private readonly ConcurrentDictionary<Type, LateBoundCtor> _ctorCache =
            new ConcurrentDictionary<Type, LateBoundCtor>();

        private readonly Func<Type, LateBoundCtor> _generateConstructor;

        public DelegateFactory()
        {
            _generateConstructor = GenerateConstructor;
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

        public LateBoundCtor CreateCtor(Type type)
        {
            var ctor = _ctorCache.GetOrAdd(type, _generateConstructor);
            return ctor;
        }

        private static LateBoundCtor GenerateConstructor(Type type)
        {
            //handle valuetypes
            if(!type.IsClass())
            {
                var ctorExpression =
                    Expression.Lambda<LateBoundCtor>(Expression.Convert(Expression.New(type), typeof(object)));
                return ctorExpression.Compile();
            }
            else
            {
                var constructors = type
                    .GetDeclaredConstructors()
                    .Where(ci => !ci.IsStatic);

                //find a ctor with only optional args
                var ctorWithOptionalArgs = constructors.FirstOrDefault(c => c.GetParameters().All(p => p.IsOptional));
                if(ctorWithOptionalArgs == null)
                    throw new ArgumentException(type + " needs to have a constructor with 0 args or only optional args", "type");

                //get all optional default values
                var args = ctorWithOptionalArgs
                    .GetParameters()
                    .Select(p => Expression.Constant(p.DefaultValue, p.ParameterType)).ToArray();

                //create the ctor expression
                var ctorExpression =
                    Expression.Lambda<LateBoundCtor>(Expression.Convert(Expression.New(ctorWithOptionalArgs, args),
                        typeof(object)));
                return ctorExpression.Compile();
            }
        }

        private static Expression[] CreateParameterExpressions(MethodInfo method, Expression instanceParameter,
            Expression argumentsParameter)
        {
            var expressions = new List<UnaryExpression>();
            var realMethodParameters = method.GetParameters();
            if (method.IsDefined(typeof (ExtensionAttribute), false))
            {
                Type extendedType = method.GetParameters()[0].ParameterType;
                expressions.Add(Expression.Convert(instanceParameter, extendedType));
                realMethodParameters = realMethodParameters.Skip(1).ToArray();
            }

            expressions.AddRange(realMethodParameters.Select((parameter, index) =>
                Expression.Convert(
                    Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)),
                    parameter.ParameterType)));

            return expressions.ToArray();
        }

        public LateBoundParamsCtor CreateCtor(ConstructorInfo constructorInfo,
            IEnumerable<ConstructorParameterMap> ctorParams)
        {
            ParameterExpression paramsExpr = Expression.Parameter(typeof (object[]), "parameters");

            var convertExprs = ctorParams
                .Select((ctorParam, i) => Expression.Convert(
                    Expression.ArrayIndex(paramsExpr, Expression.Constant(i)),
                    ctorParam.Parameter.ParameterType))
                .ToArray();

            NewExpression newExpression = Expression.New(constructorInfo, convertExprs);

            var lambda = Expression.Lambda<LateBoundParamsCtor>(newExpression, paramsExpr);

            return lambda.Compile();
        }
    }
}