using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using Configuration;
    using static Expression;

    public class EnumerableMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            // destination type must be IEnumerable interface or a class implementing at least IList 
            return ((context.DestinationType.IsInterface() && context.DestinationType.IsEnumerableType()) ||
                    context.DestinationType.IsListType())
                   && context.SourceType.IsEnumerableType();
        }

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            if(destExpression.Type.IsInterface())
            {
                var listType = typeof(List<>).MakeGenericType(TypeHelper.GetElementType(destExpression.Type));
                destExpression = Default(listType);
            }
            return CollectionMapperExtensions.MapCollectionExpression(configurationProvider, profileMap, propertyMap, sourceExpression,
                destExpression, contextExpression, IfEditableList, typeof(List<>), CollectionMapperExtensions.MapItemExpr);
        }

        private static Expression IfEditableList(Expression dest)
        {
            return And(TypeIs(dest, typeof(IList)), Not(TypeIs(dest, typeof(Array))));
        }
    }
}