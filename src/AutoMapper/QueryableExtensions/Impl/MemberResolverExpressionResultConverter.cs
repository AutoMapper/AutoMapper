using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    public class MemberResolverExpressionResultConverter : IExpressionResultConverter
    {
        public ExpressionResolutionResult GetExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, PropertyMap propertyMap) 
            => ExpressionResolutionResult(expressionResolutionResult, propertyMap.CustomExpression);

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

    internal class ParameterConversionVisitor : ExpressionVisitor
    {
        private readonly Expression _newParameter;
        private readonly ParameterExpression _oldParameter;

        public ParameterConversionVisitor(Expression newParameter, ParameterExpression oldParameter)
        {
            _newParameter = newParameter;
            _oldParameter = oldParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node) => node == _oldParameter ? _newParameter : node;

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != _oldParameter) // if instance is not old parameter - do nothing
                return base.VisitMember(node);

            var newObj = Visit(node.Expression);
            var newMember = _newParameter.Type.GetMember(node.Member.Name).First();
            return Expression.MakeMemberAccess(newObj, newMember);
        }
    }
}