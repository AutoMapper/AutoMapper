using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    using Configuration;
    using Mappers;

    public class EnumerableExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyMap.DestinationPropertyType.IsEnumerableType() && propertyMap.SourceType.IsEnumerableType() &&
                    !(TypeHelper.GetElementType(propertyMap.DestinationPropertyType).IsPrimitive() && TypeHelper.GetElementType(propertyMap.SourceType).IsPrimitive());
        }

        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount)
        {
            return BindEnumerableExpression(configuration, propertyMap, request, result, typePairCount);
        }

        private static MemberAssignment BindEnumerableExpression(IConfigurationProvider configuration, PropertyMap propertyMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount)
        {
            var destinationListType = TypeHelper.GetElementType(propertyMap.DestinationPropertyType);
            var sourceListType = TypeHelper.GetElementType(propertyMap.SourceType);
            var expression = result.ResolutionExpression;

            if (sourceListType != destinationListType)
            {
                var listTypePair = new ExpressionRequest(sourceListType, destinationListType, request.MembersToExpand, request);
                var transformedExpression = configuration.ExpressionBuilder.CreateMapExpression(listTypePair, typePairCount);
                if(transformedExpression == null)
                {
                    return null;
                }
                expression = Expression.Call(typeof (Enumerable), "Select", new[] {sourceListType, destinationListType}, result.ResolutionExpression, transformedExpression);
            }

            expression = Expression.Call(typeof(Enumerable), propertyMap.DestinationPropertyType.IsArray ? "ToArray" : "ToList", new[] { destinationListType }, expression);

            return Expression.Bind(propertyMap.DestinationProperty, expression);
        }
    }
}