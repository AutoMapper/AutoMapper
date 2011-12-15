using System;
using System.Collections.Generic;

namespace AutoMapper.Mappers
{
    public static class MapperRegistry
    {
        public static Func<IEnumerable<IObjectMapper>> AllMappers = () => new IObjectMapper[]
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