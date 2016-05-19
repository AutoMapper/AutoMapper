using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Configuration;

    public class EnumerableToDictionaryMapper : IObjectMapExpression
    {
        public static TDestination Map<TSource, TSourceElement, TDestination, TDestinationKey, TDestinationValue>(TSource source, TDestination destination, ResolutionContext context)
           where TSource : IEnumerable<TSourceElement>
           where TDestination : class, IDictionary<TDestinationKey, TDestinationValue>
        {
            if (source == null && context.Mapper.ShouldMapSourceCollectionAsNull(context))
                return null;

            TDestination list = destination ?? (
                typeof(TDestination).IsInterface()
                    ? new Dictionary<TDestinationKey, TDestinationValue>() as TDestination
                    : (TDestination)(context.ConfigurationProvider.AllowNullDestinationValues
                ? ObjectCreator.CreateNonNullValue(typeof(TDestination))
                : ObjectCreator.CreateObject(typeof(TDestination))));

            list.Clear();

            var itemContext = new ResolutionContext(context);
            foreach(var item in (IEnumerable<TSourceElement>)source ?? Enumerable.Empty<TSourceElement>())
            {
                list.Add((KeyValuePair<TDestinationKey, TDestinationValue>)itemContext.Map(item, default(KeyValuePair<TDestinationKey, TDestinationValue>), typeof(TSourceElement), typeof(KeyValuePair<TDestinationKey, TDestinationValue>)));
            }
            return list;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(EnumerableToDictionaryMapper).GetAllMethods().First(_ => _.IsStatic);

        public bool IsMatch(TypePair context)
        {
            return (context.DestinationType.IsDictionaryType())
                   && (context.SourceType.IsEnumerableType())
                   && (!context.SourceType.IsDictionaryType());
        }

        public object Map(ResolutionContext context)
        {
            Type sourceElementType = TypeHelper.GetElementType(context.SourceType);
            Type genericDestDictType = context.DestinationType.GetDictionaryType();
            Type destKeyType = genericDestDictType.GetTypeInfo().GenericTypeArguments[0];
            Type destValueType = genericDestDictType.GetTypeInfo().GenericTypeArguments[1];

            return
                MapMethodInfo.MakeGenericMethod(context.SourceType, sourceElementType, context.DestinationType, destKeyType, destValueType)
                    .Invoke(null, new[] { context.SourceValue, context.DestinationValue, context });
        }

        public Expression MapExpression(Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            Type sourceElementType = TypeHelper.GetElementType(sourceExpression.Type);
            Type genericDestDictType = destExpression.Type;
            Type destKeyType = genericDestDictType.GetTypeInfo().GenericTypeArguments[0];
            Type destValueType = genericDestDictType.GetTypeInfo().GenericTypeArguments[1];

            return Expression.Call(null,
                MapMethodInfo.MakeGenericMethod(sourceExpression.Type, sourceElementType, genericDestDictType, destKeyType, destValueType), sourceExpression, destExpression, contextExpression);
        }
    }
}