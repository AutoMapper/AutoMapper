using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper.Configuration;

namespace AutoMapper.QueryableExtensions
{
    using static Expression;

    internal class NullableSourceExpressionBinder : IExpressionBinder
    {
        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps)
        {
            var defaultDestination = Activator.CreateInstance(propertyMap.DestinationMemberType);
            return Bind(propertyMap.DestinationMember, Coalesce(result.ResolutionExpression, Constant(defaultDestination)));
        }

        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) =>
            result.Type.IsNullableType() && !propertyMap.DestinationMemberType.IsNullableType() && propertyMap.DestinationMemberType.IsValueType();
    }
}