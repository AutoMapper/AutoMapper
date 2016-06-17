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

    public class ArrayMapper : IObjectMapper
    {
        private static readonly MethodInfo MapMethodInfo = typeof(ArrayMapper).GetAllMethods().First(_ => _.IsStatic);
        
        public static TDestination[] Map<TSource, TDestination>(IEnumerable<TSource> source, ResolutionContext context, Func<TSource, ResolutionContext, TDestination> newItemFunc)
        {
            var itemContext = new ResolutionContext(context);

            return source.Select(item => newItemFunc(item, itemContext))
                .ToArray();
        }

        public bool IsMatch(TypePair context)
        {
            return (context.DestinationType.IsArray) && (context.SourceType.IsEnumerableType());
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            var sourceElementType = TypeHelper.GetElementType(sourceExpression.Type);
            var destElementType = TypeHelper.GetElementType(destExpression.Type);

            if (destExpression.Type.IsAssignableFrom(sourceExpression.Type)
                && typeMapRegistry.GetTypeMap(new TypePair(sourceElementType, destElementType)) == null)
            {
                // return (TDestination[]) source;
                var convertExpr = Convert(sourceExpression, destElementType.MakeArrayType());

                if (configurationProvider.AllowNullCollections)
                    return convertExpr;

                // return (TDestination[]) source ?? new TDestination[0];
                return Coalesce(convertExpr, NewArrayBounds(destElementType, Constant(0)));
            }

            var ifNullExpr = configurationProvider.AllowNullCollections
                                 ? (Expression) Constant(null)
                                 : NewArrayBounds(destElementType, Constant(0));
            var itemExpr = typeMapRegistry.MapItemExpr(configurationProvider, propertyMap, sourceExpression.Type, destExpression.Type);

            var mapExpr = Call(null, MapMethodInfo.MakeGenericMethod(sourceElementType, destElementType), sourceExpression, contextExpression, itemExpr);

            // return (source == null) ? ifNullExpr : Map<TSourceElement, TDestElement>(source, context);
            return Condition(Equal(sourceExpression, Constant(null)), ifNullExpr, mapExpr);
        }

    }
}