using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using System;
    using System.Reflection;
    using Configuration;
    using static Expression;
    using static ExpressionExtensions;

    public class ArrayMapper : IObjectMapper
    {
        private static readonly MethodInfo MapMethodInfo = typeof(ArrayMapper).GetAllMethods().First(_ => _.IsStatic);
        
        public static TDestination[] Map<TSource, TDestination>(IEnumerable<TSource> source, ResolutionContext context, Func<TSource, ResolutionContext, TDestination> newItemFunc)
        {
            var count = source.Count();
            var array = new TDestination[count];

            int i = 0;
            foreach (var item in source)
                array[i++] = newItemFunc(item, context);
            return array;
        }

        public bool IsMatch(TypePair context)
        {
            return (context.DestinationType.IsArray) && (context.SourceType.IsEnumerableType());
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var sourceElementType = TypeHelper.GetElementType(sourceExpression.Type);
            var destElementType = TypeHelper.GetElementType(destExpression.Type);

            if (destExpression.Type.IsAssignableFrom(sourceExpression.Type) && configurationProvider.ResolveTypeMap(sourceElementType, destElementType) == null)
            {
                // return (TDestination[]) source;
                var convertExpr = Convert(sourceExpression, destElementType.MakeArrayType());

                if (configurationProvider.Configuration.AllowNullCollections)
                    return convertExpr;

                // return (TDestination[]) source ?? new TDestination[0];
                return Coalesce(convertExpr, NewArrayBounds(destElementType, Constant(0)));
            }

            var ifNullExpr = configurationProvider.Configuration.AllowNullCollections
                                 ? (Expression) Constant(null, destExpression.Type)
                                 : NewArrayBounds(destElementType, Constant(0));

            var itemParam = Parameter(sourceElementType, "item");
            var itemExpr = typeMapRegistry.MapItemExpr(configurationProvider, propertyMap, sourceExpression.Type, destExpression.Type, itemParam, contextExpression);

            //var count = source.Count();
            //var array = new TDestination[count];

            //int i = 0;
            //foreach (var item in source)
            //    array[i++] = newItemFunc(item, context);
            //return array;

            var countParam = Parameter(typeof(int), "count");
            var arrayParam = Parameter(destExpression.Type, "destinationArray");
            var indexParam = Parameter(typeof(int), "destinationArrayIndex");

            var actions = new List<Expression>();
            var parameters = new List<ParameterExpression> { countParam, arrayParam, indexParam };

            var countMethod = typeof(Enumerable)
                .GetTypeInfo()
                .DeclaredMethods
                .Single(mi => mi.Name == "Count" && mi.GetParameters().Length == 1)
                .MakeGenericMethod(sourceElementType);
            actions.Add(Assign(countParam, Call(countMethod, sourceExpression)));
            actions.Add(Assign(arrayParam, NewArrayBounds(destElementType, countParam)));
            actions.Add(Assign(indexParam, Constant(0)));
            actions.Add(ForEach(sourceExpression, itemParam,
                Assign(ArrayAccess(arrayParam, PostIncrementAssign(indexParam)), itemExpr)
                ));
            actions.Add(arrayParam);

            var mapExpr = Block(parameters, actions);

            // return (source == null) ? ifNullExpr : Map<TSourceElement, TDestElement>(source, context);
            return Condition(Equal(sourceExpression, Constant(null)), ifNullExpr, mapExpr);
        }

    }
}