using System.Collections.Concurrent;

namespace AutoMapper.QueryableExtensions.Impl
{
    using System.Linq.Expressions;
    using Internal;

    public class StringExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyMap.DestinationPropertyType == typeof(string);
        }

        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, ConcurrentDictionary<ExpressionRequest, int> typePairCount)
        {
            return BindStringExpression(propertyMap, result);
        }

        private static MemberAssignment BindStringExpression(PropertyMap propertyMap, ExpressionResolutionResult result)
        {
            return Expression.Bind(propertyMap.DestinationProperty.MemberInfo, Expression.Call(result.ResolutionExpression, "ToString", null, null));
        }
    }
}