using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Configuration;
using AutoMapper.Mappers.Internal;

namespace AutoMapper.Mappers
{
    using static Expression;
    using static CollectionMapperExpressionFactory;

    public class ReadOnlyDictionaryMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            if (!(context.SourceType.IsEnumerableType() && context.DestinationType.IsGenericType()))
                return false;

            var genericType = context.DestinationType.GetGenericTypeDefinition();

            return genericType == typeof(ReadOnlyDictionary<,>);
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var dictType = typeof(Dictionary<,>).MakeGenericType(ElementTypeHelper.GetElementTypes(destExpression.Type, ElementTypeFlags.BreakKeyValuePair));
            var dict = MapCollectionExpression(configurationProvider, profileMap, memberMap, sourceExpression, Default(dictType), contextExpression, typeof(Dictionary<,>), MapKeyPairValueExpr);
            var dest = Variable(dictType, "dest");

            return Block(new[] { dest }, Assign(dest, dict), Condition(NotEqual(dest, Default(dictType)), New(destExpression.Type.GetDeclaredConstructors().First(), dest), Default(destExpression.Type)));
        }
    }
}