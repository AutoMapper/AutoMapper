using AutoMapper.Configuration;
using AutoMapper.Execution;
using AutoMapper.Internal;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    using static Expression;

    public class MappedTypeExpressionBinder : IExpressionBinder
    {
        public bool IsMatch(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result) => 
            propertyTypeMap != null && propertyTypeMap.CustomMapExpression == null;

        public MemberAssignment Build(IConfigurationProvider configuration, PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps) 
            => BindMappedTypeExpression(configuration, propertyMap, request, result, typePairCount, letPropertyMaps);

        private static MemberAssignment BindMappedTypeExpression(IConfigurationProvider configuration, PropertyMap propertyMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps)
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

            return Bind(propertyMap.DestinationMember, transformedExpression);
        }
    }
}