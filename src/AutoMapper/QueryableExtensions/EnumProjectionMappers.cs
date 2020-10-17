using AutoMapper.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    using static Expression;
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class EnumProjectionMapper : IProjectionMapper
    {
        public Expression Project(IGlobalConfiguration configuration, IMemberMap memberMap, TypeMap memberTypeMap, ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps)
            => Convert(resolvedSource, memberMap.DestinationType);
        public abstract bool IsMatch(IMemberMap memberMap, TypeMap memberTypeMap, Expression resolvedSource);
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class EnumToUnderlyingTypeProjectionMapper : EnumProjectionMapper
    {
        public override bool IsMatch(IMemberMap memberMap, TypeMap memberTypeMap, Expression resolvedSource) => memberMap.Types.IsEnumToUnderlyingType();
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UnderlyingTypeToEnumProjectionMapper : EnumProjectionMapper
    {
        public override bool IsMatch(IMemberMap memberMap, TypeMap memberTypeMap, Expression resolvedSource) => memberMap.Types.IsUnderlyingTypeToEnum();
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class EnumToEnumProjectionMapper : EnumProjectionMapper
    {
        public override bool IsMatch(IMemberMap memberMap, TypeMap memberTypeMap, Expression resolvedSource) => memberMap.Types.IsEnumToEnum();
    }
}