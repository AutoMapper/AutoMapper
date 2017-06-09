using System.Collections.Generic;
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;
using AutoMapper.Configuration;

namespace AutoMapper.QueryableExtensions.Impl
{
    public class NullableDestinationExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) =>
            propertyMap.DestinationPropertyType.IsNullableType()
            && !result.Type.IsNullableType();

        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps) 
            => BindNullableExpression(propertyMap, result);

        private static MemberAssignment BindNullableExpression(PropertyMap propertyMap,
            ExpressionResolutionResult result)
        {
            if (result.ResolutionExpression.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = (MemberExpression) result.ResolutionExpression;
                if (memberExpr.Expression != null && memberExpr.Expression.NodeType == ExpressionType.MemberAccess)
                {
                    var destType = propertyMap.DestinationPropertyType;
                    var parentExpr = memberExpr.Expression;
                    Expression expressionToBind = Convert(memberExpr, destType);
                    var nullExpression = Convert(Constant(null), destType);
                    while (parentExpr.NodeType != ExpressionType.Parameter)
                    {
                        memberExpr = (MemberExpression) memberExpr.Expression;
                        parentExpr = memberExpr.Expression;
                        expressionToBind = Condition(
                            Equal(memberExpr, Constant(null)),
                            nullExpression,
                            expressionToBind
                            );
                    }

                    return Bind(propertyMap.DestinationProperty, expressionToBind);
                }
            }

            return Bind(propertyMap.DestinationProperty,
                Convert(result.ResolutionExpression, propertyMap.DestinationPropertyType));
        }
    }
}