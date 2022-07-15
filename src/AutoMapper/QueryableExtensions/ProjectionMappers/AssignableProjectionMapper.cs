using AutoMapper.Execution;
using AutoMapper.Internal;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AssignableProjectionMapper : IProjectionMapper
    {
        public bool IsMatch(TypePair context) => context.DestinationType.IsAssignableFrom(context.SourceType);
        public Expression Project(IGlobalConfiguration configuration, MemberMap memberMap, TypeMap memberTypeMap, in ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps)
            => ExpressionBuilder.ToType(resolvedSource, memberMap.DestinationType);
    }
}