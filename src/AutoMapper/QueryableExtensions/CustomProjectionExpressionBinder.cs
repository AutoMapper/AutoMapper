using AutoMapper.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CustomProjectionExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) 
            => propertyTypeMap?.CustomMapExpression != null;

        public MemberAssignment Build(IGlobalConfiguration configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps)
            => Expression.Bind(propertyMap.DestinationMember, propertyTypeMap.CustomMapExpression.ReplaceParameters(result.ResolutionExpression));
    }
}