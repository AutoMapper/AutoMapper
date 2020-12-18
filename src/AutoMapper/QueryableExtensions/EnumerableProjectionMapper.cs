using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.QueryableExtensions.Impl
{
    using static ExpressionFactory;
    using static ReflectionHelper;
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class EnumerableProjectionMapper : IProjectionMapper
    {
        public bool IsMatch(MemberMap memberMap, TypeMap memberTypeMap, Expression resolvedSource) =>
            memberMap.DestinationType.IsEnumerableType() && memberMap.SourceType.IsEnumerableType();
        public Expression Project(IGlobalConfiguration configuration, MemberMap memberMap, TypeMap memberTypeMap, ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps) 
        {
            var destinationListType = GetElementType(memberMap.DestinationType);
            var sourceListType = GetElementType(memberMap.SourceType);
            var sourceExpression = resolvedSource;
            if (sourceListType != destinationListType)
            {
                var listTypePair = new ProjectionRequest(sourceListType, destinationListType, request.MembersToExpand, request.GetPreviousRequestsAndSelf());
                var transformedExpressions = configuration.ProjectionBuilder.CreateProjection(listTypePair, letPropertyMaps.New());
                if(transformedExpressions.Empty)
                {
                    return null;
                }
                sourceExpression = transformedExpressions.Chain(sourceExpression, Select);
            }
            if (!memberMap.DestinationType.IsAssignableFrom(sourceExpression.Type))
            {
                var convertFunction = memberMap.DestinationType.IsArray ? nameof(Enumerable.ToArray) : nameof(Enumerable.ToList);
                sourceExpression = Call(typeof(Enumerable), convertFunction, new[] { destinationListType }, sourceExpression);
            }
            return sourceExpression;
        }
        private static Expression Select(Expression source, LambdaExpression lambda) =>
            Call(typeof(Enumerable), nameof(Enumerable.Select), new[] { lambda.Parameters[0].Type, lambda.ReturnType }, source, lambda);
    }
}