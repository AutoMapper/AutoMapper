using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    public class MappedTypeExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) => 
            propertyTypeMap != null && propertyTypeMap.CustomProjection == null;

        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps) 
            => BindMappedTypeExpression(configuration, propertyMap, request, result, typePairCount, letPropertyMaps);

        private static MemberAssignment BindMappedTypeExpression(IConfigurationProvider configuration, PropertyMap propertyMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps)
        {
            var transformedExpression = configuration.ExpressionBuilder.CreateMapExpression(request, result.ResolutionExpression, typePairCount, letPropertyMaps);
            if(transformedExpression == null)
            {
                return null;
            }
            // Handles null source property so it will not create an object with possible non-nullable propeerties 
            // which would result in an exception.
            if (propertyMap.TypeMap.Profile.AllowNullDestinationValues && !propertyMap.AllowNull)
            {
                var expressionNull = Expression.Constant(null, propertyMap.DestinationPropertyType);
                transformedExpression =
                    Expression.Condition(Expression.NotEqual(result.ResolutionExpression, Expression.Constant(null)),
                        transformedExpression, expressionNull);
            }

            return Expression.Bind(propertyMap.DestinationProperty, transformedExpression);
        }
    }
}