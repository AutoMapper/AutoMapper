using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Configuration;

    public class ReadOnlyCollectionMapper : IObjectMapExpression
    {
        
        public object Map(ResolutionContext context)
        {
            var listType = typeof(List<>).MakeGenericType(TypeHelper.GetElementType(context.DestinationType));
            var getDestExpr = Lambda(New(listType), Parameter(listType, "d"));
            var constructor = context.DestinationType.GetConstructors().First();
            return constructor.Invoke( new [] {
                CollectionMapper.MapMethodInfo.MakeGenericMethod(context.SourceType, TypeHelper.GetElementType(context.SourceType), listType, TypeHelper.GetElementType(context.DestinationType))
                    .Invoke(null, new[] { context.SourceValue, null, context, getDestExpr.Compile(), null })});
        }

        private static Expression MapExpressionBase(TypeMapRegistry typeMapRegistry,
            IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression,
            Expression destExpression, Expression contextExpression)
        {
            var listType = typeof (List<>).MakeGenericType(TypeHelper.GetElementType(destExpression.Type));
            var getDestExpr = Lambda(New(listType), Parameter(listType, "d"));
            var itemExpr = CollectionMapper.ItemExpr(typeMapRegistry, configurationProvider, propertyMap, sourceExpression.Type, listType);

            var list = Call(null,
                CollectionMapper.MapMethodInfo.MakeGenericMethod(sourceExpression.Type, TypeHelper.GetElementType(sourceExpression.Type),
                    listType, TypeHelper.GetElementType(destExpression.Type)),
                sourceExpression, Default(listType), contextExpression, Constant(getDestExpr.Compile()),
                Constant(itemExpr.Compile()));
            return New(destExpression.Type.GetConstructors().First(), list);
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
            return MapExpressionBase(typeMapRegistry, configurationProvider, propertyMap, sourceExpression, destExpression, contextExpression);
        }
    }
}