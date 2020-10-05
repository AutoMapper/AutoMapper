using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.QueryableExtensions.Impl
{
    using static Expression;

    internal class NullableSourceExpressionBinder : IExpressionBinder
    {
        public MemberAssignment Build(IGlobalConfiguration configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps)
        {
            var defaultDestination = Activator.CreateInstance(propertyMap.DestinationType);
            return Bind(propertyMap.DestinationMember, Coalesce(result.ResolutionExpression, Constant(defaultDestination)));
        }

        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) =>
            result.Type.IsNullableType() && !propertyMap.DestinationType.IsNullableType() && propertyMap.DestinationType.IsValueType;
    }
}