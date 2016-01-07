using AutoMapper.QueryableExtensions;
using AutoMapper.QueryableExtensions.Impl;

namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Internal;

    public class ConstructorMap
    {
        private static readonly DelegateFactory DelegateFactory = new DelegateFactory();
        private readonly Lazy<LateBoundParamsCtor> _runtimeCtor;
        public ConstructorInfo Ctor { get; private set; }
        public IEnumerable<ConstructorParameterMap> CtorParams { get; }

        public ConstructorMap(ConstructorInfo ctor, IEnumerable<ConstructorParameterMap> ctorParams)
        {
            Ctor = ctor;
            CtorParams = ctorParams;

            _runtimeCtor = new Lazy<LateBoundParamsCtor>(() => DelegateFactory.CreateCtor(ctor, CtorParams));
        }

        private static readonly IExpressionResultConverter[] ExpressionResultConverters =
        {
            new MemberGetterExpressionResultConverter(),
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

                var sourceType = result.Type;
                var destinationType = map.Parameter.ParameterType;

                var typeMap = context.ConfigurationProvider.ResolveTypeMap(result, destinationType);

                Type targetSourceType = typeMap != null ? typeMap.SourceType : sourceType;

                var newContext = context.CreateTypeContext(typeMap, result.Value, null, targetSourceType,
                    destinationType);

                if (typeMap == null && map.Parameter.IsOptional)
                {
                    object value = map.Parameter.DefaultValue;
                    ctorArgs.Add(value);
                }
                else
                {
                    var value = context.Engine.Map(newContext);
                    ctorArgs.Add(value);
                }
            }

            return _runtimeCtor.Value(ctorArgs.ToArray());
        }
    }
}