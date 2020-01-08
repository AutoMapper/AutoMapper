using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    public class MemberResolverExpressionResultConverter : IExpressionResultConverter
    {
        public ExpressionResolutionResult GetExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, IMemberMap propertyMap, LetPropertyMaps letPropertyMaps)
        {
            Expression subQueryMarker;
            if((subQueryMarker = letPropertyMaps.GetSubQueryMarker()) != null)
            {
                return new ExpressionResolutionResult(subQueryMarker, subQueryMarker.Type);
            }
            var currentChild = propertyMap.CustomMapExpression.ReplaceParameters(expressionResolutionResult.ResolutionExpression);
            var currentChildType = currentChild.Type;

            return new ExpressionResolutionResult(currentChild, currentChildType);
        }

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, IMemberMap propertyMap) => propertyMap.CustomMapExpression != null;
    }
}