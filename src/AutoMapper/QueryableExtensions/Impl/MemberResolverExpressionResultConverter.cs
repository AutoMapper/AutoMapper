namespace AutoMapper.QueryableExtensions.Impl
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class MemberResolverExpressionResultConverter : IExpressionResultConverter
    {
        public ExpressionResolutionResult GetExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, PropertyMap propertyMap)
        {
            return ExpressionResolutionResult(expressionResolutionResult, propertyMap.CustomExpression);
        }

        private static ExpressionResolutionResult ExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, LambdaExpression lambdaExpression)
        {
            Expression currentChild = lambdaExpression.ReplaceParameters(expressionResolutionResult.ResolutionExpression);
            Type currentChildType = currentChild.Type;

            return new ExpressionResolutionResult(currentChild, currentChildType);
        }

        public ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            ConstructorParameterMap propertyMap)
        {
            return ExpressionResolutionResult(expressionResolutionResult, null);
        }

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            PropertyMap propertyMap)
        {
            return propertyMap.CustomExpression != null;
        }

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            ConstructorParameterMap propertyMap)
        {
            return false;
        }
    }

    internal class ParameterConversionVisitor : ExpressionVisitor
    {
        private readonly Expression newParameter;
        private readonly ParameterExpression oldParameter;

        public ParameterConversionVisitor(Expression newParameter, ParameterExpression oldParameter)
        {
            this.newParameter = newParameter;
            this.oldParameter = oldParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            // replace all old param references with new ones
            return node == oldParameter ? newParameter : node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != oldParameter) // if instance is not old parameter - do nothing
                return base.VisitMember(node);

            var newObj = Visit(node.Expression);
            var newMember = newParameter.Type.GetMember(node.Member.Name).First();
            return Expression.MakeMemberAccess(newObj, newMember);
        }
    }
}