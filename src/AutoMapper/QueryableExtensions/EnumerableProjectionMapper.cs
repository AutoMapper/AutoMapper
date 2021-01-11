using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.QueryableExtensions.Impl
{
    using static Expression;
    using static Execution.ExpressionBuilder;
    using static ReflectionHelper;
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class EnumerableProjectionMapper : IProjectionMapper
    {
        private static readonly MethodInfo SelectMethod = typeof(Enumerable).StaticGenericMethod("Select", parametersCount: 2);
        private static readonly MethodInfo ToArrayMethod = typeof(Enumerable).GetStaticMethod("ToArray");
        private static readonly MethodInfo ToListMethod = typeof(Enumerable).GetStaticMethod("ToList");
        public bool IsMatch(MemberMap memberMap, TypeMap memberTypeMap, Expression resolvedSource) =>
            memberMap.DestinationType.IsCollection() && memberMap.SourceType.IsCollection();
        public Expression Project(IGlobalConfiguration configuration, MemberMap memberMap, TypeMap memberTypeMap, in ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps) 
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
                var convertFunction = memberMap.DestinationType.IsArray ? ToArrayMethod : ToListMethod;
                sourceExpression = Call(convertFunction.MakeGenericMethod(destinationListType), sourceExpression);
            }
            return sourceExpression;
        }
        private static Expression Select(Expression source, LambdaExpression lambda) =>
            Call(SelectMethod.MakeGenericMethod(lambda.Parameters[0].Type, lambda.ReturnType), source, lambda);
    }
}