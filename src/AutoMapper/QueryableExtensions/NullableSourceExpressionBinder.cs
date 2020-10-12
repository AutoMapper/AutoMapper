using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.QueryableExtensions.Impl
{
    using static Expression;
    internal class NullableSourceExpressionBinder : IExpressionBinder
    {
        public Expression Build(IGlobalConfiguration configuration, IMemberMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps)
        {
            var defaultDestination = Activator.CreateInstance(propertyMap.DestinationType);
            return Coalesce(result.ResolutionExpression, Constant(defaultDestination));
        }
        public bool IsMatch(IMemberMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) =>
            result.Type.IsNullableType() && !propertyMap.DestinationType.IsNullableType() && propertyMap.DestinationType.IsValueType;
    }
}