using AutoMapper.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IProjectionMapper
    {
        bool IsMatch(MemberMap memberMap, TypeMap memberTypeMap, Expression resolvedSource);
        Expression Project(IGlobalConfiguration configuration, MemberMap memberMap, TypeMap memberTypeMap, ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps);
    }
}