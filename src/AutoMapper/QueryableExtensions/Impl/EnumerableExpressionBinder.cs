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
                var transformedExpressions = configuration.ExpressionBuilder.CreateMapExpression(listTypePair, typePairCount);
                if(transformedExpressions == null)
                {
                    return null;
                }
                expression = Select(result.ResolutionExpression, transformedExpressions[0]);
                expression = transformedExpressions.Length == 2 ? Select(expression, transformedExpressions[1]) : expression;
            }

            expression = Expression.Call(typeof(Enumerable), propertyMap.DestinationPropertyType.IsArray ? "ToArray" : "ToList", new[] { destinationListType }, expression);

            return Expression.Bind(propertyMap.DestinationProperty, expression);
        }

        private static Expression Select(Expression source, LambdaExpression lambda)
        {
            return Expression.Call(typeof(Enumerable), "Select", new[] { lambda.Parameters[0].Type, lambda.ReturnType }, source, lambda);
        }
    }
}