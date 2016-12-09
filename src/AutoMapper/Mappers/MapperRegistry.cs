using System.Collections.Generic;

namespace AutoMapper.Mappers
{
    public static class MapperRegistry
    {
        public static IObjectMapper[] Mappers() => new IObjectMapper[]
        {
            new NullableSourceMapper(),
            new ExpressionMapper(), 
            new FlagsEnumMapper(),
            new StringToEnumMapper(), 
            new EnumToEnumMapper(), 
            new EnumToUnderlyingTypeMapper(),
            new MultidimensionalArrayMapper(),
            new ArrayMapper(),
            new EnumerableToDictionaryMapper(),
#if NETSTANDARD1_3 || NET45
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