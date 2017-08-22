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

    public class EnumerableMapper : EnumerableMapperBase
    {
        public override bool IsMatch(TypePair context) => (context.DestinationType.IsInterface() && context.DestinationType.IsEnumerableType() ||
                                                  context.DestinationType.IsListType())
                                                 && context.SourceType.IsEnumerableType();

        public override Expression MapExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            if(destExpression.Type.IsInterface())
            {
                var listType = typeof(IList<>).MakeGenericType(ElementTypeHelper.GetElementType(destExpression.Type));
                destExpression = Convert(destExpression, listType);
            }
            return MapCollectionExpression(configurationProvider, profileMap, propertyMap, sourceExpression,
                destExpression, contextExpression, typeof(List<>), MapItemExpr);
        }
    }
}