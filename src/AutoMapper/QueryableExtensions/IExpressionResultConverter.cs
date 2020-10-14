namespace AutoMapper.QueryableExtensions.Impl
{
    public interface IExpressionResultConverter
    {
        ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, IMemberMap memberMap, LetPropertyMaps letPropertyMaps);
        bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, IMemberMap memberMap);
    }
}