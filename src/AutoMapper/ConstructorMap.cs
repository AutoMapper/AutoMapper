using AutoMapper.QueryableExtensions;
using AutoMapper.QueryableExtensions.Impl;

namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Execution;

    public class ConstructorMap
    {
        private static readonly DelegateFactory DelegateFactory = new DelegateFactory();
        private readonly Lazy<LateBoundParamsCtor> _runtimeCtor;
        public ConstructorInfo Ctor { get; private set; }
        internal ConstructorParameterMap[] CtorParams { get; }

        public ConstructorMap(ConstructorInfo ctor, ConstructorParameterMap[] ctorParams)
        {
            Ctor = ctor;
            CtorParams = ctorParams;

            _runtimeCtor = new Lazy<LateBoundParamsCtor>(() => DelegateFactory.CreateCtor(ctor, CtorParams));
        }

        private static readonly IExpressionResultConverter[] ExpressionResultConverters =
        {
            new MemberGetterExpressionResultConverter(),
            new ExpressionBasedResolverResultConverter(),
            new MemberResolverExpressionResultConverter(),
            new NullSubstitutionExpressionResultConverter()
        };

        public Expression NewExpression(Expression instanceParameter)
        {
            var parameters = CtorParams.Select(map =>
            {
                var result = new ExpressionResolutionResult(instanceParameter, Ctor.DeclaringType);
                foreach (var resolver in map.SourceResolvers)
                {
                    var matchingExpressionConverter =
                        ExpressionResultConverters.FirstOrDefault(c => c.CanGetExpressionResolutionResult(result, resolver));
                    if (matchingExpressionConverter == null)
                        throw new Exception("Can't resolve this to Queryable Expression");
                    result = matchingExpressionConverter.GetExpressionResolutionResult(result, map, resolver);
                }
                return result;
            });
            return Expression.New(Ctor, parameters.Select(p => p.ResolutionExpression));
        }

        public object ResolveValue(ResolutionContext context)
        {
            var ctorArgs = new List<object>();

            foreach (var map in CtorParams)
            {
                var result = map.ResolveValue(context);

                var sourceType = result?.GetType() ?? context.SourceType;
                var destinationType = map.Parameter.ParameterType;

                var typeMap = context.ConfigurationProvider.ResolveTypeMap(sourceType, context.SourceType, destinationType);

                if (typeMap == null && map.Parameter.IsOptional)
                {
                    object value = map.Parameter.DefaultValue;
                    ctorArgs.Add(value);
                }
                else
                {
                    var value = context.Mapper.Map(result, null, sourceType, destinationType, context);
                    ctorArgs.Add(value);
                }
            }

            return _runtimeCtor.Value(ctorArgs.ToArray());
        }
    }
}