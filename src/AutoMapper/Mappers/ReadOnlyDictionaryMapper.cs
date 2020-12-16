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
        public bool IsMatch(in TypePair context) => context.SourceType.IsEnumerableType() && context.DestinationType.IsReadOnlyDictionaryType();
        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression)
        {
            var dictionaryTypes = destExpression.Type.GenericTypeArguments;
            var dictType = typeof(Dictionary<,>).MakeGenericType(dictionaryTypes);
            var dict = MapCollectionExpression(configurationProvider, profileMap, memberMap, sourceExpression, Default(dictType));
            var readOnlyDictType = destExpression.Type.IsInterface ? typeof(ReadOnlyDictionary<,>).MakeGenericType(dictionaryTypes) : destExpression.Type;
            var ctor = readOnlyDictType.GetConstructors()[0];
            return New(ctor, dict);
        }
    }
}