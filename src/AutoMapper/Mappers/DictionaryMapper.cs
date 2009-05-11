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
			IEnumerable<object> keyValuePairs = ((IEnumerable) context.SourceValue).Cast<object>();

			Type genericSourceDictType = context.SourceType.GetInterface("IDictionary`2");
			Type sourceKeyType = genericSourceDictType.GetGenericArguments()[0];
			Type sourceValueType = genericSourceDictType.GetGenericArguments()[1];
			Type sourceKvpType = KvpType.MakeGenericType(sourceKeyType, sourceValueType);
			Type genericDestDictType = context.DestinationType.GetDictionaryType();
			Type destKeyType = genericDestDictType.GetGenericArguments()[0];
			Type destValueType = genericDestDictType.GetGenericArguments()[1];

			Type dictionaryTypeToCreate = GetDestinationTypeToCreate(context, destKeyType, destValueType);

			object destDictionary = mapper.CreateObject(dictionaryTypeToCreate);
			int count = 0;

			foreach (object keyValuePair in keyValuePairs)
			{
				object sourceKey = sourceKvpType.GetProperty("Key").GetValue(keyValuePair, new object[0]);
				object sourceValue = sourceKvpType.GetProperty("Value").GetValue(keyValuePair, new object[0]);

				TypeMap keyTypeMap = mapper.ConfigurationProvider.FindTypeMapFor(sourceKeyType, destKeyType);
				TypeMap valueTypeMap = mapper.ConfigurationProvider.FindTypeMapFor(sourceValueType, destValueType);

				ResolutionContext keyContext = context.CreateElementContext(keyTypeMap, sourceKey, sourceKeyType, destKeyType, count);
				ResolutionContext valueContext = context.CreateElementContext(valueTypeMap, sourceValue, sourceValueType, destValueType, count);

				object destKey = mapper.Map(keyContext);
				object destValue = mapper.Map(valueContext);

				genericDestDictType.GetMethod("Add").Invoke(destDictionary, new[] {destKey, destValue});
			}

			return destDictionary;
		}

		private Type GetDestinationTypeToCreate(ResolutionContext context, Type destKeyType, Type destValueType)
		{
			return context.DestinationType.IsInterface
			       	? typeof(Dictionary<,>).MakeGenericType(destKeyType, destValueType)
			       	: context.DestinationType;
		}
	}
}