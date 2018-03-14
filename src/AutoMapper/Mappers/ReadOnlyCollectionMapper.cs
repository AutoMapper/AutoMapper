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

    public class ReadOnlyCollectionMapper : IObjectMapper
    {
        public bool IsMatch(in TypePair context)
        {
            if (!(context.SourceType.IsEnumerableType() && context.DestinationType.IsGenericType()))
                return false;

            var genericType = context.DestinationType.GetGenericTypeDefinition();

            return genericType == typeof (ReadOnlyCollection<>);
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var listType = typeof(List<>).MakeGenericType(ElementTypeHelper.GetElementType(destExpression.Type));
            var list = MapCollectionExpression(configurationProvider, profileMap, propertyMap, sourceExpression, Default(listType), contextExpression, typeof(List<>), MapItemExpr);
            var dest = Variable(listType, "dest");

            return Block(new[] { dest }, Assign(dest, list), Condition(NotEqual(dest, Default(listType)), New(destExpression.Type.GetDeclaredConstructors().First(), dest), Default(destExpression.Type)));
        }
    }
}