using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.Reflection;
    using Configuration;

    public class EnumerableMapper : IObjectMapExpression
    {
        public static TDestination Map<TSource, TDestination, TDestinationElement>(TSource source, TDestination destination, ResolutionContext context)
            where TSource : class, IEnumerable
            where TDestination : class, IEnumerable
        {
            if (source == null)
                return context.Mapper.ShouldMapSourceCollectionAsNull(context) ? null : new List<TDestinationElement>() as TDestination;

            var sourceElementType = TypeHelper.GetElementType(typeof(TSource), source);
            var destElementType = typeof (TDestinationElement);

            TDestination destEnumeration = (destination is IList && !(destination is Array))
                ? destination
                : (TDestination) ObjectCreator.CreateList(destElementType);

            var list = destEnumeration as IList<TDestinationElement>;
            list.Clear();
            var itemContext = new ResolutionContext(context);
            foreach(var item in source)
            {
                list.Add((TDestinationElement)itemContext.Map(item, null, sourceElementType, destElementType));
            }
            return destEnumeration;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(EnumerableMapper).GetAllMethods().First(_ => _.IsStatic);

        public object Map(ResolutionContext context)
        {
            return MapMethodInfo.MakeGenericMethod(context.SourceType, context.DestinationType, TypeHelper.GetElementType(context.DestinationType)).Invoke(null, new [] { context.SourceValue, context.DestinationValue, context });
        }

        public bool IsMatch(TypePair context)
        {
            // destination type must be IEnumerable interface or a class implementing at least IList 
            return ((context.DestinationType.IsInterface() && context.DestinationType.IsEnumerableType()) ||
                    context.DestinationType.IsListType())
                   && context.SourceType.IsEnumerableType();
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type, TypeHelper.GetElementType(destExpression.Type)), sourceExpression, destExpression, contextExpression);
        }
    }
}