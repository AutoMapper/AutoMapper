using AutoMapper.Internal;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    using static Expression;
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class EnumProjectionMapper : IProjectionMapper
    {
        public Expression Project(IGlobalConfiguration configuration, in ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps)
            => Convert(resolvedSource, request.DestinationType);
        public bool IsMatch(TypePair context) => context.IsEnumToEnum() || context.IsUnderlyingTypeToEnum() || context.IsEnumToUnderlyingType();
    }
}