namespace AutoMapper.Mappers
{
    using Internal;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    // So IEnumerable<T> inherits IEnumerable
    // but IDictionary<TKey, TValue> DOES NOT inherit IDictionary
    // Fiddlesticks.
    public class DictionaryMapper : IObjectMapper
    {
        private static readonly Type KvpType = typeof (KeyValuePair<,>);

        public bool IsMatch(TypePair context)
        {
            return (context.SourceType.IsDictionaryType() && context.DestinationType.IsDictionaryType());
        }

        public object Map(ResolutionContext context)
        {
            if(context.IsSourceValueNull && context.Engine.ShouldMapSourceCollectionAsNull(context))
            {
                return null;
            }
            Type genericSourceDictType = context.SourceType.GetDictionaryType();
            Type sourceKeyType = genericSourceDictType.GetTypeInfo().GenericTypeArguments[0];
            Type sourceValueType = genericSourceDictType.GetTypeInfo().GenericTypeArguments[1];
            Type sourceKvpType = KvpType.MakeGenericType(sourceKeyType, sourceValueType);
            Type genericDestDictType = context.DestinationType.GetDictionaryType();
            Type destKeyType = genericDestDictType.GetTypeInfo().GenericTypeArguments[0];
            Type destValueType = genericDestDictType.GetTypeInfo().GenericTypeArguments[1];

            var kvpEnumerator = GetKeyValuePairEnumerator(context, sourceKvpType);
            var destDictionary = ObjectCreator.CreateDictionary(context.DestinationType, destKeyType, destValueType);
            int count = 0;
            while(kvpEnumerator.MoveNext())
            {
                var keyValuePair = kvpEnumerator.Current;
                object sourceKey = sourceKvpType.GetProperty("Key").GetValue(keyValuePair, new object[0]);
                object sourceValue = sourceKvpType.GetProperty("Value").GetValue(keyValuePair, new object[0]);

                TypeMap keyTypeMap = context.ConfigurationProvider.ResolveTypeMap(sourceKey, null, sourceKeyType,
                    destKeyType);
                TypeMap valueTypeMap = context.ConfigurationProvider.ResolveTypeMap(sourceValue, null, sourceValueType,
                    destValueType);

                ResolutionContext keyContext = context.CreateElementContext(keyTypeMap, sourceKey, sourceKeyType,
                    destKeyType, count);
                ResolutionContext valueContext = context.CreateElementContext(valueTypeMap, sourceValue, sourceValueType,
                    destValueType, count);

                object destKey = context.Engine.Map(keyContext);
                object destValue = context.Engine.Map(valueContext);

                genericDestDictType.GetMethod("Add").Invoke(destDictionary, new[] { destKey, destValue });

                count++;
            }

            return destDictionary;
        }

        private static IEnumerator GetKeyValuePairEnumerator(ResolutionContext context, Type sourceKvpType)
        {
            if(context.SourceValue == null)
            {
                return Enumerable.Empty<object>().GetEnumerator();
            }
            var sourceEnumerableValue = (IEnumerable) context.SourceValue;
            var dictionaryEntries = sourceEnumerableValue.Cast<object>().OfType<DictionaryEntry>().Select(e => Activator.CreateInstance(sourceKvpType, e.Key, e.Value));
            if(dictionaryEntries.Any())
            {
                return dictionaryEntries.GetEnumerator();
            }
            var enumerableKvpType = typeof(IEnumerable<>).MakeGenericType(sourceKvpType);
            if(enumerableKvpType.IsAssignableFrom(sourceEnumerableValue.GetType()))
            {
                return (IEnumerator)enumerableKvpType.GetMethod("GetEnumerator").Invoke(sourceEnumerableValue, null);
            }
            throw new AutoMapperMappingException(context, "Cannot map dictionary type " + context.SourceType);
        }
    }
}