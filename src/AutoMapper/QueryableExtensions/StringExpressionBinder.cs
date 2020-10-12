using AutoMapper.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class StringExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(IMemberMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) 
            => propertyMap.DestinationType == typeof(string);

        public Expression Build(IGlobalConfiguration configuration, IMemberMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps)
            => Expression.Call(result.ResolutionExpression, "ToString", null, null);
    }
}