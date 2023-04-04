﻿namespace AutoMapper.QueryableExtensions.Impl;
internal class NullableSourceProjectionMapper : IProjectionMapper
{
    public Expression Project(IGlobalConfiguration configuration, in ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps) =>
        Coalesce(resolvedSource, New(request.DestinationType));
    public bool IsMatch(TypePair context) =>
        !context.DestinationType.CanAssignNull() && context.SourceType.IsNullableType();
}