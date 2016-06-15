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

    public class CollectionMapper :  IObjectMapExpression
    {
        public static TDestination Map<TSource, TSourceItem, TDestination, TDestinationItem>(TSource source, TDestination destination, ResolutionContext context, Func<TDestination, TDestination> newDestination, Func<TSourceItem, ResolutionContext, TDestinationItem> addFunc)
            where TSource : IEnumerable
            where TDestination : class, ICollection<TDestinationItem>
        {
            if (source == null && context.Mapper.ShouldMapSourceCollectionAsNull(context))
                return null;
            if(addFunc == null)
                addFunc = (item, resolutionContext) => resolutionContext.Map(item, default(TDestinationItem));

            TDestination list =  newDestination(destination);

            list.Clear();
            var itemContext = new ResolutionContext(context);
            foreach (var item in source != null ? source.Cast<TSourceItem>() : Enumerable.Empty<TSourceItem>())
                list.Add(addFunc(item, itemContext));

            return list;
        }

        internal static Expression NewExpr(Type destinationType, Type ifInterfaceType)
        {
            var destElementType = TypeHelper.GetElementType(destinationType);
            var newExpr = destinationType.IsInterface()
                ? New(ifInterfaceType.MakeGenericType(destElementType))
                : DelegateFactory.GenerateConstructorExpression(destinationType);
            return Convert(newExpr, destinationType);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(CollectionMapper).GetAllMethods().First(_ => _.IsStatic);

        public object Map(ResolutionContext context)
        {
            return MapBase(context, IfNotNull(Constant(context.DestinationValue)), typeof(List<>));
        }

        internal static object MapBase(ResolutionContext context, BinaryExpression conditionalExpression, Type ifInterfaceType)
        {
            var d = Parameter(context.DestinationType, "dest");
            var newExpr = Condition(conditionalExpression, d, NewExpr(context.DestinationType, ifInterfaceType));

            return
                MapMethodInfo.MakeGenericMethod(context.SourceType, TypeHelper.GetElementType(context.SourceType), context.DestinationType, TypeHelper.GetElementType(context.DestinationType))
                    .Invoke(null, new[] { context.SourceValue, context.DestinationValue, context, Lambda(newExpr, d).Compile(), null });
        }

        public bool IsMatch(TypePair context)
        {
            var isMatch = context.SourceType.IsEnumerableType() && context.DestinationType.IsCollectionType();

            return isMatch;
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return MapExpressionBase(typeMapRegistry, configurationProvider, propertyMap, sourceExpression, destExpression, contextExpression, IfNotNull, typeof(List<>));
        }

        internal static BinaryExpression IfNotNull(Expression destExpression)
        {
            return NotEqual(destExpression, Constant(null));
        }

        internal static Expression MapExpressionBase(TypeMapRegistry typeMapRegistry,
            IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression,
            Expression destExpression, Expression contextExpression, Func<Expression, BinaryExpression> conditionalExpression, Type ifInterfaceType)
        {
            var d = Parameter(destExpression.Type, "d");
            var newExpr = Condition(conditionalExpression(d), d, NewExpr(destExpression.Type, ifInterfaceType));
            //var ifNullExpr = configurationProvider.AllowNullCollections
            //         ? Constant(null, destExpression.Type)
            //         : newExpr;
            var b = Lambda(newExpr, d);
            var itemExpr = ItemExpr(typeMapRegistry, configurationProvider, propertyMap, sourceExpression, destExpression);

            return Call(null,
                MapMethodInfo.MakeGenericMethod(sourceExpression.Type, TypeHelper.GetElementType(sourceExpression.Type),
                    destExpression.Type, TypeHelper.GetElementType(destExpression.Type)),
                sourceExpression, destExpression, contextExpression, Constant(b.Compile()),
                Constant(itemExpr.Compile()));
        }

        private static LambdaExpression ItemExpr(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider,
            PropertyMap propertyMap, Expression sourceExpression, Expression destExpression)
        {

            var sourceElementType = TypeHelper.GetElementType(sourceExpression.Type);
            var destElementType = TypeHelper.GetElementType(destExpression.Type);

            var typePair = new TypePair(sourceElementType, destElementType);

            var itemParam = Parameter(sourceElementType, "item");
            var itemContextParam = Parameter(typeof(ResolutionContext), "itemContext");
            Expression itemExpr;

            var match = configurationProvider.GetMappers().FirstOrDefault(m => m.IsMatch(typePair));
            var expressionMapper = match as IObjectMapExpression;
            if (expressionMapper != null)
            {
                itemExpr = expressionMapper.MapExpression(typeMapRegistry, configurationProvider, propertyMap, itemParam,
                    Default(destElementType), itemContextParam);
            }
            else
            {
                var mapMethod = typeof(ResolutionContext).GetDeclaredMethods().First(m => m.Name == "Map").MakeGenericMethod(sourceElementType, destElementType);
                itemExpr = Call(
                    itemContextParam,
                    mapMethod,
                    itemParam,
                    Default(destElementType)
                    );
            }
            return Lambda(itemExpr, itemParam, itemContextParam);
        }
    }
}