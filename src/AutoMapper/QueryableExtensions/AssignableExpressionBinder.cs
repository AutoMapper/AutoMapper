using AutoMapper.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AssignableExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(IMemberMap memberMap, TypeMap memberTypeMap, Expression resolvedSource) 
            => memberTypeMap == null && memberMap.DestinationType.IsAssignableFrom(resolvedSource.Type);

        public Expression Build(IGlobalConfiguration configuration, IMemberMap memberMap, TypeMap memberTypeMap, ExpressionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps)
            => ExpressionFactory.ToType(resolvedSource, memberMap.DestinationType);
    }
}