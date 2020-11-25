using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AutoMapper.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PrimitiveHelper
    {
        public static IReadOnlyCollection<T> NullCheck<T>(this IReadOnlyCollection<T> source) => source ?? Array.Empty<T>();
        public static IEnumerable<T> Concat<T>(this IReadOnlyCollection<T> collection, IReadOnlyCollection<T> otherCollection)
        {
            otherCollection ??= Array.Empty<T>();
            if (collection.Count == 0)
            {
                return otherCollection;
            }
            return otherCollection.Count == 0 ? collection : Enumerable.Concat(collection, otherCollection);
        }
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            dictionary.TryGetValue(key, out TValue value);
            return value;
        }
        public static bool IsEnumToEnum(this in TypePair context) => context.SourceType.IsEnum && context.DestinationType.IsEnum;
        public static bool IsUnderlyingTypeToEnum(this in TypePair context) =>
            context.DestinationType.IsEnum && context.SourceType.IsAssignableFrom(Enum.GetUnderlyingType(context.DestinationType));
        public static bool IsEnumToUnderlyingType(this in TypePair context) =>
            context.SourceType.IsEnum && context.DestinationType.IsAssignableFrom(Enum.GetUnderlyingType(context.SourceType));
    }
}