using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    using static CollectionMapperExpressionFactory;

    public class CollectionMapper : EnumerableMapperBase
    {
        public override bool IsMatch(TypePair context) => context.SourceType.IsEnumerableType() && 
            (context.DestinationType.IsCollectionType() || context.DestinationType.IsListType());

        public override Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
            => MapCollectionExpression(configurationProvider, profileMap, memberMap, sourceExpression, destExpression, contextExpression, typeof(List<>), MapItemExpr);
    }
}