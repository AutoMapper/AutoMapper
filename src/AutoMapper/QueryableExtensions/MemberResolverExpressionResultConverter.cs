using System.Linq;
using System.ComponentModel;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.QueryableExtensions.Impl
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MemberResolverExpressionResultConverter : IExpressionResultConverter
    {
        public ExpressionResolutionResult GetExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, IMemberMap memberMap, LetPropertyMaps letPropertyMaps)
        {
            var mapFrom = memberMap.CustomMapExpression;
            if (!IsSubQuery() || letPropertyMaps.ConfigurationProvider.ResolveTypeMap(memberMap.SourceType, memberMap.DestinationType) == null)
            {
                return new ExpressionResolutionResult(mapFrom.ReplaceParameters(memberMap.CheckCustomSource(expressionResolutionResult, letPropertyMaps)));
            }
            var customSource = memberMap.ProjectToCustomSource;
            if (customSource == null)
            {
                return new ExpressionResolutionResult(letPropertyMaps.GetSubQueryMarker(mapFrom), mapFrom);
            }
            var newMapFrom = IncludedMember.Chain(customSource, mapFrom);
            return new ExpressionResolutionResult(letPropertyMaps.GetSubQueryMarker(newMapFrom), newMapFrom);
            bool IsSubQuery()
            {
                if (!(mapFrom.Body is MethodCallExpression methodCall))
                {
                    return false;
                }
                var method = methodCall.Method;
                return method.IsStatic && method.DeclaringType == typeof(Enumerable);
            }
        }
        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, IMemberMap memberMap) => memberMap.CustomMapExpression != null;
    }
}