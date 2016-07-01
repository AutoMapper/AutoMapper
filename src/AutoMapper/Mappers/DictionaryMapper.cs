using System.Linq.Expressions;
using AutoMapper.Execution;

namespace AutoMapper.Mappers
{
    using System.Collections.Generic;
    using Configuration;
    
    public class DictionaryMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
#if NET45
            => (context.SourceType.IsDictionaryType() && context.DestinationType.IsDictionaryType() && !context.SourceType.IsDynamic() && !context.DestinationType.IsDynamic());
#else
            => (context.SourceType.IsDictionaryType() && context.DestinationType.IsDictionaryType());
#endif

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
            => typeMapRegistry.MapCollectionExpression(configurationProvider, propertyMap, sourceExpression, destExpression, contextExpression, CollectionMapperExtensions.IfNotNull, typeof(Dictionary<,>), CollectionMapperExtensions.MapKeyPairValueExpr);
    }
}