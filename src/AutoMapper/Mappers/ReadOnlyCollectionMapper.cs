using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    using static Expression;
    using static ExpressionFactory;

    public class ReadOnlyCollectionMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context) => context.SourceType.IsEnumerableType() && context.DestinationType.IsGenericType(typeof(ReadOnlyCollection<>));
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var listType = typeof(List<>).MakeGenericType(destExpression.Type.GenericTypeArguments);
            var list = MapCollectionExpression(configurationProvider, profileMap, memberMap, sourceExpression, Default(listType));
            var ctor = destExpression.Type.GetConstructors()[0];
            return New(ctor, list);
        }
    }
}