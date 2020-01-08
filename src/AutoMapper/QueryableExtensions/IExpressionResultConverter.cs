namespace AutoMapper.QueryableExtensions
{
    public interface IExpressionResultConverter
    {
        ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, IMemberMap propertyMap, LetPropertyMaps letPropertyMaps);
        bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, IMemberMap propertyMap);
    }
}