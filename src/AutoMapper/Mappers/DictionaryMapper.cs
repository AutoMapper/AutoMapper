using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper.Mappers
{
	// So IEnumerable<T> inherits IEnumerable
	// but IDictionary<TKey, TValue> DOES NOT inherit IDictionary
	// Fiddlesticks.
	public class DictionaryMapper : IObjectMapper
	{
		private static readonly Type KvpType = typeof(KeyValuePair<,>);

		public bool IsMatch(ResolutionContext context)
		{
			return (context.SourceType.IsDictionaryType() && context.DestinationType.IsDictionaryType());
		}

		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
            if (context.IsSourceValueNull && mapper.ShouldMapSourceCollectionAsNull(context))
                return null;

			var sourceEnumerableValue = (IEnumerable)context.SourceValue ?? new object[0];
			IEnumerable<object> keyValuePairs = sourceEnumerableValue.Cast<object>();

			Type genericSourceDictType = context.SourceType.GetDictionaryType();
			Type sourceKeyType = genericSourceDictType.GetGenericArguments()[0];
			Type sourceValueType = genericSourceDictType.GetGenericArguments()[1];
			Type sourceKvpType = KvpType.MakeGenericType(sourceKeyType, sourceValueType);
			Type genericDestDictType = context.DestinationType.GetDictionaryType();
			Type destKeyType = genericDestDictType.GetGenericArguments()[0];
			Type destValueType = genericDestDictType.GetGenericArguments()[1];

			var dictionaryEntries = keyValuePairs.OfType<DictionaryEntry>();
			if (dictionaryEntries.Any())
				keyValuePairs = dictionaryEntries.Select(e => Activator.CreateInstance(sourceKvpType, e.Key, e.Value));

			object destDictionary = ObjectCreator.CreateDictionary(context.DestinationType, destKeyType, destValueType);
			int count = 0;

			foreach (object keyValuePair in keyValuePairs)
			{
				object sourceKey = sourceKvpType.GetProperty("Key").GetValue(keyValuePair, new object[0]);
				object sourceValue = sourceKvpType.GetProperty("Value").GetValue(keyValuePair, new object[0]);

				TypeMap keyTypeMap = mapper.ConfigurationProvider.FindTypeMapFor(sourceKey, sourceKeyType, destKeyType);
				TypeMap valueTypeMap = mapper.ConfigurationProvider.FindTypeMapFor(sourceValue, sourceValueType, destValueType);

				ResolutionContext keyContext = context.CreateElementContext(keyTypeMap, sourceKey, sourceKeyType, destKeyType, count);
				ResolutionContext valueContext = context.CreateElementContext(valueTypeMap, sourceValue, sourceValueType, destValueType, count);

				object destKey = mapper.Map(keyContext);
				object destValue = mapper.Map(valueContext);

				genericDestDictType.GetMethod("Add").Invoke(destDictionary, new[] { destKey, destValue });

				count++;
			}

			return destDictionary;
		}
	}
}