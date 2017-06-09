using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.QueryableExtensions.Impl
{
    public class MemberGetterExpressionResultConverter : IExpressionResultConverter
    {
        public ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, PropertyMap propertyMap, LetPropertyMaps letPropertyMaps) 
            => ExpressionResolutionResult(expressionResolutionResult, propertyMap.SourceMembers);

        public ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult,
            ConstructorParameterMap propertyMap) 
            => ExpressionResolutionResult(expressionResolutionResult, propertyMap.SourceMembers);

        private static ExpressionResolutionResult ExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, IEnumerable<MemberInfo> sourceMembers) 
            => sourceMembers.Aggregate(expressionResolutionResult, ExpressionResolutionResult);

        private static ExpressionResolutionResult ExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, MemberInfo getter)
        {
            var member = Expression.MakeMemberAccess(expressionResolutionResult.ResolutionExpression, getter);
            return new ExpressionResolutionResult(member, member.Type);
        }

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, PropertyMap propertyMap) 
            => propertyMap.SourceMembers.Any();

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, ConstructorParameterMap propertyMap) 
            => propertyMap.SourceMembers.Any();
    }
}