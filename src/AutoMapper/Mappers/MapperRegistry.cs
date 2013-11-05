using System;
using System.Collections.Generic;

namespace AutoMapper.Mappers
{
    public class MapperRegistry : IMapperRegistry
    {
        /// <summary>
        /// Extension point for modifying list of object mappers
        /// </summary>
        public static Func<IEnumerable<IObjectMapper>> AllMappers = () => new IObjectMapper[]
        {
            new TypeMapMapper(TypeMapObjectMapperRegistry.AllMappers()),
            new StringMapper(),
            new FlagsEnumMapper(),
            new AssignableMapper(),
            new EnumMapper(),
            new PrimitiveArrayMapper(),
            new ArrayMapper(),
			new EnumerableToDictionaryMapper(),
            new DictionaryMapper(),
            new ReadOnlyCollectionMapper(),
            new CollectionMapper(),
            new EnumerableMapper(),
            new NullableSourceMapper(),
            new NullableMapper(),
            new ImplicitConversionOperatorMapper(),
            new ExplicitConversionOperatorMapper()
        };

        public IEnumerable<IObjectMapper> GetMappers()
        {
            return AllMappers();
        }
    }
}