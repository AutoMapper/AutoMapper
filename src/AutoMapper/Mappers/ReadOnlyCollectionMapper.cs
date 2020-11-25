using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    using static Expression;
    using static CollectionMapperExpressionFactory;

    public class ReadOnlyCollectionMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context) => context.SourceType.IsEnumerableType() && context.DestinationType.IsGenericType(typeof(ReadOnlyCollection<>));
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var listType = typeof(List<>).MakeGenericType(ElementTypeHelper.GetElementType(destExpression.Type));
            var list = MapCollectionExpression(configurationProvider, profileMap, memberMap, sourceExpression, Default(listType), contextExpression, typeof(List<>), MapItemExpr);
            var ctor = destExpression.Type.GetDeclaredConstructors().First();
            return New(ctor, list);
        }
    }
}