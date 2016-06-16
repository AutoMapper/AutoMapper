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
                list.Add(itemContext.Map(keyPair.Key, default(TDestinationKey)),
                        itemContext.Map(keyPair.Value, default(TDestinationValue)));
            }
            return list;
        }

        public bool IsMatch(TypePair context)
        {
            return (context.SourceType.IsDictionaryType() && context.DestinationType.IsDictionaryType());
        }

        public object Map(ResolutionContext context)
            => context.MapCollection(CollectionMapperExtensions.IfNotNull(Expression.Constant(context.DestinationValue)), typeof(Dictionary<,>), CollectionMapperExtensions.MapKeyValuePairMethodInfo);

        public Expression MapExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configurationProvider, PropertyMap propertyMap, Expression sourceExpression, Expression destExpression, Expression contextExpression)
            => typeMapRegistry.MapCollectionExpression(configurationProvider, propertyMap, sourceExpression, destExpression, contextExpression, CollectionMapperExtensions.IfNotNull, typeof(Dictionary<,>), CollectionMapperExtensions.MapKeyPairValueExpr);
    }
}