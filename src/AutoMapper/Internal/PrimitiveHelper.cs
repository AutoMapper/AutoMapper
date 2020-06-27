using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AutoMapper.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PrimitiveHelper
    {
        public static void ForAll<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var feature in enumerable)
            {
                action(feature);
            }
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            dictionary.TryGetValue(key, out TValue value);
            return value;
        }

        public static bool IsEnumToEnum(this TypePair context) => context.SourceType.IsEnum && context.DestinationType.IsEnum;

        public static bool IsUnderlyingTypeToEnum(this TypePair context) =>
            context.DestinationType.IsEnum && context.SourceType.IsAssignableFrom(Enum.GetUnderlyingType(context.DestinationType));

        public static bool IsEnumToUnderlyingType(this TypePair context) =>
            context.SourceType.IsEnum && context.DestinationType.IsAssignableFrom(Enum.GetUnderlyingType(context.SourceType));
    }
}