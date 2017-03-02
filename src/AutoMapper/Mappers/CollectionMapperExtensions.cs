using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Execution;

namespace AutoMapper.Mappers
{
    internal static class CollectionMapperExtensions
    {
        internal static Expression MapCollectionExpression(this IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression,
            Expression destExpression, Expression contextExpression, Func<Expression, Expression> conditionalExpression, Type ifInterfaceType, MapItem mapItem)
        {
            var passedDestination = Expression.Variable(destExpression.Type, "passedDestination");
            var condition = conditionalExpression(passedDestination);
            var newExpression = Expression.Variable(passedDestination.Type, "collectionDestination");
            var sourceElementType = TypeHelper.GetElementType(sourceExpression.Type);
            ParameterExpression itemParam;

            var itemExpr = mapItem(configurationProvider, propertyMap, sourceExpression.Type, passedDestination.Type, contextExpression, out itemParam);

            var destinationElementType = itemExpr.Type;
            var destinationCollectionType = typeof(ICollection<>).MakeGenericType(destinationElementType);
            if(!destinationCollectionType.IsAssignableFrom(destExpression.Type))
            {
                destinationCollectionType = typeof(IList);
            }
            var addMethod = destinationCollectionType.GetDeclaredMethod("Add");
            var destination = propertyMap?.UseDestinationValue == true ? passedDestination : newExpression;
            var addItems = ExpressionExtensions.ForEach(sourceExpression, itemParam, Expression.Call(destination, addMethod, itemExpr));

            var mapExpr = Expression.Block(addItems, destination);

            var allowNullCollections = propertyMap?.TypeMap.Profile.AllowNullCollections ??
                                       configurationProvider.Configuration.AllowNullCollections;
            var ifNullExpr = allowNullCollections ? Expression.Constant(null, passedDestination.Type) : (Expression) newExpression;
            var clearMethod = destinationCollectionType.GetDeclaredMethod("Clear");
            var checkNull =  
                Expression.Block(new[] { newExpression, passedDestination },
                    Expression.Assign(passedDestination, destExpression),
                    Expression.IfThenElse(condition ?? Expression.Constant(false),
                        Expression.Block(Expression.Assign(newExpression, passedDestination), Expression.Call(newExpression, clearMethod)),
                        Expression.Assign(newExpression, passedDestination.Type.NewExpr(ifInterfaceType))),
                    Expression.Condition(Expression.Equal(sourceExpression, Expression.Constant(null)), ExpressionExtensions.ToType(ifNullExpr, passedDestination.Type), ExpressionExtensions.ToType(mapExpr, passedDestination.Type))
                );
            if(propertyMap != null)
            {
                return checkNull;
            }
            var elementTypeMap = configurationProvider.ResolveTypeMap(sourceElementType, destinationElementType);
            if(elementTypeMap == null)
            {
                return checkNull;
            }
            var checkContext = TypeMapPlanBuilder.CheckContext(elementTypeMap, contextExpression);
            if(checkContext == null)
            {
                return checkNull;
            }
            return Expression.Block(checkContext, checkNull);
        }

        internal static Delegate Constructor(Type type)
        {
            return Expression.Lambda(ExpressionExtensions.ToType(DelegateFactory.GenerateConstructorExpression(type), type)).Compile();
        }

        internal static Expression NewExpr(this Type baseType, Type ifInterfaceType)
        {
            var newExpr = baseType.IsInterface()
                ? Expression.New(ifInterfaceType.MakeGenericType(TypeHelper.GetElementTypes(baseType, ElementTypeFlags.BreakKeyValuePair)))
                : DelegateFactory.GenerateConstructorExpression(baseType);
            return ExpressionExtensions.ToType(newExpr, baseType);
        }

        public delegate Expression MapItem(IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Type sourceType, Type destType, Expression contextParam, out ParameterExpression itemParam);

        internal static Expression MapItemExpr(this IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Type sourceType, Type destType, Expression contextParam, out ParameterExpression itemParam)
        {
            var sourceElementType = TypeHelper.GetElementType(sourceType);
            var destElementType = TypeHelper.GetElementType(destType);
            itemParam = Expression.Parameter(sourceElementType, "item");

            var typePair = new TypePair(sourceElementType, destElementType);

            var itemExpr = TypeMapPlanBuilder.MapExpression(configurationProvider, typePair, itemParam, contextParam, propertyMap);
            return ExpressionExtensions.ToType(itemExpr, destElementType);
        }

        internal static Expression MapKeyPairValueExpr(IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Type sourceType, Type destType, Expression contextParam, out ParameterExpression itemParam)
        {
            var sourceElementTypes = TypeHelper.GetElementTypes(sourceType, ElementTypeFlags.BreakKeyValuePair);
            var destElementTypes = TypeHelper.GetElementTypes(destType, ElementTypeFlags.BreakKeyValuePair);

            var typePairKey = new TypePair(sourceElementTypes[0], destElementTypes[0]);
            var typePairValue = new TypePair(sourceElementTypes[1], destElementTypes[1]);

            var sourceElementType = typeof(KeyValuePair<,>).MakeGenericType(sourceElementTypes);
            itemParam = Expression.Parameter(sourceElementType, "item");
            var destElementType = typeof(KeyValuePair<,>).MakeGenericType(destElementTypes);

            var keyExpr = TypeMapPlanBuilder.MapExpression(configurationProvider, typePairKey, Expression.Property(itemParam, "Key"), contextParam, propertyMap);
            var valueExpr = TypeMapPlanBuilder.MapExpression(configurationProvider, typePairValue, Expression.Property(itemParam, "Value"), contextParam, propertyMap);
            var keyPair = Expression.New(destElementType.GetConstructors().First(), keyExpr, valueExpr);
            return keyPair;
        }

        internal static BinaryExpression IfNotNull(Expression destExpression)
        {
            return Expression.NotEqual(destExpression, Expression.Constant(null));
        }
    }
}