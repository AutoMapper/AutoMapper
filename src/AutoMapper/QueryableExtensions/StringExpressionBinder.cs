using AutoMapper.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class StringExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) 
            => propertyMap.DestinationType == typeof(string);

        public MemberAssignment Build(IGlobalConfiguration configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps)
            => Expression.Bind(propertyMap.DestinationMember, Expression.Call(result.ResolutionExpression, "ToString", null, null));
    }
}