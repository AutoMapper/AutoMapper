namespace AutoMapper.QueryableExtensions.Impl
{
    using System.Linq.Expressions;
    using Internal;

    public class NullableExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyMap.DestinationPropertyType.IsNullableType()
                   && !result.Type.IsNullableType();
        }

        public MemberAssignment Build(IMappingEngine mappingEngine, PropertyMap propertyMap, TypeMap propertyTypeMap,
            ExpressionRequest request, ExpressionResolutionResult result,
            IDictionary<ExpressionRequest, int> typePairCount)
        {
            return BindNullableExpression(propertyMap, result);
        }

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
                    Expression expressionToBind = Expression.Convert(memberExpr, destType);
                    var nullExpression = Expression.Convert(Expression.Constant(null), destType);
                    while (parentExpr.NodeType != ExpressionType.Parameter)
                    {
                        memberExpr = (MemberExpression) memberExpr.Expression;
                        parentExpr = memberExpr.Expression;
                        expressionToBind = Expression.Condition(
                            Expression.Equal(memberExpr, Expression.Constant(null)),
                            nullExpression,
                            expressionToBind
                            );
                    }

                    return Expression.Bind(propertyMap.DestinationProperty.MemberInfo, expressionToBind);
                }
            }

            return Expression.Bind(propertyMap.DestinationProperty.MemberInfo,
                Expression.Convert(result.ResolutionExpression, propertyMap.DestinationPropertyType));
        }
    }
}