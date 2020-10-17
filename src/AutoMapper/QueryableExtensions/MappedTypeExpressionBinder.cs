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
        public bool IsMatch(IMemberMap memberMap, TypeMap memberTypeMap, Expression resolvedSource) =>
            memberTypeMap != null && memberTypeMap.CustomMapExpression == null;

        public Expression Build(IGlobalConfiguration configuration, IMemberMap memberMap, TypeMap memberTypeMap, ExpressionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps) 
        {
            var transformedExpression = configuration.ExpressionBuilder.CreateMapExpression(request, resolvedSource, letPropertyMaps);
            if(transformedExpression == null)
            {
                return null;
            }
            var sourceExpression = resolvedSource;
            // Handles null source property so it will not create an object with possible non-nullable properties which would result in an exception.
            if (memberMap.AllowsNullDestinationValues() && !(sourceExpression is ParameterExpression) && !sourceExpression.Type.IsCollectionType())
            {
                transformedExpression = sourceExpression.IfNullElse(Constant(null, transformedExpression.Type), transformedExpression);
            }
            return transformedExpression;
        }
    }
}