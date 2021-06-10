using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.QueryableExtensions
{
    using static Expression;

    internal class NullableSourceExpressionBinder : IExpressionBinder
    {
        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps)
        {
            return Bind(propertyMap.DestinationMember, Coalesce(result.ResolutionExpression, New(propertyMap.DestinationType)));
        }

        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) =>
            result.Type.IsNullableType() && !propertyMap.DestinationType.IsNullableType() && propertyMap.DestinationType.IsValueType;
    }
}