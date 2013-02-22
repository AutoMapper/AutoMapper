namespace AutoMapper.Mappers
{
    public static class MapperRegistryInitializer
    {
        static MapperRegistryInitializer()
        {
            MapperRegistry.AllMappers = () => new IObjectMapper[]
            {
#if !SILVERLIGHT
                new DataReaderMapper(),
#endif
                new TypeMapMapper(TypeMapObjectMapperRegistry.AllMappers()),
                new StringMapper(),
                new FlagsEnumMapper(),
                new EnumMapper(),
                new ArrayMapper(),
                new EnumerableToDictionaryMapper(),
#if !SILVERLIGHT
                new NameValueCollectionMapper(),
#endif
                new DictionaryMapper(),
#if !SILVERLIGHT
                new ListSourceMapper(),
#endif
                new ReadOnlyCollectionMapper(),
#if !SILVERLIGHT
                new HashSetMapper(),
#endif
                new CollectionMapper(),
                new EnumerableMapper(),
                new AssignableMapper(),
                new TypeConverterMapper(),
                new NullableSourceMapper(),
                new NullableMapper(),
                new ImplicitConversionOperatorMapper(),
                new ExplicitConversionOperatorMapper(),
            };
        }
    }
}
