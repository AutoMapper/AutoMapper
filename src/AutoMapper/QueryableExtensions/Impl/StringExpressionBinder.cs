using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    public class StringExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) 
            => propertyMap.DestinationPropertyType == typeof(string);

        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps) 
            => BindStringExpression(propertyMap, result);

        private static MemberAssignment BindStringExpression(PropertyMap propertyMap, ExpressionResolutionResult result)
            => Expression.Bind(propertyMap.DestinationProperty, Expression.Call(result.ResolutionExpression, "ToString", null, null));
    }
}