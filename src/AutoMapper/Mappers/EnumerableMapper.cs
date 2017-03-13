using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper.Configuration;

namespace AutoMapper.Mappers
{
    using static Expression;

    public class EnumerableMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context) => (context.DestinationType.IsInterface() && context.DestinationType.IsEnumerableType() ||
                                                  context.DestinationType.IsListType())
                                                 && context.SourceType.IsEnumerableType();

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

        private static Expression IfEditableList(Expression dest) => And(TypeIs(dest, typeof(IList)), Not(TypeIs(dest, typeof(Array))));
    }
}