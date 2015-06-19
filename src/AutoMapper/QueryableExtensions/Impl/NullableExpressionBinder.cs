namespace AutoMapper.QueryableExtensions.Impl
{
    using System.Linq.Expressions;
    using Internal;

    /// <summary>
    /// 
    /// </summary>
    public class NullableExpressionBinder : IExpressionBinder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyMap"></param>
        /// <param name="propertyTypeMap"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            return propertyMap.DestinationPropertyType.IsNullableType()
                   && !result.Type.IsNullableType();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mappingEngine"></param>
        /// <param name="propertyMap"></param>
        /// <param name="propertyTypeMap"></param>
        /// <param name="request"></param>
        /// <param name="result"></param>
        /// <param name="typePairCount"></param>
        /// <returns></returns>
        public MemberAssignment Build(IMappingEngine mappingEngine, PropertyMap propertyMap, TypeMap propertyTypeMap,
            ExpressionRequest request, ExpressionResolutionResult result,
            IDictionary<ExpressionRequest, int> typePairCount)
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