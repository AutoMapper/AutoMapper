using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Execution;
using AutoMapper.Internal;

namespace AutoMapper.Mappers.Internal
{
    using static Expression;
    using static ExpressionBuilder;
    using static ExpressionFactory;
    using static ElementTypeHelper;

    public static class CollectionMapperExpressionFactory
    {
        public delegate Expression MapItem(IConfigurationProvider configurationProvider, ProfileMap profileMap,
            Type sourceType, Type destType, Expression contextParam,
            out ParameterExpression itemParam);

        public static Expression MapCollectionExpression(IConfigurationProvider configurationProvider,
            ProfileMap profileMap, IMemberMap memberMap, Expression sourceExpression, Expression destExpression,
            Expression contextExpression, Type ifInterfaceType, MapItem mapItem)
        {
            var passedDestination = Variable(destExpression.Type, "passedDestination");
            var newExpression = Variable(passedDestination.Type, "collectionDestination");
            var sourceElementType = GetElementType(sourceExpression.Type);

            var itemExpr = mapItem(configurationProvider, profileMap, sourceExpression.Type, passedDestination.Type,
                contextExpression, out ParameterExpression itemParam);

            var destinationElementType = itemExpr.Type;
            var destinationCollectionType = typeof(ICollection<>).MakeGenericType(destinationElementType);
            if (!destinationCollectionType.IsAssignableFrom(destExpression.Type))
                destinationCollectionType = typeof(IList);
            var addMethod = destinationCollectionType.GetDeclaredMethod("Add");

            Expression destination, assignNewExpression;

            UseDestinationValue();

            var addItems = ForEach(sourceExpression, itemParam, Call(destination, addMethod, itemExpr));
            var overMaxDepth = contextExpression.OverMaxDepth(memberMap?.TypeMap);
            if (overMaxDepth != null)
            {
                addItems = Condition(overMaxDepth, Empty(), addItems);
            }
            var mapExpr = Block(addItems, destination);

            var clearMethod = destinationCollectionType.GetDeclaredMethod("Clear");
            var checkNull =
                Block(new[] { newExpression, passedDestination },
                    Assign(passedDestination, destExpression),
                    assignNewExpression,
                    Call(destination, clearMethod),
                    mapExpr
                );
            if (memberMap != null)
                return checkNull;
            var elementTypeMap = configurationProvider.ResolveTypeMap(sourceElementType, destinationElementType);
            if (elementTypeMap == null)
                return checkNull;
            var checkContext = CheckContext(elementTypeMap, contextExpression);
            if (checkContext == null)
                return checkNull;
            return Block(checkContext, checkNull);
            void UseDestinationValue()
            {
                if(memberMap?.UseDestinationValue == true)
                {
                    destination = passedDestination;
                    assignNewExpression = Empty();
                }
                else
                {
                    destination = newExpression;
                    var createInstance = passedDestination.Type.NewExpr(ifInterfaceType);
                    var shouldCreateDestination = Equal(passedDestination, Constant(null));
                    if (memberMap?.CanBeSet == true)
                    {
                        var isReadOnly = Property(ToType(passedDestination, destinationCollectionType), "IsReadOnly");
                        shouldCreateDestination = OrElse(shouldCreateDestination, isReadOnly);
                    }
                    assignNewExpression = Assign(newExpression, Condition(shouldCreateDestination, ToType(createInstance, passedDestination.Type), passedDestination));
                }
            }
        }

        private static Expression NewExpr(this Type baseType, Type ifInterfaceType)
        {
            var newExpr = baseType.IsInterface
                ? New(
                    ifInterfaceType.MakeGenericType(GetElementTypes(baseType,
                        ElementTypeFlags.BreakKeyValuePair)))
                : DelegateFactory.GenerateConstructorExpression(baseType);
            return newExpr;
        }

        public static Expression MapItemExpr(IConfigurationProvider configurationProvider, ProfileMap profileMap, Type sourceType, Type destType, Expression contextParam, out ParameterExpression itemParam)
        {
            var sourceElementType = GetElementType(sourceType);
            var destElementType = GetElementType(destType);
            itemParam = Parameter(sourceElementType, "item");

            var typePair = new TypePair(sourceElementType, destElementType);

            var itemExpr = MapExpression(configurationProvider, profileMap, typePair, itemParam, contextParam);
            return ToType(itemExpr, destElementType);
        }

        public static Expression MapKeyPairValueExpr(IConfigurationProvider configurationProvider, ProfileMap profileMap, Type sourceType, Type destType, Expression contextParam, out ParameterExpression itemParam)
        {
            var sourceElementTypes = GetElementTypes(sourceType, ElementTypeFlags.BreakKeyValuePair);
            var destElementTypes = GetElementTypes(destType, ElementTypeFlags.BreakKeyValuePair);

            var typePairKey = new TypePair(sourceElementTypes[0], destElementTypes[0]);
            var typePairValue = new TypePair(sourceElementTypes[1], destElementTypes[1]);

            var sourceElementType = typeof(KeyValuePair<,>).MakeGenericType(sourceElementTypes);
            itemParam = Parameter(sourceElementType, "item");
            var destElementType = typeof(KeyValuePair<,>).MakeGenericType(destElementTypes);

            var keyExpr = MapExpression(configurationProvider, profileMap, typePairKey,
                Property(itemParam, "Key"), contextParam);
            var valueExpr = MapExpression(configurationProvider, profileMap, typePairValue,
                Property(itemParam, "Value"), contextParam);
            var keyPair = New(destElementType.GetDeclaredConstructors().First(), keyExpr, valueExpr);
            return keyPair;
        }
    }
}