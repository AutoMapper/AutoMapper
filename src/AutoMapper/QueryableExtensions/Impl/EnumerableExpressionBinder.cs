using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Configuration;
using AutoMapper.Mappers;
using AutoMapper.Mappers.Internal;

namespace AutoMapper.QueryableExtensions.Impl
{
    public class EnumerableExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) =>
            propertyMap.DestinationPropertyType.IsEnumerableType() && propertyMap.SourceType.IsEnumerableType() &&
            !(ElementTypeHelper.GetElementType(propertyMap.DestinationPropertyType).IsPrimitive() &&
              ElementTypeHelper.GetElementType(propertyMap.SourceType).IsPrimitive());

        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount) 
            => BindEnumerableExpression(configuration, propertyMap, request, result, typePairCount);

        private static MemberAssignment BindEnumerableExpression(IConfigurationProvider configuration, PropertyMap propertyMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount)
        {
            var destinationListType = ElementTypeHelper.GetElementType(propertyMap.DestinationPropertyType);
            var sourceListType = ElementTypeHelper.GetElementType(propertyMap.SourceType);
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