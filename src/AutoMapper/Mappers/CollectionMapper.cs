using System.Collections;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using System.Collections.Generic;
    using System.Reflection;
    using Configuration;

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

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, 
                MapMethodInfo.MakeGenericMethod(sourceExpression.Type, TypeHelper.GetElementType(sourceExpression.Type), destExpression.Type, TypeHelper.GetElementType(destExpression.Type)),
                    sourceExpression, destExpression, contextExpression);
        }
    }
}