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
        public static TDestination Map<TSource, TSourceItem, TDestination, TDestinationItem>(TSource source, TDestination destination, ResolutionContext context, Func<TDestination> newDestination, Func<TSourceItem, ResolutionContext, TDestinationItem> addFunc)
            where TSource : IEnumerable
            where TDestination : class, ICollection<TDestinationItem>
        {
            if (source == null && context.Mapper.ShouldMapSourceCollectionAsNull(context))
                return null;
            if(addFunc == null)
                addFunc = (item, resolutionContext) => (TDestinationItem)resolutionContext.Map(item, null, typeof(TSourceItem), typeof(TDestinationItem));

            TDestination list = destination ?? newDestination();

            list.Clear();
            var itemContext = new ResolutionContext(context);
            foreach (var item in (IEnumerable) source ?? Enumerable.Empty<object>())
                list.Add(addFunc((TSourceItem)item, itemContext));

            return list;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(CollectionMapper).GetAllMethods().First(_ => _.IsStatic);

        public object Map(ResolutionContext context)
        {
            var makeGenericType = typeof(List<>).MakeGenericType(context.DestinationType);
            var newExpr = context.DestinationType.IsInterface()
                   ? New(makeGenericType)
                   : DelegateFactory.GenerateConstructorExpression(context.DestinationType);

            return
                MapMethodInfo.MakeGenericMethod(context.SourceType, TypeHelper.GetElementType(context.SourceType), context.DestinationType, TypeHelper.GetElementType(context.DestinationType))
                    .Invoke(null, new[] {context.SourceValue, context.DestinationValue, context, Lambda(newExpr).Compile(), null});
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

            var destCollectionType = destExpression.Type;

            var makeGenericType = typeof(List<>).MakeGenericType(destElementType);
            var newExpr = destExpression.Type.IsInterface()
                ? New(makeGenericType)
                : DelegateFactory.GenerateConstructorExpression(destExpression.Type);
            var ifNullExpr = configurationProvider.AllowNullCollections
                     ? Constant(null, destCollectionType)
                     : newExpr;

            var itemParam = Parameter(sourceElementType, "item");
            var itemContextParam = Parameter(typeof(ResolutionContext), "itemContext");

            var blockExprs = new List<Expression>();
            var blockParams = new List<ParameterExpression>();
            

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
                itemExpr = Convert(Call(itemContextParam, mapMethod,
                    Convert(itemParam, typeof(object)),
                    Convert(Default(destElementType), typeof(object)),
                    Constant(sourceElementType),
                    Constant(destElementType)),
                    destElementType);
            }
            blockExprs.Add(destExpression);
            
            return Call(null, 
                MapMethodInfo.MakeGenericMethod(sourceExpression.Type, TypeHelper.GetElementType(sourceExpression.Type), destExpression.Type, TypeHelper.GetElementType(destExpression.Type)),
                    sourceExpression, destExpression, contextExpression, Constant(Lambda(newExpr).Compile()), Constant(Lambda(itemExpr, itemParam, itemContextParam).Compile()));
        }
    }
}