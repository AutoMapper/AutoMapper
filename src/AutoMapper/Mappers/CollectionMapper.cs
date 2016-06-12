using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;

namespace AutoMapper.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Configuration;
    using Execution;
    using static ExpressionExtensions;

    public class CollectionMapper :  IObjectMapExpression
    {
        public static TDestination Map<TSource, TSourceItem, TDestination, TDestinationItem>(TSource source, TDestination destination, ResolutionContext context)
            where TSource : IEnumerable
            where TDestination : class, ICollection<TDestinationItem>
        {
            if (source == null && context.Mapper.ShouldMapSourceCollectionAsNull(context))
                return null;

            TDestination list = destination ?? (
                typeof (TDestination).IsInterface()
                    ? new List<TDestinationItem>() as TDestination
                    : (TDestination) (context.ConfigurationProvider.AllowNullDestinationValues
                ? ObjectCreator.CreateNonNullValue(typeof(TDestination))
                : ObjectCreator.CreateObject(typeof(TDestination))));

            list.Clear();
            var itemContext = new ResolutionContext(context);
            foreach (var item in (IEnumerable) source ?? Enumerable.Empty<object>())
                list.Add((TDestinationItem)itemContext.Map(item, default(TDestinationItem), typeof(TSourceItem), typeof(TDestinationItem)));

            return list;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(CollectionMapper).GetAllMethods().First(_ => _.IsStatic);
        private static readonly MethodInfo MapToExistingMethodInfo;
        private static readonly MethodInfo MapToNewMethodInfo;

        static CollectionMapper()
        {
            Expression<Func<IEnumerable<object>, ICollection<object>, ResolutionContext, ICollection<object>>> expr =
                (source, dest, context) => MapToExisting(source, dest, context);

            MapToExistingMethodInfo = ((MethodCallExpression)expr.Body).Method.GetGenericMethodDefinition();

            Expression<Func<IEnumerable<object>, ICollection<object>, ResolutionContext, ICollection<object>>> expr2 =
                (source, dest, context) => MapToNew(source, dest, context);

            MapToNewMethodInfo = ((MethodCallExpression)expr2.Body).Method.GetGenericMethodDefinition();
        }

        private static ICollection<TDestination> MapToExisting<TSource, TDestination>(IEnumerable<TSource> source, ICollection<TDestination> destination,
            ResolutionContext context)
        {
            destination.Clear();

            var itemContext = new ResolutionContext(context);
            foreach (var item in source)
                destination.Add((TDestination) itemContext.Map(item, default(TDestination), typeof(TSource), typeof(TDestination)));

            return destination;
        }

        private static ICollection<TDestination> MapToNew<TSource, TDestination>(IEnumerable<TSource> source,
            ICollection<TDestination> destination,
            ResolutionContext context)
        {
            var itemContext = new ResolutionContext(context);
            foreach (var item in source)
                destination.Add((TDestination)itemContext.Map(item, default(TDestination), typeof(TSource), typeof(TDestination)));

            return destination;
        }

        public object Map(ResolutionContext context)
        {
            return
                MapMethodInfo.MakeGenericMethod(context.SourceType, TypeHelper.GetElementType(context.SourceType), context.DestinationType, TypeHelper.GetElementType(context.DestinationType))
                    .Invoke(null, new[] {context.SourceValue, context.DestinationValue, context});
        }

        public bool IsMatch(TypePair context)
        {
            var isMatch = context.SourceType.IsEnumerableType() && context.DestinationType.IsCollectionType();

            return isMatch;
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            //return Expression.Call(null,
            //    MapMethodInfo.MakeGenericMethod(sourceExpression.Type, TypeHelper.GetElementType(sourceExpression.Type),
            //        destExpression.Type, TypeHelper.GetElementType(destExpression.Type)),
            //    sourceExpression, destExpression, contextExpression);

            var sourceElementType = TypeHelper.GetElementType(sourceExpression.Type);
            var destElementType = TypeHelper.GetElementType(destExpression.Type);
            var destCollectionType = destExpression.Type;

            var makeGenericType = typeof(List<>).MakeGenericType(destElementType);
            var destICollectionType = typeof(ICollection<>).MakeGenericType(destElementType);
            var newExpr = ToType(
                destExpression.Type.IsInterface()
                    ? New(makeGenericType)
                    : DelegateFactory.GenerateConstructorExpression(destExpression.Type), 
                destICollectionType);
            var ifNullExpr = configurationProvider.AllowNullCollections
                     ? Constant(null, destCollectionType)
                     : newExpr;

            var itemParam = Parameter(sourceElementType, "item");
            var itemContextParam = Parameter(typeof(ResolutionContext), "itemContext");

            var blockExprs = new List<Expression>();
            var blockParams = new List<ParameterExpression> { itemContextParam };
            if (propertyMap.UseDestinationValue)
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

            // var itemContext = new ResolutionContext(context);
            var resContextCtor = typeof(ResolutionContext).GetTypeInfo().DeclaredConstructors.Single(ci => ci.GetParameters().Length == 1);
            blockExprs.Add(Assign(itemContextParam, New(resContextCtor, contextExpression)));
            var addMethod = destICollectionType.GetMethod("Add");
            var mapMethod = typeof(ResolutionContext).GetTypeInfo().DeclaredMethods.First(m => m.Name == "Map");
            blockExprs.Add(ForEach(sourceExpression, itemParam, Call(
                destExpression,
                addMethod,
                ToType(Call(itemContextParam, mapMethod,
                    ToType(itemParam, typeof(object)),
                    ToType(Default(destElementType), typeof(object)),
                    Constant(sourceElementType),
                    Constant(destElementType)
                ), destElementType))));

            blockExprs.Add(destExpression);
/*
 *          var itemContext = new ResolutionContext(context);
            foreach (var item in source)
                destination.Add((TDestination)itemContext.Map(item, default(TDestination), typeof(TSource), typeof(TDestination)));

            return destination;

 */

            //var mapExpr = Call(null, toUse.MakeGenericMethod(sourceElementType, destElementType),
            //                      sourceExpression, destExpression, contextExpression);

            var mapExpr = Block(blockParams, blockExprs);

            return Condition(
                Equal(sourceExpression, Constant(null)), 
                ToType(ifNullExpr, destCollectionType), 
                ToType(mapExpr, destCollectionType));
        }
    }
}