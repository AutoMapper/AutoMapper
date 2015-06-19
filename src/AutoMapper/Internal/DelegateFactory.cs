namespace AutoMapper.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// 
    /// </summary>
    public class DelegateFactory
    {
        private static IDictionaryFactory DictionaryFactory { get; }
            = PlatformAdapter.Resolve<IDictionaryFactory>();

        private IDictionary<Type, LateBoundCtor> CtorCache { get; }
            = DictionaryFactory.CreateDictionary<Type, LateBoundCtor>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public LateBoundMethod CreateGet(MethodInfo method)
        {
            var instanceParameter = Expression.Parameter(typeof (object), "target");
            var argumentsParameter = Expression.Parameter(typeof (object[]), "arguments");

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

            var lambda = Expression.Lambda<LateBoundMethod>(
                Expression.Convert(call, typeof (object)),
                instanceParameter,
                argumentsParameter);

            return lambda.Compile();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public LateBoundPropertyGet CreateGet(PropertyInfo property)
        {
            var instanceParameter = Expression.Parameter(typeof (object), "target");

            var member = Expression.Property(
                Expression.Convert(instanceParameter, property.DeclaringType), property);

            var lambda = Expression.Lambda<LateBoundPropertyGet>(
                Expression.Convert(member, typeof (object)),
                instanceParameter
                );

            return lambda.Compile();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public LateBoundFieldGet CreateGet(FieldInfo field)
        {
            var instanceParameter = Expression.Parameter(typeof (object), "target");

            var member = Expression.Field(Expression.Convert(instanceParameter, field.DeclaringType), field);

            var lambda = Expression.Lambda<LateBoundFieldGet>(
                Expression.Convert(member, typeof (object)),
                instanceParameter
                );

            return lambda.Compile();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public LateBoundFieldSet CreateSet(FieldInfo field)
        {
            var instanceParameter = Expression.Parameter(typeof (object), "target");
            var valueParameter = Expression.Parameter(typeof (object), "value");

            var member = Expression.Field(Expression.Convert(instanceParameter, field.DeclaringType), field);
            var assignExpression = Expression.Assign(member,
                Expression.Convert(valueParameter, field.FieldType));

            var lambda = Expression.Lambda<LateBoundFieldSet>(
                assignExpression,
                instanceParameter,
                valueParameter
                );

            return lambda.Compile();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public LateBoundPropertySet CreateSet(PropertyInfo property)
        {
            var instanceParameter = Expression.Parameter(typeof (object), "target");
            var valueParameter = Expression.Parameter(typeof (object), "value");

            var member = Expression.Property(
                Expression.Convert(instanceParameter, property.DeclaringType), property);
            var assignExpression = Expression.Assign(member,
                Expression.Convert(valueParameter, property.PropertyType));

            var lambda = Expression.Lambda<LateBoundPropertySet>(
                assignExpression,
                instanceParameter,
                valueParameter
                );

            return lambda.Compile();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public LateBoundCtor CreateCtor(Type type)
        {
            var ctor = CtorCache.GetOrAdd(type, t =>
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
                            "Type needs to have a constructor with 0 args or only optional args", nameof(type));

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="instanceParameter"></param>
        /// <param name="argumentsParameter"></param>
        /// <returns></returns>
        private static Expression[] CreateParameterExpressions(MethodInfo method, Expression instanceParameter,
            Expression argumentsParameter)
        {
            var expressions = new List<UnaryExpression>();
            var realMethodParameters = method.GetParameters();
            if (method.IsDefined(typeof (ExtensionAttribute), false))
            {
                var extendedType = method.GetParameters()[0].ParameterType;
                expressions.Add(Expression.Convert(instanceParameter, extendedType));
                realMethodParameters = realMethodParameters.Skip(1).ToArray();
            }

            expressions.AddRange(realMethodParameters.Select((parameter, index) =>
                Expression.Convert(
                    Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)),
                    parameter.ParameterType)));

            return expressions.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="constructorInfo"></param>
        /// <param name="ctorParams"></param>
        /// <returns></returns>
        public LateBoundParamsCtor CreateCtor(ConstructorInfo constructorInfo,
            IEnumerable<ConstructorParameterMap> ctorParams)
        {
            var paramsExpr = Expression.Parameter(typeof (object[]), "parameters");

            var convertExprs = ctorParams
                .Select((ctorParam, i) => Expression.Convert(
                    Expression.ArrayIndex(paramsExpr, Expression.Constant(i)),
                    ctorParam.Parameter.ParameterType))
                .ToArray();

            var newExpression = Expression.New(constructorInfo, convertExprs);

            var lambda = Expression.Lambda<LateBoundParamsCtor>(newExpression, paramsExpr);

            return lambda.Compile();
        }
    }
}