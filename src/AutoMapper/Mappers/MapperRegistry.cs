using System.Collections.Generic;
namespace AutoMapper.Internal.Mappers
{
    internal static class MapperRegistry
    {
        public static List<IObjectMapper> Mappers() => new List<IObjectMapper>
        {
            new AssignableMapper(),// except collections, which are copied; most likely match
            new ArrayMapper(),// set with indices or Array.Copy
            new NameValueCollectionMapper(),// special case; legacy
            new CollectionMapper(),// matches IEnumerable, requires ICollection<>, IList or read-only collections
            new NullableSourceMapper(),// map from the underlying type
            new ToStringMapper(),// object.ToString, no boxing, special case enums
            new NullableDestinationMapper(),// map to the underlying type
            new ConvertMapper(),// the Convert class, mostly primitives
            new StringToEnumMapper(),// special case enums
            new EnumToEnumMapper(),// map by string value or by numeric value
            new ParseStringMapper(),// Parse(string), no boxing, Guid, TimeSpan, DateTimeOffset
            new UnderlyingTypeEnumMapper(),// enum numeric value
            new KeyValueMapper(),// KeyValuePair, for dictionaries
            new ConstructorMapper(),// new Destination(source)
            new ConversionOperatorMapper("op_Implicit"),// implicit operator Destination or implicit operator Source
            new ConversionOperatorMapper("op_Explicit"),// explicit operator Destination or explicit operator Source
            new FromStringDictionaryMapper(),// property values to typed object
            new ToStringDictionaryMapper(),// typed object to property values
            new FromDynamicMapper(),// dynamic to typed object
            new ToDynamicMapper(),// typed object to dynamic
            new TypeConverterMapper(),// TypeConverter, the most expensive
        };
    }
}