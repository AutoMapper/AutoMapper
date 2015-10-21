namespace AutoMapper.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class DelegateFactory
    {
        private static readonly IDictionaryFactory DictionaryFactory = PlatformAdapter.Resolve<IDictionaryFactory>();

        private readonly IDictionary<Type, LateBoundCtor> _ctorCache =
            DictionaryFactory.CreateDictionary<Type, LateBoundCtor>();

        public LateBoundMethod CreateGet(MethodInfo method)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof (object), "target");
            ParameterExpression argumentsParameter = Expression.Parameter(typeof (object[]), "arguments");

            MethodCallExpression call;
            if (!method.IsDefined(typeof (ExtensionAttribute), false))
            {
                // instance member method
                call = Expression.Call(
                    Expression.Convert(instanceParameter, method.DeclaringType),
                    method,
                    CreateParameterExpressions(method, instanceParameter, argumentsParameter));
            }
            else
            {
                // static extension method
                call = Expression.Call(
                    method,
                    CreateParameterExpressions(method, instanceParameter, argumentsParameter));
            }

            Expression<LateBoundMethod> lambda = Expression.Lambda<LateBoundMethod>(
                Expression.Convert(call, typeof (object)),
                instanceParameter,
                argumentsParameter);

            return lambda.Compile();
        }

        public LateBoundPropertyGet CreateGet(PropertyInfo property)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof (object), "target");

            MemberExpression member = Expression.Property(
                Expression.Convert(instanceParameter, property.DeclaringType), property);

            Expression<LateBoundPropertyGet> lambda = Expression.Lambda<LateBoundPropertyGet>(
                Expression.Convert(member, typeof (object)),
                instanceParameter
                );

            return lambda.Compile();
        }

        public LateBoundFieldGet CreateGet(FieldInfo field)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof (object), "target");

            MemberExpression member = Expression.Field(Expression.Convert(instanceParameter, field.DeclaringType), field);

            Expression<LateBoundFieldGet> lambda = Expression.Lambda<LateBoundFieldGet>(
                Expression.Convert(member, typeof (object)),
                instanceParameter
                );

            return lambda.Compile();
        }

        public LateBoundFieldSet CreateSet(FieldInfo field)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof (object), "target");
            ParameterExpression valueParameter = Expression.Parameter(typeof (object), "value");

            MemberExpression member = Expression.Field(Expression.Convert(instanceParameter, field.DeclaringType), field);
            BinaryExpression assignExpression = Expression.Assign(member,
                Expression.Convert(valueParameter, field.FieldType));

            Expression<LateBoundFieldSet> lambda = Expression.Lambda<LateBoundFieldSet>(
                assignExpression,
                instanceParameter,
                valueParameter
                );

            return lambda.Compile();
        }

        public LateBoundPropertySet CreateSet(PropertyInfo property)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof (object), "target");
            ParameterExpression valueParameter = Expression.Parameter(typeof (object), "value");

            MemberExpression member = Expression.Property(
                Expression.Convert(instanceParameter, property.DeclaringType), property);
            BinaryExpression assignExpression = Expression.Assign(member,
                Expression.Convert(valueParameter, property.PropertyType));

            Expression<LateBoundPropertySet> lambda = Expression.Lambda<LateBoundPropertySet>(
                assignExpression,
                instanceParameter,
                valueParameter
                );


            return lambda.Compile();
        }

        public LateBoundCtor CreateCtor(Type type)
        {
            LateBoundCtor ctor = _ctorCache.GetOrAdd(type, t =>
            {
                //handle valuetypes
                if (!type.IsClass())
                {
                    var ctorExpression =
                        Expression.Lambda<LateBoundCtor>(Expression.Convert(Expression.New(type), typeof (object)));
                    return ctorExpression.Compile();
                }
                else
                {
                    var constructors = type
                        .GetDeclaredConstructors()
                        .Where(ci => !ci.IsStatic);

                    //find a ctor with only optional args
                    var ctorWithOptionalArgs = constructors.FirstOrDefault(c => c.GetParameters().All(p => p.IsOptional));
                    if (ctorWithOptionalArgs == null)
                        throw new ArgumentException(
                            "Type needs to have a constructor with 0 args or only optional args", "type");

                    //get all optional default values
                    var args = ctorWithOptionalArgs
                        .GetParameters()
                        .Select(p => Expression.Constant(p.DefaultValue, p.ParameterType)).ToArray();

                    //create the ctor expression
                    var ctorExpression =
                        Expression.Lambda<LateBoundCtor>(Expression.Convert(Expression.New(ctorWithOptionalArgs, args),
                            typeof (object)));
                    return ctorExpression.Compile();
                }
            });

            return ctor;
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