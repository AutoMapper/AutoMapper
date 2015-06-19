namespace AutoMapper.QueryableExtensions.Impl
{
    using System.Linq.Expressions;
    using Internal;

    /// <summary>
    /// 
    /// </summary>
    public class AssignableExpressionBinder : IExpressionBinder
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
            return propertyMap.DestinationPropertyType.IsAssignableFrom(result.Type);
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
            ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount)
        {
            return Expression.Bind(propertyMap.DestinationProperty.MemberInfo, result.ResolutionExpression);
        }
    }
}