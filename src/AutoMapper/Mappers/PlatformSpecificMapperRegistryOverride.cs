namespace AutoMapper.Mappers
{
    using System.Linq;

    public class PlatformSpecificMapperRegistryOverride : IPlatformSpecificMapperRegistry
    {
        private object mapperLock = new object();

        public void Initialize()
        {
#if NET4 || MONODROID || MONOTOUCH || __IOS__ || ASPNET50 || ASPNETCORE50
            InsertBefore<DictionaryMapper>(new NameValueCollectionMapper());
#endif
#if MONODROID || MONOTOUCH || __IOS__ || NET4
            InsertBefore<AssignableMapper>(new ListSourceMapper());
#endif
#if NET4 || NETFX_CORE || MONODROID || MONOTOUCH || __IOS__ || SILVERLIGHT || ASPNET50 || ASPNETCORE50
            InsertBefore<CollectionMapper>(new HashSetMapper());
#endif
#if NET4 || MONODROID || MONOTOUCH || __IOS__ || SILVERLIGHT || ASPNET50 || ASPNETCORE50
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