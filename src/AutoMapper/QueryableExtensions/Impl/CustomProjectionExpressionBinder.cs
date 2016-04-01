using System.Linq;

namespace AutoMapper.QueryableExtensions.Impl
{
    using System.Collections.Concurrent;
    using System.Linq.Expressions;

    public class CustomProjectionExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyTypeMap?.CustomProjection != null;
        }

        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, ConcurrentDictionary<ExpressionRequest, int> typePairCount)
        {
            return BindCustomProjectionExpression(propertyMap, propertyTypeMap, result);
        }

        private static MemberAssignment BindCustomProjectionExpression(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            var visitor = new ParameterConversionVisitor(result.ResolutionExpression, propertyTypeMap.CustomProjection.Parameters.FirstOrDefault());

            var replaced = visitor.Visit(propertyTypeMap.CustomProjection.Body);

            return Expression.Bind(propertyMap.DestinationProperty.MemberInfo, replaced);
        }
    }
}