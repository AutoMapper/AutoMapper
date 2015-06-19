namespace AutoMapper.Mappers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Internal;

    // So IEnumerable<T> inherits IEnumerable
    // but IDictionary<TKey, TValue> DOES NOT inherit IDictionary
    // Fiddlesticks.
    // In fact the closest neighbor in the tree is IEnumerable I think ... :) probably of object,object in the "general" case
    /// <summary>
    /// 
    /// </summary>
    public class DictionaryMapper : IObjectMapper
    {
        /// <summary>
        /// 
        /// </summary>
        private Type KvpType { get; } = typeof (KeyValuePair<,>);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsMatch(ResolutionContext context)
        {
            return (context.SourceType.IsDictionaryType() && context.DestinationType.IsDictionaryType());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Map(ResolutionContext context)
        {
            var runner = context.MapperContext.Runner;
            if (context.IsSourceValueNull && runner.ShouldMapSourceCollectionAsNull(context))
                return null;

            var sourceEnumerableValue = (IEnumerable) context.SourceValue ?? new object[0];
            var keyValuePairs = sourceEnumerableValue.Cast<object>();

            var genericSourceDictType = context.SourceType.GetDictionaryType();
            var sourceKeyType = genericSourceDictType.GetGenericArguments()[0];
            var sourceValueType = genericSourceDictType.GetGenericArguments()[1];
            var sourceKvpType = KvpType.MakeGenericType(sourceKeyType, sourceValueType);
            var genericDestDictType = context.DestinationType.GetDictionaryType();
            var destKeyType = genericDestDictType.GetGenericArguments()[0];
            var destValueType = genericDestDictType.GetGenericArguments()[1];

            var dictionaryEntries = keyValuePairs.OfType<DictionaryEntry>();
            if (dictionaryEntries.Any())
                keyValuePairs = dictionaryEntries.Select(e => Activator.CreateInstance(sourceKvpType, e.Key, e.Value));

            var destDictionary = ObjectCreator.CreateDictionary(context.DestinationType, destKeyType, destValueType);
            var count = 0;

            foreach (var keyValuePair in keyValuePairs)
            {
                var sourceKey = sourceKvpType.GetProperty("Key").GetValue(keyValuePair, new object[0]);
                var sourceValue = sourceKvpType.GetProperty("Value").GetValue(keyValuePair, new object[0]);

                var keyTypeMap = context.MapperContext.ConfigurationProvider.ResolveTypeMap(
                    sourceKey, null, sourceKeyType, destKeyType);
                var valueTypeMap = context.MapperContext.ConfigurationProvider.ResolveTypeMap(
                    sourceValue, null, sourceValueType, destValueType);

                var keyContext = context.CreateElementContext(keyTypeMap, sourceKey, sourceKeyType,
                    destKeyType, count);
                var valueContext = context.CreateElementContext(valueTypeMap, sourceValue, sourceValueType,
                    destValueType, count);

                //TODO: may need "mapper" (or "runner") Map after all... but let's see if we can route that through MapperContext, or in this case through ResolutionContext (?) that's a lot of contexts which feels like a smell to me, but it's kind of like a request/response pattern ...
                var destKey = runner.Map(keyContext);
                var destValue = runner.Map(valueContext);

                genericDestDictType.GetMethod("Add").Invoke(destDictionary, new[] {destKey, destValue});

                count++;
            }

            return destDictionary;
        }
    }
}