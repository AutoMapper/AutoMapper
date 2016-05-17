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
            if (source == null && context.Mapper.ShouldMapSourceCollectionAsNull(context))
                return null;

            var sourceElementType = TypeHelper.GetElementType(typeof(TSource), source);
            var destElementType = typeof (TDestinationElement);
            source = source ?? (context.ConfigurationProvider.AllowNullDestinationValues
                ? ObjectCreator.CreateNonNullValue(typeof(TSource))
                : ObjectCreator.CreateObject(typeof(TSource))) as TSource;

            TDestination destEnumeration = (destination is IList && !(destination is Array))
                ? destination
                : (TDestination) ObjectCreator.CreateList(destElementType);

            var list = destEnumeration as IList<TDestinationElement>;
            list.Clear();
            
            foreach (var item in source)
                list.Add((TDestinationElement)context.Mapper.Map(item, null, sourceElementType, destElementType, context));

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

        public Expression MapExpression(Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type, TypeHelper.GetElementType(destExpression.Type)), sourceExpression, destExpression, contextExpression);
        }
    }
}