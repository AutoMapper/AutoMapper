using System.Linq.Expressions;
using AutoMapper.Internal;
namespace AutoMapper.QueryableExtensions.Impl
{
    using static Expression;
    internal class NullableSourceProjectionMapper : IProjectionMapper
    {
        public Expression Project(IGlobalConfiguration configuration, MemberMap memberMap, TypeMap memberTypeMap, ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps) =>
            Coalesce(resolvedSource, New(memberMap.DestinationType));
        public bool IsMatch(MemberMap memberMap, TypeMap memberTypeMap, Expression resolvedSource) =>
            memberMap.DestinationType.IsValueType && !memberMap.DestinationType.IsNullableType() && resolvedSource.Type.IsNullableType();
    }
}