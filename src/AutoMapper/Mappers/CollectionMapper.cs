using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Execution;
using static System.Linq.Expressions.Expression;
using static AutoMapper.ExpressionExtensions;

namespace AutoMapper.Mappers
{
    using System.Collections.Generic;
    using System.Reflection;
    using Configuration;

    public static class CollectionMapperExtensions
    {
        internal static Expression MapCollectionExpression(this IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression,
           Expression destExpression, Expression contextExpression, Func<Expression, Expression> conditionalExpression, Type ifInterfaceType, MapItem mapItem)
        {
            var passedDestination = Variable(destExpression.Type, "passedDestination");
            var condition = conditionalExpression(passedDestination);
            var newExpression = Variable(passedDestination.Type, "collectionDestination");
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
            var addItems = ForEach(sourceExpression, itemParam, Call(destination, addMethod, itemExpr));

            var mapExpr = Block(addItems, destination);

            var allowNullCollections = propertyMap?.TypeMap.Profile.AllowNullCollections ??
                                       configurationProvider.Configuration.AllowNullCollections;
            var ifNullExpr = allowNullCollections ? Constant(null, passedDestination.Type) : (Expression) newExpression;
            var clearMethod = destinationCollectionType.GetDeclaredMethod("Clear");
            var checkNull =  
                Block(new[] { newExpression, passedDestination },
                    Assign(passedDestination, destExpression),
                    IfThenElse(condition ?? Constant(false),
                                    Block(Assign(newExpression, passedDestination), Call(newExpression, clearMethod)),
                                    Assign(newExpression, passedDestination.Type.NewExpr(ifInterfaceType))),
                    Condition(Equal(sourceExpression, Constant(null)), ToType(ifNullExpr, passedDestination.Type), ToType(mapExpr, passedDestination.Type))
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
            return Block(checkContext, checkNull);
        }

        internal static Delegate Constructor(Type type)
        {
            return Lambda(ToType(DelegateFactory.GenerateConstructorExpression(type), type)).Compile();
        }

        internal static Expression NewExpr(this Type baseType, Type ifInterfaceType)
        {
            var newExpr = baseType.IsInterface()
                ? New(ifInterfaceType.MakeGenericType(TypeHelper.GetElementTypes(baseType, ElementTypeFlags.BreakKeyValuePair)))
                : DelegateFactory.GenerateConstructorExpression(baseType);
            return ToType(newExpr, baseType);
        }

        public delegate Expression MapItem(IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Type sourceType, Type destType, Expression contextParam, out ParameterExpression itemParam);

        internal static Expression MapItemExpr(this IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Type sourceType, Type destType, Expression contextParam, out ParameterExpression itemParam)
        {
            var sourceElementType = TypeHelper.GetElementType(sourceType);
            var destElementType = TypeHelper.GetElementType(destType);
            itemParam = Parameter(sourceElementType, "item");

            var typePair = new TypePair(sourceElementType, destElementType);

            var itemExpr = TypeMapPlanBuilder.MapExpression(configurationProvider, typePair, itemParam, contextParam, propertyMap);
            return ToType(itemExpr, destElementType);
        }

        internal static Expression MapKeyPairValueExpr(IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Type sourceType, Type destType, Expression contextParam, out ParameterExpression itemParam)
        {
            var sourceElementTypes = TypeHelper.GetElementTypes(sourceType, ElementTypeFlags.BreakKeyValuePair);
            var destElementTypes = TypeHelper.GetElementTypes(destType, ElementTypeFlags.BreakKeyValuePair);

            var typePairKey = new TypePair(sourceElementTypes[0], destElementTypes[0]);
            var typePairValue = new TypePair(sourceElementTypes[1], destElementTypes[1]);

            var sourceElementType = typeof(KeyValuePair<,>).MakeGenericType(sourceElementTypes);
            itemParam = Parameter(sourceElementType, "item");
            var destElementType = typeof(KeyValuePair<,>).MakeGenericType(destElementTypes);

            var keyExpr = TypeMapPlanBuilder.MapExpression(configurationProvider, typePairKey, Property(itemParam, "Key"), contextParam, propertyMap);
            var valueExpr = TypeMapPlanBuilder.MapExpression(configurationProvider, typePairValue, Property(itemParam, "Value"), contextParam, propertyMap);
            var keyPair = New(destElementType.GetConstructors().First(), keyExpr, valueExpr);
            return keyPair;
        }

        internal static BinaryExpression IfNotNull(Expression destExpression)
        {
            return NotEqual(destExpression, Constant(null));
        }
    }

    public class CollectionMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context) => context.SourceType.IsEnumerableType() && context.DestinationType.IsCollectionType();

        public Expression MapExpression(IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
            => configurationProvider.MapCollectionExpression(propertyMap, sourceExpression, destExpression, contextExpression, CollectionMapperExtensions.IfNotNull, typeof(List<>), CollectionMapperExtensions.MapItemExpr);
    }
}