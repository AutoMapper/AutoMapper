using System;
using System.Collections.Generic;

namespace AutoMapper.Mappers
{
    public class MapperRegistryOverride : IMapperRegistry
    {
        public static Func<IEnumerable<IObjectMapper>> AllMappers = () => new IObjectMapper[]
        {
#if !SILVERLIGHT && !NETFX_CORE && !MONODROID
            new DataReaderMapper(),
#endif
            new TypeMapMapper(TypeMapObjectMapperRegistry.AllMappers()),
            new StringMapper(),
            new FlagsEnumMapper(),
            new EnumMapper(),
            new ArrayMapper(),
			new EnumerableToDictionaryMapper(),
#if !SILVERLIGHT && !NETFX_CORE
            new NameValueCollectionMapper(), 
#endif
            new DictionaryMapper(),
#if !SILVERLIGHT && !NETFX_CORE
            new ListSourceMapper(),
#endif
            new ReadOnlyCollectionMapper(),
#if !WINDOWS_PHONE
            new HashSetMapper(), 
#endif
            new CollectionMapper(),
            new EnumerableMapper(),
            new AssignableMapper(),
#if !NETFX_CORE
            new TypeConverterMapper(),
#endif
            new NullableSourceMapper(),
            new NullableMapper(),
            new ImplicitConversionOperatorMapper(),
            new ExplicitConversionOperatorMapper(),
        };

        public IEnumerable<IObjectMapper> GetMappers()
        {
            return AllMappers();
        }
    }
}