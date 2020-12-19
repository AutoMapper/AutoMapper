using AutoMapper.Execution;
using AutoMapper.Internal;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    using static Expression;
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MappedTypeProjectionMapper : IProjectionMapper
    {
        public bool IsMatch(MemberMap memberMap, TypeMap memberTypeMap, Expression resolvedSource) => memberTypeMap != null;
        public Expression Project(IGlobalConfiguration configuration, MemberMap memberMap, TypeMap memberTypeMap, ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps) 
        {
            var transformedExpression = configuration.ProjectionBuilder.CreateInnerProjection(request, resolvedSource, letPropertyMaps);
            if(transformedExpression == null)
            {
                return null;
            }
            // Handles null source property so it will not create an object with possible non-nullable properties which would result in an exception.
            if (memberMap.AllowsNullDestinationValues() && resolvedSource is not ParameterExpression && !resolvedSource.Type.IsCollection())
            {
                transformedExpression = resolvedSource.IfNullElse(Constant(null, transformedExpression.Type), transformedExpression);
            }
            return transformedExpression;
        }
    }
}