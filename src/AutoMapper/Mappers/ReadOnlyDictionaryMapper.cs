using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper.Mappers
{
    using static Expression;
    using static ExpressionFactory;
    using static CollectionMapperExpressionFactory;

    public class ReadOnlyDictionaryMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            if (!(context.SourceType.IsEnumerableType() && context.DestinationType.IsGenericType))
                return false;

            var genericType = context.DestinationType.GetGenericTypeDefinition();

            return genericType == typeof(ReadOnlyDictionary<,>) || genericType == typeof(IReadOnlyDictionary<,>);
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var dictionaryTypes = ElementTypeHelper.GetElementTypes(destExpression.Type, ElementTypeFlags.BreakKeyValuePair);
            var dictType = typeof(Dictionary<,>).MakeGenericType(dictionaryTypes);
            var dict = MapCollectionExpression(configurationProvider, profileMap, memberMap, sourceExpression, Default(dictType), contextExpression, typeof(Dictionary<,>), MapKeyPairValueExpr);
            var dest = Variable(dictType, "dest");

            var readOnlyDictType = destExpression.Type.IsInterface
                ? typeof(ReadOnlyDictionary<,>).MakeGenericType(dictionaryTypes)
                : destExpression.Type;

            var ctor = readOnlyDictType.GetDeclaredConstructors()
                .First(ci => ci.GetParameters().Length == 1 && ci.GetParameters()[0].ParameterType.IsAssignableFrom(dest.Type));

            return Block(new[] { dest }, 
                Assign(dest, dict), 
                Condition(NotEqual(dest, Default(dictType)), 
                    ToType(New(ctor, dest), destExpression.Type), 
                    Default(destExpression.Type)));
        }
    }
}