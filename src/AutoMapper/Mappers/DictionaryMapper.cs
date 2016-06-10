using System.Linq.Expressions;

namespace AutoMapper.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Configuration;
    
    public class DictionaryMapper : IObjectMapExpression
    {
        public static TDestination Map<TSource, TSourceKey, TSourceValue, TDestination, TDestinationKey, TDestinationValue>(TSource source, TDestination destination, ResolutionContext context)
            where TSource : IDictionary<TSourceKey, TSourceValue>
            where TDestination : class, IDictionary<TDestinationKey, TDestinationValue>
        {
            if (source == null && context.Mapper.ShouldMapSourceCollectionAsNull(context))
                return null;

            TDestination list = destination ?? (
                typeof (TDestination).IsInterface()
                    ? new Dictionary<TDestinationKey, TDestinationValue>() as TDestination
                    : (TDestination) (context.ConfigurationProvider.AllowNullDestinationValues
                        ? ObjectCreator.CreateNonNullValue(typeof (TDestination))
                        : ObjectCreator.CreateObject(typeof (TDestination))));

            list.Clear();
            var itemContext = new ResolutionContext(context);
            foreach(var keyPair in (IEnumerable<KeyValuePair<TSourceKey, TSourceValue>>)source ?? Enumerable.Empty<KeyValuePair<TSourceKey, TSourceValue>>())
            {
                list.Add((TDestinationKey)itemContext.Map(keyPair.Key, default(TDestinationKey), typeof(TSourceKey), typeof(TDestinationKey)),
                        (TDestinationValue)itemContext.Map(keyPair.Value, default(TDestinationValue), typeof(TSourceValue), typeof(TDestinationValue)));
            }
            return list;
        }

        private static readonly MethodInfo MapMethodInfo = typeof(DictionaryMapper).GetAllMethods().First(_ => _.IsStatic);

        public bool IsMatch(TypePair context)
        {
            return (context.SourceType.IsDictionaryType() && context.DestinationType.IsDictionaryType());
        }

        public object Map(ResolutionContext context)
        {
            Type genericSourceDictType = context.SourceType.GetDictionaryType();
            Type sourceKeyType = genericSourceDictType.GetTypeInfo().GenericTypeArguments[0];
            Type sourceValueType = genericSourceDictType.GetTypeInfo().GenericTypeArguments[1];
            Type genericDestDictType = context.DestinationType.GetDictionaryType();
            Type destKeyType = genericDestDictType.GetTypeInfo().GenericTypeArguments[0];
            Type destValueType = genericDestDictType.GetTypeInfo().GenericTypeArguments[1];

            return
                MapMethodInfo.MakeGenericMethod(context.SourceType, sourceKeyType, sourceValueType, context.DestinationType, destKeyType, destValueType)
                    .Invoke(null, new[] { context.SourceValue, context.DestinationValue, context });
        }

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
        {
            Type genericSourceDictType = sourceExpression.Type.GetDictionaryType();
            Type sourceKeyType = genericSourceDictType.GetTypeInfo().GenericTypeArguments[0];
            Type sourceValueType = genericSourceDictType.GetTypeInfo().GenericTypeArguments[1];
            Type genericDestDictType = destExpression.Type.GetDictionaryType();
            Type destKeyType = genericDestDictType.GetTypeInfo().GenericTypeArguments[0];
            Type destValueType = genericDestDictType.GetTypeInfo().GenericTypeArguments[1];

            return Expression.Call(null,
                MapMethodInfo.MakeGenericMethod(sourceExpression.Type, sourceKeyType, sourceValueType, destExpression.Type, destKeyType, destValueType),
                    sourceExpression, destExpression, contextExpression);
        }
    }
}