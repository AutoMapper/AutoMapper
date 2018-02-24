using AutoMapper.Internal;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    public class ImplicitAssignableExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) 
            => propertyMap.DestinationPropertyType.CanImplicitConvert(result.Type) && propertyTypeMap == null;

        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps) 
            => BindAssignableExpression(propertyMap, result);

        private static MemberAssignment BindAssignableExpression(PropertyMap propertyMap, ExpressionResolutionResult result) 
            => Expression.Bind(propertyMap.DestinationProperty, Expression.Convert(result.ResolutionExpression, propertyMap.DestinationPropertyType));
    }
}