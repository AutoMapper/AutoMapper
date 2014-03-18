using System.Collections.Generic;

namespace AutoMapper.Mappers
{
    using System.Linq;

    public class PlatformSpecificMapperRegistryOverride : IPlatformSpecificMapperRegistry
    {
        public void Initialize()
        {
#if !SILVERLIGHT && !NETFX_CORE && !MONODROID && !MONOTOUCH
            InsertBefore<TypeMapMapper>(new DataReaderMapper());
#endif
#if !SILVERLIGHT && !NETFX_CORE
            InsertBefore<DictionaryMapper>(new NameValueCollectionMapper());
#endif
#if !SILVERLIGHT && !NETFX_CORE
            InsertBefore<ReadOnlyCollectionMapper>(new ListSourceMapper());
#endif
#if !WINDOWS_PHONE
            InsertBefore<CollectionMapper>(new HashSetMapper());
#endif
#if !NETFX_CORE
            InsertBefore<NullableSourceMapper>(new TypeConverterMapper());
#endif
        }

        private void InsertBefore<TObjectMapper>(IObjectMapper mapper)
            where TObjectMapper : IObjectMapper
        {
            MapperRegistry.Mappers.SyncChange(maps =>
            {
                var targetMapper = maps.FirstOrDefault(om => om is TObjectMapper);
                var index = targetMapper == null ? 0 : maps.IndexOf(targetMapper);
                maps.Insert(index, mapper);
            });
        }
    }
}