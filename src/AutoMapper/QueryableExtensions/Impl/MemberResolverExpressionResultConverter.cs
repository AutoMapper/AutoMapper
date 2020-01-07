using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    public class MemberResolverExpressionResultConverter : IExpressionResultConverter
    {
        public ExpressionResolutionResult GetExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, PropertyMap propertyMap, LetPropertyMaps letPropertyMaps)
        {
            Expression subQueryMarker;
            if((subQueryMarker = letPropertyMaps.GetSubQueryMarker()) != null)
            {
                return new ExpressionResolutionResult(subQueryMarker, subQueryMarker.Type);
            }
            return ExpressionResolutionResult(expressionResolutionResult, propertyMap.CustomMapExpression);
        }

        private static ExpressionResolutionResult ExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, LambdaExpression lambdaExpression)
        {
            var currentChild = lambdaExpression.ReplaceParameters(expressionResolutionResult.ResolutionExpression);
            var currentChildType = currentChild.Type;

            return new ExpressionResolutionResult(currentChild, currentChildType);
        }

        public ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            ConstructorParameterMap parameterMap) => ExpressionResolutionResult(expressionResolutionResult, parameterMap.CustomMapExpression);

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            PropertyMap propertyMap) => propertyMap.CustomMapExpression != null;

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            ConstructorParameterMap parameterMap) => parameterMap.CustomMapExpression != null;
    }
}