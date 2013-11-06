using System;
using System.Collections.Generic;

namespace AutoMapper.Mappers
{
    public static class MapperRegistry
    {
        private static readonly IList<IObjectMapper> _mappers = new List<IObjectMapper>
        {
            new TypeMapMapper(TypeMapObjectMapperRegistry.Mappers),
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

        /// <summary>
        /// Extension point for modifying list of object mappers
        /// </summary>
        public static IList<IObjectMapper> Mappers
        {
            get { return _mappers; }
        }
    }
}