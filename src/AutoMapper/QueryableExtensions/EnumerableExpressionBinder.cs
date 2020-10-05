using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.QueryableExtensions.Impl
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class EnumerableExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) =>
            propertyMap.DestinationType.IsEnumerableType() && propertyMap.SourceType.IsEnumerableType();

        public MemberAssignment Build(IGlobalConfiguration configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps) 
            => BindEnumerableExpression(configuration, propertyMap, request, result, typePairCount, letPropertyMaps);

        private static MemberAssignment BindEnumerableExpression(IGlobalConfiguration configuration, PropertyMap propertyMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps)
        {
            var destinationListType = ElementTypeHelper.GetElementType(propertyMap.DestinationType);
            var sourceListType = ElementTypeHelper.GetElementType(propertyMap.SourceType);
            var expression = result.ResolutionExpression;

            if (sourceListType != destinationListType)
            {
                var listTypePair = new ExpressionRequest(sourceListType, destinationListType, request.MembersToExpand, request);
                var transformedExpressions = configuration.ExpressionBuilder.CreateMapExpression(listTypePair, typePairCount, letPropertyMaps.New());
                if(transformedExpressions.Empty)
                {
                    return null;
                }
                expression = transformedExpressions.Chain(expression, Select);
            }
            if (!propertyMap.DestinationType.IsAssignableFrom(expression.Type))
            {
                var convertFunction = propertyMap.DestinationType.IsArray ? nameof(Enumerable.ToArray) : nameof(Enumerable.ToList);
                expression = Expression.Call(typeof(Enumerable), convertFunction, new[] { destinationListType }, expression);
            }
            return Expression.Bind(propertyMap.DestinationMember, expression);
        }

        private static Expression Select(Expression source, LambdaExpression lambda)
        {
            return Expression.Call(typeof(Enumerable), nameof(Enumerable.Select), new[] { lambda.Parameters[0].Type, lambda.ReturnType }, source, lambda);
        }
    }
}