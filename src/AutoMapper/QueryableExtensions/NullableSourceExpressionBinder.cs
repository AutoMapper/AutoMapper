using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.QueryableExtensions.Impl
{
    using static Expression;
    internal class NullableSourceExpressionBinder : IExpressionBinder
    {
        public Expression Build(IGlobalConfiguration configuration, IMemberMap memberMap, TypeMap memberTypeMap, ExpressionRequest request, ExpressionResolutionResult resolvedSource, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps)
        {
            var defaultDestination = Activator.CreateInstance(memberMap.DestinationType);
            return Coalesce(resolvedSource.ResolutionExpression, Constant(defaultDestination));
        }
        public bool IsMatch(IMemberMap memberMap, TypeMap memberTypeMap, ExpressionResolutionResult resolvedSource) =>
            resolvedSource.Type.IsNullableType() && !memberMap.DestinationType.IsNullableType() && memberMap.DestinationType.IsValueType;
    }
}