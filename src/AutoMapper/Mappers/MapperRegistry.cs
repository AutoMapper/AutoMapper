using System.Collections.Generic;

namespace AutoMapper.Mappers
{
    public static class MapperRegistry
    {
        private static readonly IObjectMapper[] _initialMappers =
        {
            new ExpressionMapper(), 
            new TypeMapMapper(TypeMapObjectMapperRegistry.Mappers),
            new AssignableArrayMapper(), 
            new FlagsEnumMapper(),
            new EnumMapper(),
            new MultidimensionalArrayMapper(),
            new PrimitiveArrayMapper(),
            new ArrayMapper(),
            new EnumerableToDictionaryMapper(),
            new NameValueCollectionMapper(),
            new DictionaryMapper(),
            new ReadOnlyCollectionMapper(),
            new HashSetMapper(),
            new CollectionMapper(),
            new EnumerableMapper(),
            new StringMapper(),
            new AssignableMapper(),
            new TypeConverterMapper(),
            new NullableSourceMapper(),
            new ImplicitConversionOperatorMapper(),
            new ExplicitConversionOperatorMapper(),
            new OpenGenericMapper(),
            new FromStringDictionaryMapper(),
            new ToStringDictionaryMapper(),
            new FromDynamicMapper(),
            new ToDynamicMapper()
        };

        private static readonly List<IObjectMapper> _mappers = new List<IObjectMapper>(_initialMappers);

        /// <summary>
        /// Extension point for modifying list of object mappers
        /// </summary>
        public static IList<IObjectMapper> Mappers => _mappers;

        /// <summary>
        /// Reset mapper registry to built-in values
        /// </summary>
        public static void Reset()
        {
            _mappers.Clear();
            _mappers.AddRange(_initialMappers);
        }
    }
}