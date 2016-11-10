namespace AutoMapper.QueryableExtensions.Impl
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public class AssignableExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyMap.DestinationPropertyType.IsAssignableFrom(result.Type) && propertyTypeMap == null;
        }

        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount)
        {
            return BindAssignableExpression(propertyMap, result);
        }

        private static MemberAssignment BindAssignableExpression(PropertyMap propertyMap,
            ExpressionResolutionResult result)
        {
            return Expression.Bind(propertyMap.DestinationProperty, result.ResolutionExpression);
        }
    }
}