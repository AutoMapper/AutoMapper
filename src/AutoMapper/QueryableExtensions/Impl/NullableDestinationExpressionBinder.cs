using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;
using AutoMapper.Configuration;
using AutoMapper.Internal;

namespace AutoMapper.QueryableExtensions.Impl
{
    public class NullableDestinationExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) =>
            propertyMap.DestinationType.IsNullableType() && !result.Type.IsNullableType();

        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps) 
            => BindNullableExpression(propertyMap, result);

        private static MemberAssignment BindNullableExpression(PropertyMap propertyMap,
            ExpressionResolutionResult result)
        {
            var destinationType = propertyMap.DestinationType;
            var expressionToBind =
                result.ResolutionExpression.GetMemberExpressions().Reverse().Aggregate(
                    ExpressionFactory.ToType(result.ResolutionExpression, destinationType),
                    (accumulator, current) => current.IfNullElse(Constant(null, destinationType), accumulator));
            return Bind(propertyMap.DestinationMember, expressionToBind);
        }
    }
}