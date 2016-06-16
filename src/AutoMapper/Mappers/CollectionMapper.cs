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

        public static TDestination Map<TSource, TSourceItem, TDestination, TDestinationItem>(TSource source, TDestination destination, ResolutionContext context, Func<TDestination, TDestination> newDestination, Func<TSourceItem, ResolutionContext, TDestinationItem> addFunc)
            where TSource : IEnumerable
            where TDestination : class, ICollection<TDestinationItem>
        {
            if (source == null && context.Mapper.ShouldMapSourceCollectionAsNull(context))
                return null;
            if (addFunc == null)
                addFunc = (item, resolutionContext) => resolutionContext.Map(item, default(TDestinationItem));

            TDestination list = newDestination(destination);

            list.Clear();
            var itemContext = new ResolutionContext(context);
            foreach (var item in source != null ? source.Cast<TSourceItem>() : Enumerable.Empty<TSourceItem>())
                list.Add(addFunc(item, itemContext));

            return list;
        }

        internal static object MapCollection(this ResolutionContext context, Expression conditionalExpression, Type ifInterfaceType, Type destinationType = null, object destinationValue = null)
        {
            if (destinationType == null)
            {
                destinationType = context.DestinationType;
                destinationValue = context.DestinationValue;
            }
            var newExpr = destinationType.NewIfConditionFails(d => conditionalExpression, ifInterfaceType);

            return
                MapMethodInfo.MakeGenericMethod(context.SourceType, TypeHelper.GetElementType(context.SourceType), destinationType, TypeHelper.GetElementType(context.DestinationType))
                    .Invoke(null, new[] { context.SourceValue, destinationValue, context, newExpr.Compile(), null });
        }

        internal static Expression MapCollectionExpression(this TypeMapRegistry typeMapRegistry,
           IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression,
           Expression destExpression, Expression contextExpression, Func<Expression, Expression> conditionalExpression, Type ifInterfaceType)
        {
            var newExpr = destExpression.Type.NewIfConditionFails(conditionalExpression, ifInterfaceType);
            var itemExpr = ItemExpr(typeMapRegistry, configurationProvider, propertyMap, sourceExpression.Type, destExpression.Type);

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

        internal static LambdaExpression ItemExpr(this TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Type sourceType, Type destType)
        {
            var sourceElementType = TypeHelper.GetElementType(sourceType);
            var destElementType = TypeHelper.GetElementType(destType);

            var typePair = new TypePair(sourceElementType, destElementType);

            var itemParam = Parameter(sourceElementType, "item");
            var itemContextParam = Parameter(typeof(ResolutionContext), "itemContext");
            Expression itemExpr;

            var match = configurationProvider.GetMappers().FirstOrDefault(m => m.IsMatch(typePair));
            var expressionMapper = match as IObjectMapExpression;
            if (expressionMapper != null)
            {
                itemExpr = ExpressionExtensions.ToType(expressionMapper.MapExpression(typeMapRegistry, configurationProvider, propertyMap, itemParam,
                    Default(destElementType), itemContextParam), destElementType);
            }
            else
            {
                var mapMethod = typeof(ResolutionContext).GetDeclaredMethods().First(m => m.Name == "Map").MakeGenericMethod(sourceElementType, destElementType);
                itemExpr = Call(itemContextParam,mapMethod,itemParam,Default(destElementType));
            }
            return Lambda(itemExpr, itemParam, itemContextParam);
        }

        internal static BinaryExpression IfNotNull(Expression destExpression)
        {
            return NotEqual(destExpression, Constant(null));
        }
    }

    public class CollectionMapper :  IObjectMapExpression
    {
        public object Map(ResolutionContext context)
            => context.MapCollection(CollectionMapperExtensions.IfNotNull(Constant(context.DestinationValue)), typeof(List<>));
        
        public bool IsMatch(TypePair context) => context.SourceType.IsEnumerableType() && context.DestinationType.IsCollectionType();

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
            => typeMapRegistry.MapCollectionExpression(configurationProvider, propertyMap, sourceExpression, destExpression, contextExpression, CollectionMapperExtensions.IfNotNull, typeof(List<>));
    }
}