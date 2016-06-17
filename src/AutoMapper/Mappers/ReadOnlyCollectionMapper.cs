using System.Linq;
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.Mappers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Configuration;

    public class ReadOnlyCollectionMapper : IObjectMapExpression
    {
        public object Map(ResolutionContext context)
        {
            var listType = typeof(List<>).MakeGenericType(TypeHelper.GetElementType(context.DestinationType));
            var list = context.MapCollection(null, typeof(List<>), CollectionMapperExtensions.MapItemMethodInfo, listType);
            if (list == null)
                return null;
            var constructor = context.DestinationType.GetConstructors().First();
            return constructor.Invoke( new [] { list });
        }

        public bool IsMatch(TypePair context)
        {
            if (!(context.SourceType.IsEnumerableType() && context.DestinationType.IsGenericType()))
                return false;

            var genericType = context.DestinationType.GetGenericTypeDefinition();

            return genericType == typeof (ReadOnlyCollection<>);
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var listType = typeof(List<>).MakeGenericType(TypeHelper.GetElementType(destExpression.Type));
            var list = typeMapRegistry.MapCollectionExpression(configurationProvider, propertyMap, sourceExpression, Default(listType), contextExpression, _ => Constant(false), typeof(List<>), CollectionMapperExtensions.MapItemExpr);
            var dest = Variable(listType, "dest");

            return Block(new[] { dest }, Assign(dest, list), Condition(NotEqual(dest, Default(listType)), New(destExpression.Type.GetConstructors().First(), dest), Default(destExpression.Type)));
        }
    }
}