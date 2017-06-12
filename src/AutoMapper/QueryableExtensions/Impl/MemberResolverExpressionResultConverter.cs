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
            return ExpressionResolutionResult(expressionResolutionResult, propertyMap.CustomExpression);
        }

        private static ExpressionResolutionResult ExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, LambdaExpression lambdaExpression)
        {
            var currentChild = lambdaExpression.ReplaceParameters(expressionResolutionResult.ResolutionExpression);
            var currentChildType = currentChild.Type;

            return new ExpressionResolutionResult(currentChild, currentChildType);
        }

        public ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            ConstructorParameterMap propertyMap) => ExpressionResolutionResult(expressionResolutionResult, null);

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            PropertyMap propertyMap) => propertyMap.CustomExpression != null;

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            ConstructorParameterMap propertyMap) => false;
    }
}