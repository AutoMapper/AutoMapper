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

        internal static object MapCollection(this ResolutionContext context, Expression conditionalExpression, Type ifInterfaceType, MethodInfo itemFunc, Type destinationType = null, object destinationValue = null, object sourceValue = null)
        {
            return null;
            //if (destinationType == null)
            //{
            //    destinationType = context.DestinationType;
            //    destinationValue = context.DestinationValue;
            //}
            //Type sourceType;
            //if (sourceValue == null)
            //{
            //    sourceType = context.SourceType;
            //    sourceValue = context.SourceValue;
            //}
            //else
            //    sourceType = sourceValue.GetType();
            //var newExpr = destinationType.NewIfConditionFails(d => conditionalExpression, ifInterfaceType);
            //var sourceElementType = TypeHelper.GetElementType(sourceType);
            //var destElementType = TypeHelper.GetElementType(destinationType);
            //var item = Parameter(sourceElementType, "item");
            //var itemContext = Parameter(typeof (ResolutionContext), "itemContext");
            //var genericItemFunc = Lambda(Call(itemFunc.MakeGenericMethod(sourceElementType, destElementType), item, itemContext), item, itemContext);

            //return
            //    MapMethodInfo.MakeGenericMethod(sourceType, sourceElementType, destinationType, destElementType)
            //        .Invoke(null, new[] { sourceValue, destinationValue, context, newExpr.Compile(), genericItemFunc.Compile() });
        }

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
            blockExprs.Add(Assign(itemContext, New(typeof(ResolutionContext).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First(), contextExpression)));

            var cast = typeof(Enumerable).GetMethod("Cast", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(itemExpr.Parameters[0].Type);

            var addMethod = typeof(ICollection<>).MakeGenericType(TypeHelper.GetElementType(destExpression.Type)).GetMethod("Add");
            if (!sourceExpression.Type.IsGenericType)
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

        public static Expression MapCollectionExpression(this TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var sourceElementType = TypeHelper.GetElementType(sourceExpression.Type);
            var destElementType = TypeHelper.GetElementType(destExpression.Type);

            var typePair = new TypePair(sourceElementType, destElementType);

            var destCollectionType = destExpression.Type;
            var destICollectionType = typeof(ICollection<>).MakeGenericType(destElementType);

            var listType = typeof(List<>).MakeGenericType(destElementType);
            var newExpr = ToType(
                destExpression.Type.IsInterface()
                    ? New(listType)
                    : DelegateFactory.GenerateConstructorExpression(destExpression.Type),
                destICollectionType);
            var ifNullExpr = configurationProvider.AllowNullCollections
                     ? Constant(null, destCollectionType)
                     : newExpr;

            var itemParam = Parameter(sourceElementType, "item");
            var itemContextParam = Parameter(typeof(ResolutionContext), "itemContext");

            var blockExprs = new List<Expression>();
            var blockParams = new List<ParameterExpression>();
            if (destCollectionType.ImplementsGenericInterface(typeof(ICollection<>)))
            {
                if (propertyMap == null)
                {
                    var destParam = Parameter(newExpr.Type, "dest");
                    blockParams.Add(destParam);

                    blockExprs.Add(Assign(destParam, destExpression));

                    destExpression = destParam;

                    blockExprs.Add(IfThenElse(NotEqual(destExpression, Constant(null)),
                        Call(destExpression, destICollectionType.GetMethod("Clear")),
                        Assign(destParam, newExpr)
                        ));
                }
                else if (propertyMap.UseDestinationValue)
                {
                    blockExprs.Add(Call(destExpression, destICollectionType.GetMethod("Clear")));
                }
                else
                {
                    var destParam = Parameter(newExpr.Type, "dest");
                    blockParams.Add(destParam);
                    blockExprs.Add(Assign(destParam, newExpr));
                    destExpression = destParam;
                }
            }
            else
            {
                var destParam = Parameter(newExpr.Type, "dest");
                blockParams.Add(destParam);
                blockExprs.Add(Assign(destParam, newExpr));
                destExpression = destParam;
            }

            Expression itemExpr;

            var match = configurationProvider.GetMappers().FirstOrDefault(m => m.IsMatch(typePair));
            var expressionMapper = match as IObjectMapExpression;
            if (expressionMapper != null)
            {
                itemExpr = expressionMapper.MapExpression(typeMapRegistry, configurationProvider, propertyMap, itemParam, Default(destElementType), itemContextParam);
            }
            else
            {
                var resContextCtor = typeof(ResolutionContext).GetTypeInfo().DeclaredConstructors.Single(ci => ci.GetParameters().Length == 1);
                blockExprs.Add(Assign(itemContextParam, New(resContextCtor, contextExpression)));

                var mapMethod = typeof(ResolutionContext).GetTypeInfo().DeclaredMethods.First(m => m.Name == "Map");

                blockParams.Add(itemContextParam);
                itemExpr = ToType(Call(itemContextParam, mapMethod,
                    ToType(itemParam, typeof(object)),
                    ToType(Default(destElementType), typeof(object)),
                    Constant(sourceElementType),
                    Constant(destElementType)
                ), destElementType);
            }

            var addMethod = destICollectionType.GetMethod("Add");
            blockExprs.Add(ForEach(sourceExpression, itemParam, Call(
                destExpression,
                addMethod,
                itemExpr)));

            blockExprs.Add(destExpression);

            var mapExpr = Block(blockParams, blockExprs);

            return Condition(
                Equal(sourceExpression, Constant(null)),
                ToType(ifNullExpr, destCollectionType),
                ToType(mapExpr, destCollectionType));
        }
        private static Expression NewIfConditionFails(this Expression destinationExpresson, Func<Expression, Expression> conditionalExpression,
            Type ifInterfaceType)
        {
            var condition = conditionalExpression(destinationExpresson);
            if (condition == null)
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