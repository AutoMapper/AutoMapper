using System.Collections.Generic;

namespace AutoMapper.Mappers
{
    internal static class MapperRegistry
    {
        public static IList<IObjectMapper> Mappers() => new List<IObjectMapper>
        {
            new NullableSourceMapper(),
            new NullableDestinationMapper(),
            new ExpressionMapper(), 
            new FlagsEnumMapper(),
            new StringToEnumMapper(), 
            new EnumToStringMapper(),
            new EnumToEnumMapper(), 
            new EnumToUnderlyingTypeMapper(),
            new UnderlyingTypeToEnumMapper(),
            new MultidimensionalArrayMapper(),
            new ArrayMapper(),
            new EnumerableToDictionaryMapper(),
#if NETSTANDARD1_3 || NET45 || NET40
            new NameValueCollectionMapper(),
#endif
            new DictionaryMapper(),
            new ReadOnlyCollectionMapper(),
            new HashSetMapper(),
            new CollectionMapper(),
            new EnumerableMapper(),
            new AssignableMapper(),
            new ConvertMapper(),
            new StringMapper(),
#if NETSTANDARD1_3 || NET45 || NET40
            new TypeConverterMapper(),
#endif
            new ImplicitConversionOperatorMapper(),
            new ExplicitConversionOperatorMapper(),
            new FromStringDictionaryMapper(),
            new ToStringDictionaryMapper(),
            new FromDynamicMapper(),
            new ToDynamicMapper()
        };
    }
}