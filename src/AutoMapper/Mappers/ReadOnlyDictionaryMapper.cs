using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    using static CollectionMapperExpressionFactory;

    public class ReadOnlyDictionaryMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context) => context.SourceType.IsReadOnlyDictionaryType() && context.DestinationType.IsReadOnlyDictionaryType();
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression, Expression contextExpression) =>
            MapToReadOnlyDictionary(configurationProvider, profileMap, memberMap, sourceExpression, destExpression, contextExpression, MapKeyPairValueExpr);
    }
}