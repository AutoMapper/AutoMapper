using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.QueryableExtensions.Impl
{
    using static Expression;
    internal class NullableSourceProjectionMapper : IProjectionMapper
    {
        public Expression Project(IGlobalConfiguration configuration, IMemberMap memberMap, TypeMap memberTypeMap, ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps)
        {
            var defaultDestination = Activator.CreateInstance(memberMap.DestinationType);
            return Coalesce(resolvedSource, Constant(defaultDestination));
        }
        public bool IsMatch(IMemberMap memberMap, TypeMap memberTypeMap, Expression resolvedSource) =>
            resolvedSource.Type.IsNullableType() && !memberMap.DestinationType.IsNullableType() && memberMap.DestinationType.IsValueType;
    }
}