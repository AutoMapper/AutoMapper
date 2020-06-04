using AutoMapper.Internal;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MemberGetterExpressionResultConverter : IExpressionResultConverter
    {
        public ExpressionResolutionResult GetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, IMemberMap propertyMap, LetPropertyMaps letPropertyMaps)
            => new ExpressionResolutionResult(propertyMap.SourceMembers.MemberAccesses(propertyMap.CheckCustomSource(expressionResolutionResult, letPropertyMaps)));
        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, IMemberMap propertyMap) => 
            propertyMap.SourceMembers.Count > 0;
    }
}