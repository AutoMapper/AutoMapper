using AutoMapper.Internal;
using System.ComponentModel;

namespace AutoMapper.QueryableExtensions.Impl
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MemberGetterExpressionResultConverter : IExpressionResultConverter
    {
        public ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, IMemberMap memberMap, LetPropertyMaps letPropertyMaps)
            => new ExpressionResolutionResult(memberMap.SourceMembers.MemberAccesses(memberMap.CheckCustomSource(expressionResolutionResult, letPropertyMaps)));
        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, IMemberMap memberMap) => 
            memberMap.SourceMembers.Count > 0;
    }
}