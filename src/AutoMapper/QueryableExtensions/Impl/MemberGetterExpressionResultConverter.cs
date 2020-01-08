using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.QueryableExtensions.Impl
{
    public class MemberGetterExpressionResultConverter : IExpressionResultConverter
    {
        public ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, IMemberMap propertyMap, LetPropertyMaps letPropertyMaps)
            => propertyMap.SourceMembers.Aggregate(expressionResolutionResult, ExpressionResolutionResult);

        private static ExpressionResolutionResult ExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, MemberInfo getter)
        {
            var member = (getter is MethodInfo method)
                ? (Expression)Expression.Call(method, expressionResolutionResult.ResolutionExpression)
                : Expression.MakeMemberAccess(expressionResolutionResult.ResolutionExpression, getter);
            return new ExpressionResolutionResult(member, member.Type);
        }

        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, IMemberMap propertyMap) => propertyMap.SourceMembers.Count > 0;
    }
}