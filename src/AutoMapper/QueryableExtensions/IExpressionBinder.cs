namespace AutoMapper.QueryableExtensions
{
    using System.Linq.Expressions;
    using Internal;

    /// <summary>
    /// Expression builders are intentionally <see cref="IMappingEngine"/> oriented as opposed to <see cref="IMapperContext"/>.
    /// </summary>
    public interface IExpressionBinder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyMap"></param>
        /// <param name="propertyTypeMap"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result);

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
        MemberAssignment Build(IMappingEngine mappingEngine, PropertyMap propertyMap,
            TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result,
            IDictionary<ExpressionRequest, int> typePairCount);
    }
}