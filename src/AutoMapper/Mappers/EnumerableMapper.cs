namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Configuration;

    public class EnumerableMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            return context.SourceType.IsEnumerableType() && 
                      context.DestinationType.IsInterface() && context.DestinationType.IsEnumerableType();
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Expression sourceExpression, Expression destExpression,
            Expression contextExpression)
        {
            var listType = typeof(List<>).MakeGenericType(TypeHelper.GetElementType(destExpression.Type));

            return typeMapRegistry.MapCollectionExpression(configurationProvider, propertyMap, sourceExpression,
                Expression.Default(listType), contextExpression, IfEditableList, typeof(List<>),
                CollectionMapperExtensions.MapItemExpr);
        }

        private static Expression IfEditableList(Expression dest)
        {
            return Expression.And(Expression.TypeIs(dest, typeof(IList)),
                Expression.Not(Expression.TypeIs(dest, typeof(Array))));
        }
    }
}