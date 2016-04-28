using System.Collections;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Configuration;

    public class CollectionMapper : IObjectMapper, IObjectMapExpression
    {
        public static ICollection<TDestinationItem> Map<TSource, TSourceItem, TDestination, TDestinationItem>(TSource source, TDestination destination, ResolutionContext context)
            where TSource : IEnumerable
            where TDestination : class, ICollection<TDestinationItem>
        {
            if (source == null && context.Mapper.ShouldMapSourceCollectionAsNull(context))
                return null;

            ICollection<TDestinationItem> list = destination ?? (
                typeof (TDestination).IsInterface()
                    ? new List<TDestinationItem>()
                    : (ICollection<TDestinationItem>) context.Mapper.CreateObject(context));

            list.Clear();

            foreach (var item in (IEnumerable) source ?? Enumerable.Empty<object>())
                list.Add((TDestinationItem)context.Mapper.Map(item, default(TDestinationItem), typeof(TSourceItem), typeof(TDestinationItem), context));

            return list;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(CollectionMapper).GetMethod("Map");

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

        public Expression MapExpression(Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(Expression.Constant(null), 
                MapMethodInfo.MakeGenericMethod(sourceExpression.Type, TypeHelper.GetElementType(sourceExpression.Type), destExpression.Type, TypeHelper.GetElementType(destExpression.Type)),
                    sourceExpression, destExpression, contextExpression);
        }
    }
}