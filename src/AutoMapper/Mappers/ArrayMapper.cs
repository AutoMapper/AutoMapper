using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using System;
    using System.Reflection;
    using Configuration;

    public class ArrayMapper : IObjectMapper, IObjectMapExpression
    {
        public static TDestination Map<TDestination,TSource, TSourceElement>(TSource source, ResolutionContext context)
            where TSource : IEnumerable
            where TDestination : class
        {
            if (source == null && context.Mapper.ShouldMapSourceCollectionAsNull(context))
                return null;
            
            var destElementType = TypeHelper.GetElementType(typeof (TDestination));

            if (!context.IsSourceValueNull && context.DestinationType.IsAssignableFrom(context.SourceType))
            {
                var elementTypeMap = context.ConfigurationProvider.ResolveTypeMap(typeof(TSourceElement), destElementType);
                if (elementTypeMap == null)
                    return source as TDestination;
            }

            IEnumerable sourceList = source;
            if (sourceList == null)
                sourceList = typeof(TSource).IsInterface ?
                new List<TSourceElement>() :
                (IEnumerable<TSourceElement>)(context.ConfigurationProvider.AllowNullDestinationValues
                ? ObjectCreator.CreateNonNullValue(typeof(TSource))
                : ObjectCreator.CreateObject(typeof(TSource)));

            var sourceLength = sourceList.OfType<object>().Count();
            Array array = ObjectCreator.CreateArray(destElementType, sourceLength);
            int count = 0;
            foreach (var item in sourceList)
                array.SetValue(context.Mapper.Map(item, null, typeof(TSourceElement), destElementType, context), count++);

            return array as TDestination;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(ArrayMapper).GetAllMethods().First(_ => _.IsStatic);

        public object Map(ResolutionContext context)
        {
            return MapMethodInfo.MakeGenericMethod(context.DestinationType, context.SourceType, TypeHelper.GetElementType(context.SourceType, (IEnumerable)context.SourceValue)).Invoke(null, new [] { context.SourceValue, context });
        }

        public bool IsMatch(TypePair context)
        {
            return (context.DestinationType.IsArray) && (context.SourceType.IsEnumerableType());
        }

        public Expression MapExpression(Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            return Expression.Call(null, MapMethodInfo.MakeGenericMethod(destExpression.Type, sourceExpression.Type, TypeHelper.GetElementType(sourceExpression.Type)), sourceExpression, contextExpression );
        }
    }
}