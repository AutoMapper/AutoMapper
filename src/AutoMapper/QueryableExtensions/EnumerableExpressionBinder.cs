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
        public bool IsMatch(IMemberMap memberMap, TypeMap memberTypeMap, ExpressionResolutionResult resolvedSource) =>
            memberMap.DestinationType.IsEnumerableType() && memberMap.SourceType.IsEnumerableType();

        public Expression Build(IGlobalConfiguration configuration, IMemberMap memberMap, TypeMap memberTypeMap, ExpressionRequest request, ExpressionResolutionResult resolvedSource, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps) 
        {
            var destinationListType = ElementTypeHelper.GetElementType(memberMap.DestinationType);
            var sourceListType = ElementTypeHelper.GetElementType(memberMap.SourceType);
            var sourceExpression = resolvedSource.ResolutionExpression;

            if (sourceListType != destinationListType)
            {
                var listTypePair = new ExpressionRequest(sourceListType, destinationListType, request.MembersToExpand, request);
                var transformedExpressions = configuration.ExpressionBuilder.CreateMapExpression(listTypePair, typePairCount, letPropertyMaps.New());
                if(transformedExpressions.Empty)
                {
                    return null;
                }
                sourceExpression = transformedExpressions.Chain(sourceExpression, Select);
            }
            if (!memberMap.DestinationType.IsAssignableFrom(sourceExpression.Type))
            {
                var convertFunction = memberMap.DestinationType.IsArray ? nameof(Enumerable.ToArray) : nameof(Enumerable.ToList);
                sourceExpression = Expression.Call(typeof(Enumerable), convertFunction, new[] { destinationListType }, sourceExpression);
            }
            return sourceExpression;
        }

        private static Expression Select(Expression source, LambdaExpression lambda) =>
            Expression.Call(typeof(Enumerable), nameof(Enumerable.Select), new[] { lambda.Parameters[0].Type, lambda.ReturnType }, source, lambda);
    }
}