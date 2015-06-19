namespace AutoMapper.QueryableExtensions.Impl
{
    using System.Linq.Expressions;
    using Internal;

    /// <summary>
    /// 
    /// </summary>
    public class CustomProjectionExpressionBinder : IExpressionBinder
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
            return propertyTypeMap?.CustomProjection != null;
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
            var visitor = new ParameterReplacementVisitor(result.ResolutionExpression);

            var replaced = visitor.Visit(propertyTypeMap.CustomProjection.Body);

            return Expression.Bind(propertyMap.DestinationProperty.MemberInfo, replaced);
        }
    }
}