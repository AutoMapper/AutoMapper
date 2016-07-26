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
            var newExpressionValue = destExpression.NewIfConditionFails(conditionalExpression, ifInterfaceType);
            var newExpression = Variable(newExpressionValue.Type, "collectionDestination");
            var sourceElementType = TypeHelper.GetElementType(sourceExpression.Type);
            var itemParam = Parameter(sourceElementType, "item");
            var itemExpr = mapItem(typeMapRegistry, configurationProvider, propertyMap, sourceExpression.Type, destExpression.Type, itemParam, contextExpression);

            var blockExpressions = new List<Expression>();
            var blockVariables = new List<ParameterExpression> { newExpression };
            Expression destination;
            var destinationElementType = itemExpr.Type;
            var destinationCollectionType = typeof(ICollection<>).MakeGenericType(destinationElementType);
            var clearMethod = destinationCollectionType.GetDeclaredMethod("Clear");
            if(destExpression.Type.IsCollectionType())
            {
                if(propertyMap == null)
                {
                    destination = newExpression;
                    blockExpressions.Add(IfThenElse(NotEqual(destExpression, Constant(null)),
                        Call(destExpression, clearMethod),
                        Assign(destination, newExpression)
                        ));
                }
                else if(propertyMap.UseDestinationValue)
                {
                    destination = destExpression;
                    blockExpressions.Add(Call(destExpression, clearMethod));
                }
                else
                {
                    destination = newExpression;
                }
            }
            else
            {
                destination = newExpression;
            }

            var cast = typeof(Enumerable).GetTypeInfo().DeclaredMethods.First(_ => _.Name == "Cast").MakeGenericMethod(itemParam.Type);

            var addMethod = destinationCollectionType.GetDeclaredMethod("Add");
            var genericSource = sourceExpression.Type.GetTypeInfo().IsGenericType ? sourceExpression : Call(null, cast, sourceExpression);
            blockExpressions.Add(ForEach(genericSource, itemParam, Call(
                destination,
                addMethod,
                itemExpr)));

            blockExpressions.Add(destination);

            var mapExpr = Block(blockExpressions);

            var ifNullExpr = configurationProvider.Configuration.AllowNullCollections ? Constant(null, destExpression.Type) : (Expression) newExpression;
            var checkNull = Block(blockVariables,
                Assign(newExpression, newExpressionValue),
                Condition(Equal(sourceExpression, Constant(null)), ToType(ifNullExpr, destExpression.Type), ToType(mapExpr, destExpression.Type))
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

        private static Expression NewIfConditionFails(this Expression destinationExpression, Func<Expression, Expression> conditionalExpression,
            Type ifInterfaceType)
        {
            var condition = conditionalExpression(destinationExpression);
            if (condition == null || destinationExpression.NodeType == ExpressionType.Default)
                return destinationExpression.Type.NewExpr(ifInterfaceType);
            return Condition(condition, destinationExpression, destinationExpression.Type.NewExpr(ifInterfaceType));
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

            var itemExpr = MapExpression(typeMapRegistry, configurationProvider, propertyMap, itemParam, contextParam, typePair);
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

            var keyExpr = MapExpression(typeMapRegistry, configurationProvider, propertyMap, Property(itemParam, "Key"), contextParam, typePairKey);
            var valueExpr = MapExpression(typeMapRegistry, configurationProvider, propertyMap, Property(itemParam, "Value"), contextParam, typePairValue);
            var keyPair = New(destElementType.GetConstructors().First(), keyExpr, valueExpr);
            return keyPair;
        }

        private static Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Expression itemParam, Expression contextParam, TypePair typePair)
        {
            Expression itemExpr;
            var typeMap = configurationProvider.ResolveTypeMap(typePair);
            if (typeMap != null && !typeMap.HasDerivedTypesToInclude())
            {
                typeMap.Seal(typeMapRegistry, configurationProvider);
                if(typeMap.MapExpression != null)
                {
                    return typeMap.MapExpression.ReplaceParameters(itemParam, Default(typePair.DestinationType), contextParam);
                }
            }
            var match = configurationProvider.GetMappers().FirstOrDefault(m => m.IsMatch(typePair));
            if (match != null && typeMap == null)
            {
                itemExpr =
                    ToType(
                        match.MapExpression(typeMapRegistry, configurationProvider, propertyMap, itemParam,
                            Default(typePair.DestinationType), contextParam), typePair.DestinationType);
            }
            else
            {
                var mapMethod =
                    typeof (ResolutionContext).GetDeclaredMethods()
                        .First(m => m.Name == "Map")
                        .MakeGenericMethod(typePair.SourceType, typePair.DestinationType);
                itemExpr = Call(contextParam, mapMethod, itemParam, Default(typePair.DestinationType));
            }
            return itemExpr;
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