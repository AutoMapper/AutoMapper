using AutoMapper.Internal;
using System.ComponentModel;
using System.Linq.Expressions;
namespace AutoMapper.QueryableExtensions.Impl
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class StringProjectionMapper : IProjectionMapper
    {
        public bool IsMatch(MemberMap memberMap, TypeMap memberTypeMap, Expression resolvedSource) => memberMap.DestinationType == typeof(string);
        public Expression Project(IGlobalConfiguration configuration, MemberMap memberMap, TypeMap memberTypeMap, ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps)
            => Expression.Call(resolvedSource, ExpressionFactory.ObjectToString);
    }
}