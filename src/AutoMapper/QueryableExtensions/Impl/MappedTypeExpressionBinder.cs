namespace AutoMapper.QueryableExtensions.Impl
{
    using System.Linq.Expressions;
    using Internal;

    public class MappedTypeExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyTypeMap != null && propertyTypeMap.CustomProjection == null;
        }

        public MemberAssignment Build(IMappingEngine mappingEngine, PropertyMap propertyMap, TypeMap propertyTypeMap,
            ExpressionRequest request, ExpressionResolutionResult result, Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            return BindMappedTypeExpression(mappingEngine, propertyMap, request, result, typePairCount);
        }

        private static MemberAssignment BindMappedTypeExpression(IMappingEngine mappingEngine, PropertyMap propertyMap,
            ExpressionRequest request, ExpressionResolutionResult result, Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            var transformedExpression = ((IMappingEngineRunner)mappingEngine).CreateMapExpression(request, result.ResolutionExpression, typePairCount);

            // Handles null source property so it will not create an object with possible non-nullable propeerties 
            // which would result in an exception.
            if (mappingEngine.ConfigurationProvider.MapNullSourceValuesAsNull)
            {
                var expressionNull = Expression.Constant(null, propertyMap.DestinationPropertyType);
                transformedExpression =
                    Expression.Condition(Expression.NotEqual(result.ResolutionExpression, Expression.Constant(null)),
                        transformedExpression, expressionNull);
            }

            return Expression.Bind(propertyMap.DestinationProperty.MemberInfo, transformedExpression);
        }
    }
}