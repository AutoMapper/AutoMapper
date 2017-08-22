using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper.Configuration;
using AutoMapper.Mappers.Internal;

namespace AutoMapper.Mappers
{
    using static CollectionMapperExpressionFactory;

    public class EnumerableToDictionaryMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context) => context.DestinationType.IsDictionaryType()
                                                 && context.SourceType.IsEnumerableType()
                                                 && !context.SourceType.IsDictionaryType();

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
            =>
            MapCollectionExpression(configurationProvider, profileMap, propertyMap, sourceExpression, destExpression,
                contextExpression, typeof(Dictionary<,>), MapItemExpr);
    }
}