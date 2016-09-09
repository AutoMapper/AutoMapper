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
        internal static Expression MapCollectionExpression(this TypeMapRegistry typeMapRegistry,
           IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression,
           Expression destExpression, Expression contextExpression, Func<Expression, Expression> conditionalExpression, Type ifInterfaceType, MapItem mapItem)
        {
            var passedDestination = Variable(destExpression.Type, "passedDestination");
            var condition = conditionalExpression(passedDestination);
            var newExpression = Variable(passedDestination.Type, "collectionDestination");
            var sourceElementType = TypeHelper.GetElementType(sourceExpression.Type);
            var itemParam = Parameter(sourceElementType, "item");

            var itemExpr = mapItem(typeMapRegistry, configurationProvider, propertyMap, sourceExpression.Type, passedDestination.Type, itemParam, contextExpression);

            var destinationElementType = TypeHelper.GetElementType(destExpression.Type);
            var destinationCollectionType = typeof(ICollection<>).MakeGenericType(destinationElementType);
            var clearMethod = destinationCollectionType.GetDeclaredMethod("Clear");
            var cast = typeof(Enumerable).GetDeclaredMethod("Cast").MakeGenericMethod(itemParam.Type);
            var addMethod = destinationCollectionType.GetDeclaredMethod("Add");
            var genericSource = sourceExpression.Type.IsGenericType() ? sourceExpression : Call(null, cast, sourceExpression);
            var destination = propertyMap?.UseDestinationValue == true ? passedDestination : newExpression;
            var addItems = ForEach(genericSource, itemParam, Call(destination, addMethod, itemExpr));

            var mapExpr = Block(addItems, destination);

            var ifNullExpr = configurationProvider.Configuration.AllowNullCollections ? Constant(null, passedDestination.Type) : (Expression) newExpression;
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
                ? New(ifInterfaceType.MakeGenericType(TypeHelper.GetElementTypes(baseType, ElemntTypeFlags.BreakKeyValuePair)))
                : DelegateFactory.GenerateConstructorExpression(baseType);
            return ToType(newExpr, baseType);
        }

        public delegate Expression MapItem(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Type sourceType, Type destType, ParameterExpression itemParam, Expression contextParam);

        internal static Expression MapItemExpr(this TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Type sourceType, Type destType, ParameterExpression itemParam, Expression contextParam)
        {
            var sourceElementType = TypeHelper.GetElementType(sourceType);
            var destElementType = TypeHelper.GetElementType(destType);

            var typePair = new TypePair(sourceElementType, destElementType);

            var itemExpr = TypeMapPlanBuilder.MapExpression(typeMapRegistry, configurationProvider, typePair, itemParam, contextParam, propertyMap);
            return itemExpr;
        }

        internal static Expression MapKeyPairValueExpr(this TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Type sourceType, Type destType, ParameterExpression itemParam, Expression contextParam)
        {
            var sourceElementTypes = TypeHelper.GetElementTypes(sourceType, ElemntTypeFlags.BreakKeyValuePair);
            var destElementTypes = TypeHelper.GetElementTypes(destType, ElemntTypeFlags.BreakKeyValuePair);

            var typePairKey = new TypePair(sourceElementTypes[0], destElementTypes[0]);
            var typePairValue = new TypePair(sourceElementTypes[1], destElementTypes[1]);

            var sourceElementType = TypeHelper.GetElementType(sourceType);
            var destElementType = TypeHelper.GetElementType(destType);

            var keyExpr = TypeMapPlanBuilder.MapExpression(typeMapRegistry, configurationProvider, typePairKey, Property(itemParam, "Key"), contextParam, propertyMap);
            var valueExpr = TypeMapPlanBuilder.MapExpression(typeMapRegistry, configurationProvider, typePairValue, Property(itemParam, "Value"), contextParam, propertyMap);
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

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
            => typeMapRegistry.MapCollectionExpression(configurationProvider, propertyMap, sourceExpression, destExpression, contextExpression, CollectionMapperExtensions.IfNotNull, typeof(List<>), CollectionMapperExtensions.MapItemExpr);
    }
}