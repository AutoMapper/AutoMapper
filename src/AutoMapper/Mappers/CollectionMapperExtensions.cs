using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Execution;

namespace AutoMapper.Mappers
{
    using static Expression;
    using static ExpressionExtensions;

    internal static class CollectionMapperExtensions
    {
        public delegate Expression MapItem(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            PropertyMap propertyMap, Type sourceType, Type destType, Expression contextParam,
            out ParameterExpression itemParam);

        internal static Expression MapCollectionExpression(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression, Func<Expression, Expression> conditionalExpression, Type ifInterfaceType, MapItem mapItem)
        {
            var passedDestination = Variable(destExpression.Type, "passedDestination");
            var condition = conditionalExpression(passedDestination);
            var newExpression = Variable(passedDestination.Type, "collectionDestination");
            var sourceElementType = TypeHelper.GetElementType(sourceExpression.Type);
            ParameterExpression itemParam;

            var itemExpr = mapItem(configurationProvider, profileMap, propertyMap, sourceExpression.Type, passedDestination.Type,
                contextExpression, out itemParam);

            var destinationElementType = itemExpr.Type;
            var destinationCollectionType = typeof(ICollection<>).MakeGenericType(destinationElementType);
            if (!destinationCollectionType.IsAssignableFrom(destExpression.Type))
                destinationCollectionType = typeof(IList);
            var addMethod = destinationCollectionType.GetDeclaredMethod("Add");
            var destination = propertyMap?.UseDestinationValue == true ? passedDestination : newExpression;
            var addItems = ForEach(sourceExpression, itemParam, Call(destination, addMethod, itemExpr));

            var mapExpr = Block(addItems, destination);

            var ifNullExpr = profileMap.AllowNullCollections ? Constant(null, passedDestination.Type) : (Expression) newExpression;
            var clearMethod = destinationCollectionType.GetDeclaredMethod("Clear");
            var checkNull =
                Block(new[] {newExpression, passedDestination},
                    Assign(passedDestination, destExpression),
                    IfThenElse(condition ?? Constant(false),
                        Block(Assign(newExpression, passedDestination), Call(newExpression, clearMethod)),
                        Assign(newExpression, passedDestination.Type.NewExpr(ifInterfaceType))),
                    Condition(Equal(sourceExpression, Constant(null)), ToType(ifNullExpr, passedDestination.Type),
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
            return Block(checkContext, checkNull);
        }

        private static Expression NewExpr(this Type baseType, Type ifInterfaceType)
        {
            var newExpr = baseType.IsInterface()
                ? New(
                    ifInterfaceType.MakeGenericType(TypeHelper.GetElementTypes(baseType,
                        ElementTypeFlags.BreakKeyValuePair)))
                : DelegateFactory.GenerateConstructorExpression(baseType);
            return ToType(newExpr, baseType);
        }

        internal static Expression MapItemExpr(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Type sourceType, Type destType, Expression contextParam, out ParameterExpression itemParam)
        {
            var sourceElementType = TypeHelper.GetElementType(sourceType);
            var destElementType = TypeHelper.GetElementType(destType);
            itemParam = Parameter(sourceElementType, "item");

            var typePair = new TypePair(sourceElementType, destElementType);

            var itemExpr = TypeMapPlanBuilder.MapExpression(configurationProvider, profileMap, typePair, itemParam, contextParam,
                propertyMap);
            return ToType(itemExpr, destElementType);
        }

        internal static Expression MapKeyPairValueExpr(IConfigurationProvider configurationProvider, ProfileMap profileMap, PropertyMap propertyMap, Type sourceType, Type destType, Expression contextParam, out ParameterExpression itemParam)
        {
            var sourceElementTypes = TypeHelper.GetElementTypes(sourceType, ElementTypeFlags.BreakKeyValuePair);
            var destElementTypes = TypeHelper.GetElementTypes(destType, ElementTypeFlags.BreakKeyValuePair);

            var typePairKey = new TypePair(sourceElementTypes[0], destElementTypes[0]);
            var typePairValue = new TypePair(sourceElementTypes[1], destElementTypes[1]);

            var sourceElementType = typeof(KeyValuePair<,>).MakeGenericType(sourceElementTypes);
            itemParam = Parameter(sourceElementType, "item");
            var destElementType = typeof(KeyValuePair<,>).MakeGenericType(destElementTypes);

            var keyExpr = TypeMapPlanBuilder.MapExpression(configurationProvider, profileMap, typePairKey,
                Property(itemParam, "Key"), contextParam, propertyMap);
            var valueExpr = TypeMapPlanBuilder.MapExpression(configurationProvider, profileMap, typePairValue,
                Property(itemParam, "Value"), contextParam, propertyMap);
            var keyPair = New(destElementType.GetConstructors().First(), keyExpr, valueExpr);
            return keyPair;
        }

        internal static BinaryExpression IfNotNull(Expression destExpression)
        {
            return NotEqual(destExpression, Constant(null));
        }
    }
}