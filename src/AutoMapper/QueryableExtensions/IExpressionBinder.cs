namespace AutoMapper.QueryableExtensions
{
    using System.Linq.Expressions;

    public interface IPropertyProjector
    {
        bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result);

        Expression Project(IMappingEngine mappingEngine, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, Internal.IDictionary<ExpressionRequest, int> typePairCount);
    }
}