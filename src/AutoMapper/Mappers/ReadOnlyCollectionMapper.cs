using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Configuration;

    public class ReadOnlyCollectionMapper : IObjectMapExpression
    {
        public static ReadOnlyCollection<TDestinationItem> Map<TSource, TSourceItem, TDestinationItem>(TSource source, ResolutionContext context)
            where TSource : IEnumerable<TSourceItem>
        {
            if (source == null && context.Mapper.ShouldMapSourceCollectionAsNull(context))
                return null;

            IList<TDestinationItem> list = new List<TDestinationItem>();

            var itemContext = new ResolutionContext(context);
            foreach(var item in source)
            {
                list.Add(itemContext.Map(item, default(TDestinationItem)));
            }
            return new ReadOnlyCollection<TDestinationItem>(list);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(ReadOnlyCollectionMapper).GetAllMethods().First(_ => _.IsStatic);

        public object Map(ResolutionContext context)
        {
            return
                MapMethodInfo.MakeGenericMethod(context.SourceType, TypeHelper.GetElementType(context.SourceType), TypeHelper.GetElementType(context.DestinationType))
                    .Invoke(null, new[] { context.SourceValue, context });
        }

        public bool IsMatch(TypePair context)
        {
            if (!(context.SourceType.IsEnumerableType() && context.DestinationType.IsGenericType()))
                return false;

            var genericType = context.DestinationType.GetGenericTypeDefinition();

            return genericType == typeof (ReadOnlyCollection<>);
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, TypeHelper.GetElementType(sourceExpression.Type), TypeHelper.GetElementType(destExpression.Type)), sourceExpression, contextExpression);
        }
    }
}