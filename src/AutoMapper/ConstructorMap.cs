using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Execution;
using AutoMapper.QueryableExtensions;
using AutoMapper.QueryableExtensions.Impl;

namespace AutoMapper
{
    using static Expression;

    public class ConstructorMap
    {
        private readonly IList<ConstructorParameterMap> _ctorParams = new List<ConstructorParameterMap>();

        public ConstructorInfo Ctor { get; }
        public TypeMap TypeMap { get; }
        internal IEnumerable<ConstructorParameterMap> CtorParams => _ctorParams;

        public ConstructorMap(ConstructorInfo ctor, TypeMap typeMap)
        {
            Ctor = ctor;
            TypeMap = typeMap;
        }

        private static readonly IExpressionResultConverter[] ExpressionResultConverters =
        {
            new MemberResolverExpressionResultConverter(),
            new MemberGetterExpressionResultConverter()
        };

        public bool CanResolve => CtorParams.All(param => param.CanResolve);

        public Expression NewExpression(Expression instanceParameter)
        {
            var parameters = CtorParams.Select(map =>
            {
                var result = new ExpressionResolutionResult(instanceParameter, Ctor.DeclaringType);

                var matchingExpressionConverter =
                    ExpressionResultConverters.FirstOrDefault(c => c.CanGetExpressionResolutionResult(result, map));

                result = matchingExpressionConverter?.GetExpressionResolutionResult(result, map)
                    ?? throw new AutoMapperMappingException($"Unable to generate the instantiation expression for the constructor {Ctor}: no expression could be mapped for constructor parameter '{map.Parameter}'.", null, TypeMap.Types);

                return result;
            });
            return New(Ctor, parameters.Select(p => p.ResolutionExpression));
        }

        public void AddParameter(ParameterInfo parameter, MemberInfo[] resolvers, bool canResolve)
        {
            _ctorParams.Add(new ConstructorParameterMap(parameter, resolvers, canResolve));
        }
    }
}
