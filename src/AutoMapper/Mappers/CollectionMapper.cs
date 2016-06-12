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

    public class CollectionMapper : IObjectMapExpression
    {
        public static TDestination Map<TSource, TSourceItem, TDestination, TDestinationItem>(TSource source, TDestination destination, ResolutionContext context)
            where TSource : IEnumerable
            where TDestination : class, ICollection<TDestinationItem>
        {
            if (source == null && context.Mapper.ShouldMapSourceCollectionAsNull(context))
                return null;

            TDestination list = destination ?? (
                typeof(TDestination).IsInterface()
                    ? new List<TDestinationItem>() as TDestination
                    : (TDestination)(context.ConfigurationProvider.AllowNullDestinationValues
                ? ObjectCreator.CreateNonNullValue(typeof(TDestination))
                : ObjectCreator.CreateObject(typeof(TDestination))));

            list.Clear();
            var itemContext = new ResolutionContext(context);
            foreach (var item in (IEnumerable)source ?? Enumerable.Empty<object>())
                list.Add((TDestinationItem)itemContext.Map(item, default(TDestinationItem), typeof(TSourceItem), typeof(TDestinationItem)));

            return list;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(CollectionMapper).GetAllMethods().First(_ => _.IsStatic);

        public object Map(ResolutionContext context)
        {
            return
                MapMethodInfo.MakeGenericMethod(context.SourceType, TypeHelper.GetElementType(context.SourceType), context.DestinationType, TypeHelper.GetElementType(context.DestinationType))
                    .Invoke(null, new[] { context.SourceValue, context.DestinationValue, context });
        }

        public bool IsMatch(TypePair context)
        {
            var isMatch = context.SourceType.IsEnumerableType() && context.DestinationType.IsCollectionType();

            return isMatch;
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {

            var sourceElementType = TypeHelper.GetElementType(sourceExpression.Type);
            var destElementType = TypeHelper.GetElementType(destExpression.Type);

            var typePair = new TypePair(sourceElementType, destElementType);
            var typeMap = typeMapRegistry.GetTypeMap(typePair);


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
            var blockParams = new List<ParameterExpression>();
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
    }
}