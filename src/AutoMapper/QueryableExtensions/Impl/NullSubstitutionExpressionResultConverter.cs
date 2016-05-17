namespace AutoMapper.QueryableExtensions.Impl
{
    using System;
    using System.Linq.Expressions;
    using Configuration;
    using Execution;

    public class ExplicitValueExpressionResultConverter : IExpressionResultConverter
    {
        public ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            PropertyMap propertyMap)
        {
            return new ExpressionResolutionResult(Expression.Constant(propertyMap.CustomValue), propertyMap.CustomValue.GetType());
        }

        public ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            ConstructorParameterMap propertyMap)
        {
            throw new NotImplementedException();
        }

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, PropertyMap propertyMap)
        {
            return propertyMap.CustomValue != null;
        }

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            ConstructorParameterMap propertyMap)
        {
            return false;
        }
    }
}