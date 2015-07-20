namespace AutoMapper.QueryableExtensions.Impl
{
    using System.Linq.Expressions;

    public class CustomProjectionExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyTypeMap?.CustomProjection != null;
        }

        public MemberAssignment Build(IMappingEngine mappingEngine, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            return BindCustomProjectionExpression(propertyMap, propertyTypeMap, result);
        }

        private static MemberAssignment BindCustomProjectionExpression(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            var visitor = new ParameterReplacementVisitor(result.ResolutionExpression);

            var replaced = visitor.Visit(propertyTypeMap.CustomProjection.Body);

            return Expression.Bind(propertyMap.DestinationProperty.MemberInfo, replaced);
        }
    }
}