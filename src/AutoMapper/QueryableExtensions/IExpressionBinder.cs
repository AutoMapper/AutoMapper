using AutoMapper.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IExpressionBinder
    {
        bool IsMatch(IMemberMap memberMap, TypeMap memberTypeMap, ExpressionResolutionResult resolvedSource);
        Expression Build(IGlobalConfiguration configuration, IMemberMap memberMap, TypeMap memberTypeMap, ExpressionRequest request, ExpressionResolutionResult resolvedSource, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps);
    }
}