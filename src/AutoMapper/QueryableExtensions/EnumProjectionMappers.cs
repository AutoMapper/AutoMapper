using AutoMapper.Internal;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    using static Expression;
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class EnumProjectionMapper : IProjectionMapper
    {
        public Expression Project(IGlobalConfiguration configuration, IMemberMap memberMap, TypeMap memberTypeMap, ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps)
            => Convert(resolvedSource, memberMap.DestinationType);
        public bool IsMatch(IMemberMap memberMap, TypeMap memberTypeMap, Expression resolvedSource)
        {
            var types = memberMap.Types();
            return types.IsEnumToEnum() || types.IsUnderlyingTypeToEnum() || types.IsEnumToUnderlyingType();
        }
    }
}