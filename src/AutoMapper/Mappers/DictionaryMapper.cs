using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using System.Collections.Generic;
    using Configuration;
    
    public class DictionaryMapper : IObjectMapExpression
    {
        public bool IsMatch(TypePair context)
            => (context.SourceType.IsDictionaryType() && context.DestinationType.IsDictionaryType());

        public object Map(ResolutionContext context)
            => context.MapCollection(CollectionMapperExtensions.IfNotNull(Expression.Constant(context.DestinationValue)), typeof(Dictionary<,>), CollectionMapperExtensions.MapKeyValuePairMethodInfo);

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
            => typeMapRegistry.MapCollectionExpression(configurationProvider, propertyMap, sourceExpression, destExpression, contextExpression, CollectionMapperExtensions.IfNotNull, typeof(Dictionary<,>), CollectionMapperExtensions.MapKeyPairValueExpr);
    }
}