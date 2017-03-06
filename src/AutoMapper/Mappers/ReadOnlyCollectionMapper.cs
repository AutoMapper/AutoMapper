using System.Linq;
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.Mappers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Configuration;

    public class ReadOnlyCollectionMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            if (!(context.SourceType.IsEnumerableType() && context.DestinationType.IsGenericType()))
                return false;

            var genericType = context.DestinationType.GetGenericTypeDefinition();

            return genericType == typeof (ReadOnlyCollection<>);
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var listType = typeof(List<>).MakeGenericType(TypeHelper.GetElementType(destExpression.Type));
            var list = CollectionMapperExtensions.MapCollectionExpression(configurationProvider, profileMap, propertyMap, sourceExpression, Default(listType), contextExpression, _ => Constant(false), typeof(List<>), CollectionMapperExtensions.MapItemExpr);
            var dest = Variable(listType, "dest");

            return Block(new[] { dest }, Assign(dest, list), Condition(NotEqual(dest, Default(listType)), New(destExpression.Type.GetConstructors().First(), dest), Default(destExpression.Type)));
        }
    }
}