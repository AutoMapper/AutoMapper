using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    using static ExpressionFactory;
    public class CollectionMapper : EnumerableMapperBase
    {
        public override bool IsMatch(in TypePair context) => context.SourceType.IsEnumerableType() && context.DestinationType.IsEnumerableType();
        public override Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Expression sourceExpression, Expression destExpression)
            => MapCollectionExpression(configurationProvider, profileMap, memberMap, sourceExpression, destExpression);
    }
}