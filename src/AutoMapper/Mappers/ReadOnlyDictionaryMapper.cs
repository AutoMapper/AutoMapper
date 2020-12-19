using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using AutoMapper.Internal;
namespace AutoMapper.Mappers
{
    using static ExpressionFactory;
    using static Expression;
    public class ReadOnlyDictionaryMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context) => context.SourceType.IsCollection() && context.DestinationType.IsReadOnlyDictionaryType();
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression) =>
            MapReadOnlyCollection(typeof(Dictionary<,>), typeof(ReadOnlyDictionary<,>), configurationProvider, profileMap, memberMap, sourceExpression, destExpression);
    }
}