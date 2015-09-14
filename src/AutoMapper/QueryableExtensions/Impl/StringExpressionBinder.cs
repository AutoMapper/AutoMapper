namespace AutoMapper.QueryableExtensions.Impl
{
    using System.Linq.Expressions;
    using Internal;

    public class ToStringPropertyProjector : IPropertyProjector
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyMap.DestinationPropertyType == typeof(string);
        }

        public Expression Project(IMappingEngine mappingEngine, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount)
        {
            return BuildStringExpression(propertyMap, result);
        }

        private static Expression BuildStringExpression(PropertyMap propertyMap, ExpressionResolutionResult result)
        {
            return Expression.Call(result.ResolutionExpression, "ToString", null, null);
        }
    }
}