using System.Collections.Concurrent;

namespace AutoMapper.QueryableExtensions.Impl
{
    using System.Linq.Expressions;

    public class MappedTypeExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyTypeMap != null && propertyTypeMap.CustomProjection == null;
        }

        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, ConcurrentDictionary<ExpressionRequest, int> typePairCount)
        {
            return BindMappedTypeExpression(configuration, propertyMap, request, result, typePairCount);
        }

        private static MemberAssignment BindMappedTypeExpression(IConfigurationProvider configuration, PropertyMap propertyMap, ExpressionRequest request, ExpressionResolutionResult result, ConcurrentDictionary<ExpressionRequest, int> typePairCount)
        {
            var transformedExpression = configuration.ExpressionBuilder.CreateMapExpression(request, result.ResolutionExpression, typePairCount);
            if(transformedExpression == null)
            {
                return null;
            }
            // Handles null source property so it will not create an object with possible non-nullable propeerties 
            // which would result in an exception.
            if (configuration.AllowNullDestinationValues)
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