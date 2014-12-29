namespace AutoMapper.Mappers
{
    using System.Linq;

    public class PlatformSpecificMapperRegistryOverride : IPlatformSpecificMapperRegistry
    {
        private object mapperLock = new object();

        public void Initialize()
        {
#if !SILVERLIGHT && !NETFX_CORE && !MONODROID && !MONOTOUCH
            InsertBefore<TypeMapMapper>(new DataReaderMapper());
#endif
#if !SILVERLIGHT && !NETFX_CORE
            InsertBefore<DictionaryMapper>(new NameValueCollectionMapper());
#endif
#if !SILVERLIGHT && !NETFX_CORE
            InsertBefore<AssignableMapper>(new ListSourceMapper());
#endif
#if !WINDOWS_PHONE
            InsertBefore<CollectionMapper>(new HashSetMapper());
#endif
#if !NETFX_CORE
            InsertBefore<NullableSourceMapper>(new TypeConverterMapper());
#endif
#if SILVERLIGHT || NETFX_CORE
            InsertBefore<AssignableMapper>(new StringMapper());
#endif
        }

        private void InsertBefore<TObjectMapper>(IObjectMapper mapper)
            where TObjectMapper : IObjectMapper
        {
            lock (mapperLock)
            {
                var targetMapper = MapperRegistry.Mappers.FirstOrDefault(om => om is TObjectMapper);
                var index = targetMapper == null ? 0 : MapperRegistry.Mappers.IndexOf(targetMapper);
                MapperRegistry.Mappers.Insert(index, mapper);
            }
        }
    }
}