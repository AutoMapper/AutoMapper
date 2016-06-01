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

    public class ArrayMapper : IObjectMapExpression
    {
        public static TDestinationElement[] Map<TSource, TSourceElement, TDestinationElement>(TSource source, ResolutionContext context)
            where TSource : IEnumerable
        {
            if (source == null)
                return context.Mapper.ShouldMapSourceCollectionAsNull(context) ? null : new TDestinationElement[0];
            
            if (context.DestinationType.IsAssignableFrom(context.SourceType))
            {
                var elementTypeMap = context.ConfigurationProvider.ResolveTypeMap(typeof(TSourceElement), typeof(TDestinationElement));
                if (elementTypeMap == null)
                    return source as TDestinationElement[];
            }

            var itemContext = new ResolutionContext(context);

            return source.Cast<TSourceElement>()
                .Select(item => (TDestinationElement) itemContext.Map(item, null, typeof(TSourceElement), typeof(TDestinationElement)))
                .ToArray();
        }

        private static readonly MethodInfo MapMethodInfo = typeof(ArrayMapper).GetAllMethods().First(_ => _.IsStatic);
        private static readonly MethodInfo Map2MethodInfo;

        static ArrayMapper()
        {
            Expression<Func<IEnumerable<object>, ResolutionContext, object[]>> expr =
                (source, context) => Map<object, object>(source, context);

            Map2MethodInfo = ((MethodCallExpression) expr.Body).Method.GetGenericMethodDefinition();
        }

        private static TDestination[] Map<TSource, TDestination>(IEnumerable<TSource> source, ResolutionContext context)
        {
            var itemContext = new ResolutionContext(context);

            return source.Select(item => (TDestination)itemContext.Map(item, null, typeof(TSource), typeof(TDestination)))
                .ToArray();
        }

        public object Map(ResolutionContext context)
        {
            return MapMethodInfo.MakeGenericMethod(context.SourceType, TypeHelper.GetElementType(context.SourceType, (IEnumerable)context.SourceValue), TypeHelper.GetElementType(context.DestinationType, (IEnumerable)context.DestinationValue)).Invoke(null, new [] { context.SourceValue, context });
        }

        public bool IsMatch(TypePair context)
        {
            return (context.DestinationType.IsArray) && (context.SourceType.IsEnumerableType());
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, Expression sourceExpression, Expression destExpression, Expression contextExpression)
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

            var mapExpr = Call(null, Map2MethodInfo.MakeGenericMethod(sourceElementType, destElementType), sourceExpression, contextExpression);

            // return (source == null) ? ifNullExpr : Map<TSourceElement, TDestElement>(source, context);
            return Condition(Equal(sourceExpression, Constant(null)), ifNullExpr, mapExpr);
        }

    }
}