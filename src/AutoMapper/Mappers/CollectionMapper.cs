using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Execution;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.Mappers
{
    using System.Collections.Generic;
    using System.Reflection;
    using Configuration;

    public static class CollectionMapperExtensions
    {
        internal static readonly MethodInfo MapMethodInfo = typeof(CollectionMapperExtensions).GetAllMethods().First(_ => _.IsStatic);

        internal static readonly MethodInfo MapItemMethodInfo = typeof(CollectionMapperExtensions).GetAllMethods().Where(_ => _.IsStatic).ElementAt(1);
        internal static readonly MethodInfo MapKeyValuePairMethodInfo = typeof(CollectionMapperExtensions).GetAllMethods().Where(_ => _.IsStatic).ElementAt(2);

        public static TDestination Map<TSource, TSourceItem, TDestination, TDestinationItem>(TSource source, TDestination destination, ResolutionContext context, Func<TDestination, TDestination> newDestination, Func<TSourceItem, ResolutionContext, TDestinationItem> addFunc)
            where TSource : IEnumerable
            where TDestination : class, ICollection<TDestinationItem>
        {
            if (source == null && context.Mapper.ShouldMapSourceCollectionAsNull(context))
                return null;

            TDestination list = newDestination(destination);

            list.Clear();
            var itemContext = new ResolutionContext(context);
            foreach (var item in source != null ? source.Cast<TSourceItem>() : Enumerable.Empty<TSourceItem>())
                list.Add(addFunc(item, itemContext));

            return list;
        }
        
        public static TDestinationItem MapItemFunc<TSourceItem,TDestinationItem>(TSourceItem item, ResolutionContext resolutionContext)
        {
            return resolutionContext.Map(item, default(TDestinationItem));
        }
        public static TDestinationItem MapKeyValuePairFunc<TSourceItem, TDestinationItem>(TSourceItem item, ResolutionContext resolutionContext)
        {
            return resolutionContext.Map(item, default(TDestinationItem));
        }

        internal static object MapCollection(this ResolutionContext context, Expression conditionalExpression, Type ifInterfaceType, MethodInfo itemFunc, Type destinationType = null, object destinationValue = null)
        {
            if (destinationType == null)
            {
                destinationType = context.DestinationType;
                destinationValue = context.DestinationValue;
            }
            var newExpr = destinationType.NewIfConditionFails(d => conditionalExpression, ifInterfaceType);
            var sourceElementType = TypeHelper.GetElementType(context.SourceType);
            var destElementType = TypeHelper.GetElementType(destinationType);
            var item = Parameter(sourceElementType, "item");
            var itemContext = Parameter(typeof (ResolutionContext), "itemContext");
            var genericItemFunc = Lambda(Call(itemFunc.MakeGenericMethod(sourceElementType, destElementType), item, itemContext), item, itemContext);

            return
                MapMethodInfo.MakeGenericMethod(context.SourceType, TypeHelper.GetElementType(context.SourceType), destinationType, TypeHelper.GetElementType(context.DestinationType))
                    .Invoke(null, new[] { context.SourceValue, destinationValue, context, newExpr.Compile(), genericItemFunc.Compile() });
        }

        internal static Expression MapCollectionExpression(this TypeMapRegistry typeMapRegistry,
           IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression,
           Expression destExpression, Expression contextExpression, Func<Expression, Expression> conditionalExpression, Type ifInterfaceType, MapItem mapItem)
        {
            var newExpr = destExpression.Type.NewIfConditionFails(conditionalExpression, ifInterfaceType);
            var itemExpr = mapItem(typeMapRegistry, configurationProvider, propertyMap, sourceExpression.Type, destExpression.Type);

            return Call(null,
                MapMethodInfo.MakeGenericMethod(sourceExpression.Type, TypeHelper.GetElementType(sourceExpression.Type),
                    destExpression.Type, TypeHelper.GetElementType(destExpression.Type)),
                sourceExpression, destExpression, contextExpression, Constant(newExpr.Compile()),
                Constant(itemExpr.Compile()));
        }

        private static LambdaExpression NewIfConditionFails(this Type destinationType, Func<Expression, Expression> conditionalExpression,
            Type ifInterfaceType)
        {
            var dest = Parameter(destinationType, "dest");
            var condition = conditionalExpression(dest);
            if (condition == null)
                return Lambda(destinationType.NewExpr(ifInterfaceType), dest);
            return Lambda(Condition(condition, dest, destinationType.NewExpr(ifInterfaceType)), dest);
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

            var match = configurationProvider.GetMappers().FirstOrDefault(m => m.IsMatch(typePair));
            var expressionMapper = match as IObjectMapExpression;
            if (expressionMapper != null)
            {
                itemExpr =
                    ExpressionExtensions.ToType(
                        expressionMapper.MapExpression(typeMapRegistry, configurationProvider, propertyMap, itemParam,
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

    public class CollectionMapper :  IObjectMapExpression
    {
        public object Map(ResolutionContext context)
            => context.MapCollection(CollectionMapperExtensions.IfNotNull(Constant(context.DestinationValue)), typeof(List<>), CollectionMapperExtensions.MapItemMethodInfo);
        
        public bool IsMatch(TypePair context) => context.SourceType.IsEnumerableType() && context.DestinationType.IsCollectionType();

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
            => typeMapRegistry.MapCollectionExpression(configurationProvider, propertyMap, sourceExpression, destExpression, contextExpression, CollectionMapperExtensions.IfNotNull, typeof(List<>), CollectionMapperExtensions.MapItemExpr);
    }
}