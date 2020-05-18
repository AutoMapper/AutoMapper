using System.Linq;
using System.ComponentModel;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.QueryableExtensions.Impl
{
    using static Expression;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MemberResolverExpressionResultConverter : IExpressionResultConverter
    {
        public ExpressionResolutionResult GetExpressionResolutionResult(
            ExpressionResolutionResult expressionResolutionResult, IMemberMap propertyMap, LetPropertyMaps letPropertyMaps)
        {
            var mapFrom = propertyMap.CustomMapExpression;
            if (!IsSubQuery() || letPropertyMaps.ConfigurationProvider.ResolveTypeMap(propertyMap.SourceType, propertyMap.DestinationType) == null)
            {
                return new ExpressionResolutionResult(mapFrom.ReplaceParameters(propertyMap.CheckCustomSource(expressionResolutionResult, letPropertyMaps)));
            }
            if (propertyMap.CustomSource == null)
            {
                return new ExpressionResolutionResult(letPropertyMaps.GetSubQueryMarker(mapFrom), mapFrom);
            }
            var newMapFrom = Lambda(mapFrom.ReplaceParameters(propertyMap.CustomSource.Body), propertyMap.CustomSource.Parameters);
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
        public bool CanGetExpressionResolutionResult(ExpressionResolutionResult expressionResolutionResult, IMemberMap propertyMap) => propertyMap.CustomMapExpression != null;
    }
}