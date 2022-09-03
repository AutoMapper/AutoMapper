using System.Linq.Expressions;
using AutoMapper.Internal;
namespace AutoMapper.QueryableExtensions.Impl;

using static Expression;
internal class NullableSourceProjectionMapper : IProjectionMapper
{
    public Expression Project(IGlobalConfiguration configuration, in ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps) =>
        Coalesce(resolvedSource, New(request.DestinationType));
    public bool IsMatch(TypePair context) =>
        context.DestinationType.IsValueType && !context.DestinationType.IsNullableType() && context.SourceType.IsNullableType();
}