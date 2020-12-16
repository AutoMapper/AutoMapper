using System;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.QueryableExtensions.Impl
{
    using static Expression;
    internal class NullableSourceProjectionMapper : IProjectionMapper
    {
        public Expression Project(IGlobalConfiguration configuration, MemberMap memberMap, TypeMap memberTypeMap, ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps)
        {
            var defaultDestination = Activator.CreateInstance(memberMap.DestinationType);
            return Coalesce(resolvedSource, Constant(defaultDestination));
        }
        public bool IsMatch(MemberMap memberMap, TypeMap memberTypeMap, Expression resolvedSource) =>
            resolvedSource.Type.IsNullableType() && !memberMap.DestinationType.IsNullableType() && memberMap.DestinationType.IsValueType;
    }
}