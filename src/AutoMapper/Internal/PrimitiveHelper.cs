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
            if (otherCollection == null || otherCollection.Count == 0)
            {
                return collection;
            }
            if (collection.Count == 0)
            {
                return otherCollection;
            }
            return Enumerable.Concat(collection, otherCollection);
        }
        public static void CheckIsDerivedFrom(this TypePair types, TypePair baseTypes)
        {
            types.SourceType.CheckIsDerivedFrom(baseTypes.SourceType);
            types.DestinationType.CheckIsDerivedFrom(baseTypes.DestinationType);
        }
        public static bool IsEnumToEnum(this TypePair context) => context.SourceType.IsEnum && context.DestinationType.IsEnum;
        public static bool IsUnderlyingTypeToEnum(this TypePair context) =>
            context.DestinationType.IsEnum && context.SourceType.IsAssignableFrom(Enum.GetUnderlyingType(context.DestinationType));
        public static bool IsEnumToUnderlyingType(this TypePair context) =>
            context.SourceType.IsEnum && context.DestinationType.IsAssignableFrom(Enum.GetUnderlyingType(context.SourceType));
    }
}