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
            var newExpr = destExpression.NewIfConditionFails(conditionalExpression, ifInterfaceType);
            var itemExpr = mapItem(typeMapRegistry, configurationProvider, propertyMap, sourceExpression.Type, destExpression.Type);

            var blockExprs = new List<Expression>();
            var blockParams = new List<ParameterExpression>();
            var dest = destExpression;
            if (destExpression.Type.IsCollectionType())
            {
                if (propertyMap == null)
                {
                    var destParam = Parameter(newExpr.Type, "d");
                    blockParams.Add(destParam);

                    blockExprs.Add(Assign(destParam, destExpression));

                    dest = destParam;

                    var clearMethod = typeof(ICollection<>).MakeGenericType(TypeHelper.GetElementType(destExpression.Type)).GetMethod("Clear");
                    blockExprs.Add(IfThenElse(NotEqual(destExpression, Constant(null)),
                        Call(destExpression, clearMethod),
                        Assign(destParam, newExpr)
                        ));
                }
                else if (propertyMap.UseDestinationValue)
                {
                    var clearMethod = typeof(ICollection<>).MakeGenericType(TypeHelper.GetElementType(destExpression.Type)).GetMethod("Clear");
                    blockExprs.Add(Call(destExpression, clearMethod));
                }
                else
                {
                    var destParam = Parameter(newExpr.Type, "d");
                    blockParams.Add(destParam);
                    blockExprs.Add(Assign(destParam, newExpr));
                    dest = destParam;
                }
            }
            else
            {
                var destParam = Parameter(newExpr.Type, "d");
                blockParams.Add(destParam);
                blockExprs.Add(Assign(destParam, newExpr));
                dest = destParam;
            }

            var itemContext = Variable(typeof(ResolutionContext), "itemContext");
            blockParams.Add(itemContext);
            blockExprs.Add(Assign(itemContext, New(typeof(ResolutionContext).GetTypeInfo().DeclaredConstructors.First(_ => _.GetParameters().Length == 1), contextExpression)));

            var cast = typeof(Enumerable).GetTypeInfo().DeclaredMethods.First(_ => _.Name == "Cast").MakeGenericMethod(itemExpr.Parameters[0].Type);

            var addMethod = typeof(ICollection<>).MakeGenericType(TypeHelper.GetElementType(destExpression.Type)).GetMethod("Add");
            if (!sourceExpression.Type.GetTypeInfo().IsGenericType)
                sourceExpression = Call(null, cast, sourceExpression);
            blockExprs.Add(ForEach(sourceExpression, itemExpr.Parameters[0], Call(
                dest,
                addMethod,
                itemExpr.ReplaceParameters(itemExpr.Parameters[0], itemContext))));

            blockExprs.Add(dest);

            var mapExpr = Block(blockParams, blockExprs);

            var ifNullExpr = configurationProvider.AllowNullCollections
                     ? Constant(null, destExpression.Type)
                     : newExpr;
            return Condition(
                Equal(sourceExpression, Constant(null)),
                ToType(ifNullExpr, destExpression.Type),
                ToType(mapExpr, destExpression.Type));
        }
        
        private static Expression NewIfConditionFails(this Expression destinationExpresson, Func<Expression, Expression> conditionalExpression,
            Type ifInterfaceType)
        {
            var condition = conditionalExpression(destinationExpresson);
            if (condition == null || destinationExpresson.NodeType == ExpressionType.Default)
                return destinationExpresson.Type.NewExpr(ifInterfaceType);
            return Condition(condition, destinationExpresson, destinationExpresson.Type.NewExpr(ifInterfaceType));
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
            return ExpressionExtensions.ToType(newExpr, baseType);
        }

        public delegate LambdaExpression MapItem(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Type sourceType, Type destType);

        internal static LambdaExpression MapItemExpr(this TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Type sourceType, Type destType)
        {
            var sourceElementType = TypeHelper.GetElementType(sourceType);
            var destElementType = TypeHelper.GetElementType(destType);

            var typePair = new TypePair(sourceElementType, destElementType);

            var itemParam = Parameter(sourceElementType, "item");
            var itemContextParam = Parameter(typeof(ResolutionContext), "itemContext");
            var itemExpr = MapExpression(typeMapRegistry, configurationProvider, propertyMap, itemParam, itemContextParam, typePair);
            return Lambda(itemExpr, itemParam, itemContextParam);
        }

        internal static LambdaExpression MapKeyPairValueExpr(this TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Type sourceType, Type destType)
        {
            var sourceElementTypes = TypeHelper.GetElementTypes(sourceType, ElemntTypeFlags.BreakKeyValuePair);
            var destElementTypes = TypeHelper.GetElementTypes(destType, ElemntTypeFlags.BreakKeyValuePair);

            var typePairKey = new TypePair(sourceElementTypes[0], destElementTypes[0]);
            var typePairValue = new TypePair(sourceElementTypes[1], destElementTypes[1]);

            var sourceElementType = TypeHelper.GetElementType(sourceType);
            var destElementType = TypeHelper.GetElementType(destType);
            var itemParam = Parameter(sourceElementType, "item");
            var itemContextParam = Parameter(typeof(ResolutionContext), "itemContext");

            var keyExpr = MapExpression(typeMapRegistry, configurationProvider, propertyMap, Property(itemParam, "Key"), itemContextParam, typePairKey);
            var valueExpr = MapExpression(typeMapRegistry, configurationProvider, propertyMap, Property(itemParam, "Value"), itemContextParam, typePairValue);
            var keyPair = New(destElementType.GetConstructors().First(), keyExpr, valueExpr);
            return Lambda(keyPair, itemParam, itemContextParam);
        }

        private static Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Expression itemParam, Expression itemContextParam, TypePair typePair)
        {
            Expression itemExpr;
            var typeMap = configurationProvider.ResolveTypeMap(typePair);
            if (typeMap != null && (typeMap.TypeConverterType != null || typeMap.CustomMapper != null))
            {
                if (!typeMap.Sealed)
                    typeMap.Seal(typeMapRegistry, configurationProvider);
                return typeMap.MapExpression.ReplaceParameters(itemParam, Default(typePair.DestinationType), itemContextParam);
            }
            var match = configurationProvider.GetMappers().FirstOrDefault(m => m.IsMatch(typePair));
            if (match != null)
            {
                itemExpr =
                    ExpressionExtensions.ToType(
                        match.MapExpression(typeMapRegistry, configurationProvider, propertyMap, itemParam,
                            Default(typePair.DestinationType), itemContextParam), typePair.DestinationType);
            }
            else
            {
                var mapMethod =
                    typeof (ResolutionContext).GetDeclaredMethods()
                        .First(m => m.Name == "Map")
                        .MakeGenericMethod(typePair.SourceType, typePair.DestinationType);
                itemExpr = Call(itemContextParam, mapMethod, itemParam, Default(typePair.DestinationType));
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