using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using System;
    using System.Collections.Generic;
    using Configuration;

    public class HashSetMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
            => context.SourceType.IsEnumerableType() && IsSetType(context.DestinationType);

        public Expression MapExpression(IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
            => configurationProvider.MapCollectionExpression(propertyMap, sourceExpression, destExpression, contextExpression, CollectionMapperExtensions.IfNotNull, typeof(HashSet<>), CollectionMapperExtensions.MapItemExpr);

        private static bool IsSetType(Type type)
        {
            return type.ImplementsGenericInterface(typeof(ISet<>));
        }
    }
}