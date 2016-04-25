using AutoMapper.QueryableExtensions;
using AutoMapper.QueryableExtensions.Impl;

namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

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
            new MemberGetterExpressionResultConverter(),
            new MemberResolverExpressionResultConverter(),
        };

        public bool CanResolve => CtorParams.All(param => param.CanResolve);

        public Expression NewExpression(Expression instanceParameter)
        {
            var parameters = CtorParams.Select(map =>
            {
                var result = new ExpressionResolutionResult(instanceParameter, Ctor.DeclaringType);

                var matchingExpressionConverter =
                    ExpressionResultConverters.FirstOrDefault(c => c.CanGetExpressionResolutionResult(result, map));

                if (matchingExpressionConverter == null)
                    throw new Exception("Can't resolve this to Queryable Expression");

                result = matchingExpressionConverter.GetExpressionResolutionResult(result, map);

                return result;
            });
            return Expression.New(Ctor, parameters.Select(p => p.ResolutionExpression));
        }

        public Expression BuildExpression(
            TypeMapRegistry typeMapRegistry,
            ParameterExpression srcParam, 
            ParameterExpression ctxtParam)
        {
            if (!CanResolve)
                return null;

            var ctorArgs = CtorParams.Select(p => p.CreateExpression(typeMapRegistry, srcParam, ctxtParam));

            ctorArgs =
                ctorArgs.Zip(Ctor.GetParameters(),
                    (exp, pi) => exp.Type == pi.ParameterType ? exp : Expression.Convert(exp, pi.ParameterType))
                    .ToArray();

            var newExpr = Expression.New(Ctor, ctorArgs);

            return newExpr;
        }

        public void AddParameter(ParameterInfo parameter, IMemberGetter[] resolvers, bool canResolve)
        {
            _ctorParams.Add(new ConstructorParameterMap(parameter, resolvers, canResolve));
        }
    }
}