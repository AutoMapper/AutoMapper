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
        public bool IsMatch(TypePair context)
        {
            if (!(context.SourceType.IsEnumerableType() && context.DestinationType.IsGenericType))
                return false;

            var genericType = context.DestinationType.GetGenericTypeDefinition();

            return genericType == typeof (ReadOnlyCollection<>);
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            IMemberMap memberMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var listType = typeof(List<>).MakeGenericType(ElementTypeHelper.GetElementType(destExpression.Type));
            var list = MapCollectionExpression(configurationProvider, profileMap, memberMap, sourceExpression, Default(listType), contextExpression, typeof(List<>), MapItemExpr);
            var dest = Variable(listType, "dest");

            var ctor = destExpression.Type.GetDeclaredConstructors()
                .First(ci => ci.GetParameters().Length == 1 && ci.GetParameters()[0].ParameterType.IsAssignableFrom(dest.Type));

            return Block(new[] { dest }, 
                Assign(dest, list), 
                Condition(NotEqual(dest, Default(listType)), 
                    New(ctor, dest), 
                    Default(destExpression.Type)));
        }
    }
}