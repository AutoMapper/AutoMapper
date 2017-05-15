using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper.Configuration;
using AutoMapper.Mappers.Internal;

namespace AutoMapper.Mappers
{
    using static Expression;
    using static CollectionMapperExpressionFactory;

    public class EnumerableMapper : EnumerableMapperBase, IObjectMapper
    {
        public bool IsMatch(TypePair context) => (context.DestinationType.IsInterface() && context.DestinationType.IsEnumerableType() ||
                                                  context.DestinationType.IsListType())
                                                 && context.SourceType.IsEnumerableType();

        public Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            if(destExpression.Type.IsInterface())
            {
                var listType = typeof(IList<>).MakeGenericType(ElementTypeHelper.GetElementType(destExpression.Type));
                destExpression = Convert(destExpression, listType);
            }
            return MapCollectionExpression(configurationProvider, profileMap, propertyMap, sourceExpression,
                destExpression, contextExpression, IfEditableList, typeof(List<>), MapItemExpr);
        }

        private static Expression IfEditableList(Expression dest) => AndAlso(TypeIs(dest, typeof(IList)), Not(TypeIs(dest, typeof(Array))));
    }
}