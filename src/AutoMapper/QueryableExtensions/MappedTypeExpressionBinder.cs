using AutoMapper.Execution;
using AutoMapper.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    using static Expression;
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MappedTypeExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(IMemberMap memberMap, TypeMap memberTypeMap, ExpressionResolutionResult resolvedSource) =>
            memberTypeMap != null && memberTypeMap.CustomMapExpression == null;

        public Expression Build(IGlobalConfiguration configuration, IMemberMap memberMap, TypeMap memberTypeMap, ExpressionRequest request, ExpressionResolutionResult resolvedSource, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps) 
        {
            var transformedExpression = configuration.ExpressionBuilder.CreateMapExpression(request, resolvedSource.ResolutionExpression, typePairCount, letPropertyMaps);
            if(transformedExpression == null)
            {
                return null;
            }
            var sourceExpression = resolvedSource.ResolutionExpression;
            // Handles null source property so it will not create an object with possible non-nullable properties which would result in an exception.
            if (memberMap.AllowsNullDestinationValues() && !(sourceExpression is ParameterExpression) && !sourceExpression.Type.IsCollectionType())
            {
                transformedExpression = sourceExpression.IfNullElse(Constant(null, transformedExpression.Type), transformedExpression);
            }
            return transformedExpression;
        }
    }
}