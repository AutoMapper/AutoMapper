namespace AutoMapper.QueryableExtensions.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;    

    public class MemberGetterExpressionResultConverter : IExpressionResultConverter
    {
        public ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, PropertyMap propertyMap)
        {
            return ExpressionResolutionResult(expressionResolutionResult, propertyMap.SourceMembers);
        }

        public ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            ConstructorParameterMap propertyMap)
        {
            return ExpressionResolutionResult(expressionResolutionResult, propertyMap.SourceMembers);
        }

        private static ExpressionResolutionResult ExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, IEnumerable<IMemberGetter> sourceMembers)
        {
            return sourceMembers.Aggregate(expressionResolutionResult, ExpressionResolutionResult);
        }

        private static ExpressionResolutionResult ExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, IMemberGetter getter)
        {
            Expression currentChild = expressionResolutionResult.ResolutionExpression;
            Type currentChildType;
            var memberInfo = getter.MemberInfo;

            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                currentChild = Expression.Property(currentChild, propertyInfo);
                currentChildType = propertyInfo.PropertyType;
            }
            else
                currentChildType = currentChild.Type;

            return new ExpressionResolutionResult(currentChild, currentChildType);
        }

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, PropertyMap propertyMap)
        {
            return propertyMap.SourceMembers.Any();
        }

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, ConstructorParameterMap propertyMap)
        {
            return propertyMap.SourceMembers.Any() && propertyMap.CustomExpression == null;
        }
    }
}