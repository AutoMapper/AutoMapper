using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using System.Collections.Generic;
    using System.Reflection;
    using Configuration;

    public class CollectionMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context) => context.SourceType.IsEnumerableType() && context.DestinationType.IsCollectionType();

        public Expression MapExpression(IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
            => configurationProvider.MapCollectionExpression(propertyMap, sourceExpression, destExpression, contextExpression, CollectionMapperExtensions.IfNotNull, typeof(List<>), CollectionMapperExtensions.MapItemExpr);
    }
}