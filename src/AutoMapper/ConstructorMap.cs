using AutoMapper.QueryableExtensions;
using AutoMapper.QueryableExtensions.Impl;
using System.Linq.Expressions;

namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Execution;
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
            new MemberGetterExpressionResultConverter(),
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
            return New(Ctor, parameters.Select(p => p.ResolutionExpression));
        }

        public Expression BuildExpression(TypeMapPlanBuilder builder)
        {
            if (!CanResolve)
                return null;

            var ctorArgs = CtorParams.Select(p => p.CreateExpression(builder));

            ctorArgs =
                ctorArgs.Zip(Ctor.GetParameters(),
                    (exp, pi) => exp.Type == pi.ParameterType ? exp : Convert(exp, pi.ParameterType))
                    .ToArray();
            var newExpr = New(Ctor, ctorArgs);
            return newExpr;
        }

        public void AddParameter(ParameterInfo parameter, MemberInfo[] resolvers, bool canResolve)
        {
            _ctorParams.Add(new ConstructorParameterMap(parameter, resolvers, canResolve));
        }
    }
}