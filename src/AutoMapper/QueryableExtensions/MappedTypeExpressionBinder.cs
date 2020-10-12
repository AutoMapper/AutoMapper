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
        public bool IsMatch(IMemberMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) => 
            propertyTypeMap != null && propertyTypeMap.CustomMapExpression == null;

        public Expression Build(IGlobalConfiguration configuration, IMemberMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps) 
        {
            var transformedExpression = configuration.ExpressionBuilder.CreateMapExpression(request, result.ResolutionExpression, typePairCount, letPropertyMaps);
            if(transformedExpression == null)
            {
                return null;
            }
            // Handles null source property so it will not create an object with possible non-nullable properties which would result in an exception.
            if (propertyMap.AllowsNullDestinationValues() && !(result.ResolutionExpression is ParameterExpression) && !result.ResolutionExpression.Type.IsCollectionType())
            {
                transformedExpression = result.ResolutionExpression.IfNullElse(Constant(null, transformedExpression.Type), transformedExpression);
            }
            return transformedExpression;
        }
    }
}