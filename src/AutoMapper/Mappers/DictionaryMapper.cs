using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper.Configuration;

namespace AutoMapper.Mappers
{
    public class DictionaryMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context) => context.SourceType.IsDictionaryType() && context.DestinationType.IsDictionaryType();

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
            => CollectionMapperExtensions.MapCollectionExpression(configurationProvider, profileMap, propertyMap, sourceExpression, destExpression, contextExpression, CollectionMapperExtensions.IfNotNull, typeof(Dictionary<,>), CollectionMapperExtensions.MapKeyPairValueExpr);
    }
}