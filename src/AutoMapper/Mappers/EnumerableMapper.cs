using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.Reflection;
    using Configuration;

    public class EnumerableMapper : IObjectMapExpression
    {

        public object Map(ResolutionContext context)
        {
            var listType = typeof(List<>).MakeGenericType(TypeHelper.GetElementType(context.DestinationType));
            return context.MapCollection(IfEditableList(Expression.Constant(listType)), typeof(List<>), CollectionMapperExtensions.MapItemMethodInfo, listType);
        }

        public bool IsMatch(TypePair context)
        {
            // destination type must be IEnumerable interface or a class implementing at least IList 
            return ((context.DestinationType.IsInterface() && context.DestinationType.IsEnumerableType()) ||
                    context.DestinationType.IsListType())
                   && context.SourceType.IsEnumerableType();
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var listType = typeof(List<>).MakeGenericType(TypeHelper.GetElementType(destExpression.Type));

            return typeMapRegistry.MapCollectionExpression(configurationProvider, propertyMap, sourceExpression, Expression.Default(listType), contextExpression, IfEditableList, typeof(List<>), CollectionMapperExtensions.MapItemExpr);
        }

        private static Expression IfEditableList(Expression dest)
        {
            return Expression.And(Expression.TypeIs(dest, typeof(IList)), Expression.Not(Expression.TypeIs(dest, typeof(Array))));
        }
    }
}