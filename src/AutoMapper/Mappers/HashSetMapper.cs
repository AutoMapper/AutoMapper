using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper.Configuration;

namespace AutoMapper.Mappers
{
    public class HashSetMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
            => context.SourceType.IsEnumerableType() && IsSetType(context.DestinationType);

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
            => CollectionMapperExtensions.MapCollectionExpression(configurationProvider, profileMap, propertyMap, sourceExpression, destExpression, contextExpression, CollectionMapperExtensions.IfNotNull, typeof(HashSet<>), CollectionMapperExtensions.MapItemExpr);

        private static bool IsSetType(Type type) => type.ImplementsGenericInterface(typeof(ISet<>));
    }
}