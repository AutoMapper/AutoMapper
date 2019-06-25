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
            new EnumerableToDictionaryMapper(),
            new NameValueCollectionMapper(),
            new ReadOnlyDictionaryMapper(),
            new DictionaryMapper(),
            new ReadOnlyCollectionMapper(),
            new HashSetMapper(),
            new CollectionMapper(),
            new EnumerableMapper(),
            new AssignableMapper(),
            new FlagsEnumMapper(),
            new StringToEnumMapper(),
            new EnumToStringMapper(),
            new EnumToEnumMapper(),
            new EnumToUnderlyingTypeMapper(),
            new UnderlyingTypeToEnumMapper(),
            new ConvertMapper(),
            new StringMapper(),
            new TypeConverterMapper(),
            new ImplicitConversionOperatorMapper(),
            new ExplicitConversionOperatorMapper(),
            new FromStringDictionaryMapper(),
            new ToStringDictionaryMapper(),
            new FromDynamicMapper(),
            new ToDynamicMapper()
        };
    }
}