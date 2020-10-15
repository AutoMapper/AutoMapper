using AutoMapper.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class StringExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(IMemberMap memberMap, TypeMap memberTypeMap, ExpressionResolutionResult resolvedSource) 
            => memberMap.DestinationType == typeof(string);

        public Expression Build(IGlobalConfiguration configuration, IMemberMap memberMap, TypeMap memberTypeMap, ExpressionRequest request, ExpressionResolutionResult resolvedSource, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps)
            => Expression.Call(resolvedSource.ResolutionExpression, "ToString", null, null);
    }
}