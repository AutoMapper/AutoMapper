using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Configuration;

    public class EnumerableToDictionaryMapper : IObjectMapExpression
    {
        public bool IsMatch(TypePair context)
        {
            return (context.DestinationType.IsDictionaryType())
                   && (context.SourceType.IsEnumerableType())
                   && (!context.SourceType.IsDictionaryType());
        }

        public object Map(ResolutionContext context)
            => context.MapCollection(CollectionMapperExtensions.IfNotNull(Expression.Constant(context.DestinationValue)), typeof(Dictionary<,>));

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
            => typeMapRegistry.MapCollectionExpression(configurationProvider, propertyMap, sourceExpression, destExpression, contextExpression, CollectionMapperExtensions.IfNotNull, typeof(Dictionary<,>));
    }
}