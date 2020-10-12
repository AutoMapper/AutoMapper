using AutoMapper.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CustomProjectionExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(IMemberMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) 
            => propertyTypeMap?.CustomMapExpression != null;

        public Expression Build(IGlobalConfiguration configuration, IMemberMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps)
            => propertyTypeMap.CustomMapExpression.ReplaceParameters(result.ResolutionExpression);
    }
}