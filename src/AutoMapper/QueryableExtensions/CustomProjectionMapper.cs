using AutoMapper.Internal;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CustomProjectionMapper : IProjectionMapper
    {
        public bool IsMatch(MemberMap memberMap, TypeMap memberTypeMap, Expression resolvedSource) => memberTypeMap?.CustomMapExpression != null;
        public Expression Project(IGlobalConfiguration configuration, MemberMap memberMap, TypeMap memberTypeMap, ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps)
            => memberTypeMap.CustomMapExpression.ReplaceParameters(resolvedSource);
    }
}