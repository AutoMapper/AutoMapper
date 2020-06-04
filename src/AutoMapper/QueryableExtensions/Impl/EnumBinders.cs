using AutoMapper.Internal;
using AutoMapper.Mappers.Internal;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    public class EnumToUnderlyingTypeBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
            => ElementTypeHelper.IsEnumToUnderlyingType(propertyMap.Types);

        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps)
            => Expression.Bind(propertyMap.DestinationMember, ExpressionFactory.ToType(result.ResolutionExpression, propertyMap.DestinationType));
    }
}