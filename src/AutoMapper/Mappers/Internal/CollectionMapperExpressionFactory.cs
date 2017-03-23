using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Execution;
using AutoMapper.Internal;

namespace AutoMapper.Mappers.Internal
{
    using static ExpressionFactory;

    public static class CollectionMapperExpressionFactory
    {
        public delegate Expression MapItem(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            PropertyMap propertyMap, Type sourceType, Type destType, Expression contextParam,
            out ParameterExpression itemParam);

        public static Expression MapCollectionExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression, Func<Expression, Expression> conditionalExpression, Type ifInterfaceType, MapItem mapItem)
        {
            var passedDestination = Expression.Variable(destExpression.Type, "passedDestination");
            var condition = conditionalExpression(passedDestination);
            var newExpression = Expression.Variable(passedDestination.Type, "collectionDestination");
            var sourceElementType = ElementTypeHelper.GetElementType(sourceExpression.Type);

            var itemExpr = mapItem(configurationProvider, profileMap, propertyMap, sourceExpression.Type, passedDestination.Type,
                contextExpression, out ParameterExpression itemParam);

            var destinationElementType = itemExpr.Type;
            var destinationCollectionType = typeof(ICollection<>).MakeGenericType(destinationElementType);
            if (!destinationCollectionType.IsAssignableFrom(destExpression.Type))
                destinationCollectionType = typeof(IList);
            var addMethod = destinationCollectionType.GetDeclaredMethod("Add");
            var destination = propertyMap?.UseDestinationValue == true ? passedDestination : newExpression;
            var addItems = ForEach(sourceExpression, itemParam, Expression.Call(destination, addMethod, itemExpr));

            var mapExpr = Expression.Block(addItems, destination);

            var ifNullExpr = profileMap.AllowNullCollections ? Expression.Constant(null, passedDestination.Type) : (Expression) newExpression;
            var clearMethod = destinationCollectionType.GetDeclaredMethod("Clear");
            var checkNull =
                Expression.Block(new[] {newExpression, passedDestination},
                    Expression.Assign(passedDestination, destExpression),
                    Expression.IfThenElse(condition ?? Expression.Constant(false),
                        Expression.Block(Expression.Assign(newExpression, passedDestination), Expression.Call(newExpression, clearMethod)),
                        Expression.Assign(newExpression, passedDestination.Type.NewExpr(ifInterfaceType))),
                    Expression.Condition(Expression.Equal(sourceExpression, Expression.Constant(null)), ToType(ifNullExpr, passedDestination.Type),
                        ToType(mapExpr, passedDestination.Type))
                );
            if (propertyMap != null)
                return checkNull;
            var elementTypeMap = configurationProvider.ResolveTypeMap(sourceElementType, destinationElementType);
            if (elementTypeMap == null)
                return checkNull;
            var checkContext = TypeMapPlanBuilder.CheckContext(elementTypeMap, contextExpression);
            if (checkContext == null)
                return checkNull;
            return Expression.Block(checkContext, checkNull);
        }

        private static Expression NewExpr(this Type baseType, Type ifInterfaceType)
        {
            var newExpr = baseType.IsInterface()
                ? Expression.New(
                    ifInterfaceType.MakeGenericType(ElementTypeHelper.GetElementTypes(baseType,
                        ElementTypeFlags.BreakKeyValuePair)))
                : DelegateFactory.GenerateConstructorExpression(baseType);
            return ToType(newExpr, baseType);
        }

        public static Expression MapItemExpr(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Type sourceType, Type destType, Expression contextParam, out ParameterExpression itemParam)
        {
            var sourceElementType = ElementTypeHelper.GetElementType(sourceType);
            var destElementType = ElementTypeHelper.GetElementType(destType);
            itemParam = Expression.Parameter(sourceElementType, "item");

            var typePair = new TypePair(sourceElementType, destElementType);

            var itemExpr = TypeMapPlanBuilder.MapExpression(configurationProvider, profileMap, typePair, itemParam, contextParam,
                propertyMap);
            return ToType(itemExpr, destElementType);
        }

        public static Expression MapKeyPairValueExpr(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Type sourceType, Type destType, Expression contextParam, out ParameterExpression itemParam)
        {
            var sourceElementTypes = ElementTypeHelper.GetElementTypes(sourceType, ElementTypeFlags.BreakKeyValuePair);
            var destElementTypes = ElementTypeHelper.GetElementTypes(destType, ElementTypeFlags.BreakKeyValuePair);

            var typePairKey = new TypePair(sourceElementTypes[0], destElementTypes[0]);
            var typePairValue = new TypePair(sourceElementTypes[1], destElementTypes[1]);

            var sourceElementType = typeof(KeyValuePair<,>).MakeGenericType(sourceElementTypes);
            itemParam = Expression.Parameter(sourceElementType, "item");
            var destElementType = typeof(KeyValuePair<,>).MakeGenericType(destElementTypes);

            var keyExpr = TypeMapPlanBuilder.MapExpression(configurationProvider, profileMap, typePairKey,
                Expression.Property(itemParam, "Key"), contextParam, propertyMap);
            var valueExpr = TypeMapPlanBuilder.MapExpression(configurationProvider, profileMap, typePairValue,
                Expression.Property(itemParam, "Value"), contextParam, propertyMap);
            var keyPair = Expression.New(destElementType.GetConstructors().First(), keyExpr, valueExpr);
            return keyPair;
        }

        public static BinaryExpression IfNotNull(Expression destExpression) => Expression.NotEqual(destExpression, Expression.Constant(null));
    }
}