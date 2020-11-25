using System.Collections.Generic;

namespace AutoMapper.Mappers
{
    internal static class MapperRegistry
    {
        /* Mapping order:
         - Nullables
         - Collections
         - Assignable
         - Primitives
         - Converters
         - Conversion operators
         - "Special" cases
         */
        public static IList<IObjectMapper> Mappers() => new List<IObjectMapper>
        {
            new NullableSourceMapper(),
            new NullableDestinationMapper(),
            new MultidimensionalArrayMapper(),
            new ArrayCopyMapper(),
            new ArrayMapper(),
            new ReadOnlyDictionaryMapper(),
            new DictionaryMapper(),
            new EnumerableToReadOnlyDictionaryMapper(),
            new EnumerableToDictionaryMapper(),
            new ReadOnlyCollectionMapper(),
            new HashSetMapper(),
            new CollectionMapper(),
            new EnumerableMapper(),
            new AssignableMapper(),
            new FlagsEnumMapper(),
            new StringToEnumMapper(),
            new EnumToStringMapper(),
            new EnumToEnumMapper(),
            new ConvertMapper(),
            new StringMapper(),
            new ConversionOperatorMapper("op_Implicit"),
            new ConversionOperatorMapper("op_Explicit"),
            new FromStringDictionaryMapper(),
            new ToStringDictionaryMapper(),
            new FromDynamicMapper(),
            new ToDynamicMapper(),
            new TypeConverterMapper(),// the most expensive
        };
    }
}