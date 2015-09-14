namespace AutoMapper.QueryableExtensions.Impl
{
    using System.Linq.Expressions;
    using Internal;

    public class MappedTypePropertyProjector : IPropertyProjector
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyTypeMap != null;
        }

        public Expression Project(IMappingEngine mappingEngine, PropertyMap propertyMap, TypeMap propertyTypeMap,
            ExpressionRequest request, ExpressionResolutionResult result, Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            return BuildMappedTypeExpression(mappingEngine, propertyMap, request, result, typePairCount);
        }

        private static Expression BuildMappedTypeExpression(IMappingEngine mappingEngine, PropertyMap propertyMap,
            ExpressionRequest request, ExpressionResolutionResult result, Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            var transformedExpression = ((IMappingEngineRunner)mappingEngine).CreateMapExpression(request, result.ResolutionExpression, typePairCount);

            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //BELOW SHOULDN'T BE NEEDED WHEN NULL GUARDS ARE PUT IN BY MAPPINGENGINE...

            // Handles null source property so it will not create an object with possible non-nullable propeerties 
            // which would result in an exception.
            if (mappingEngine.ConfigurationProvider.MapNullSourceValuesAsNull)
            {
                var expressionNull = Expression.Constant(null, propertyMap.DestinationPropertyType);
                transformedExpression =
                    Expression.Condition(Expression.NotEqual(Expression.TypeAs(result.ResolutionExpression, typeof(object)), Expression.Constant(null)),
                        transformedExpression, expressionNull);
            }

            return transformedExpression;
        }
    }
}