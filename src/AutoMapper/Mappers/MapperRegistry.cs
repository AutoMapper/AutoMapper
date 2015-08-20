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
#if NET4 || MONODROID || MONOTOUCH || __IOS__
            new NameValueCollectionMapper(),
#endif
            new DictionaryMapper(),
            new ReadOnlyCollectionMapper(),
#if NET4 || NETFX_CORE || MONODROID || MONOTOUCH || __IOS__ || SILVERLIGHT || DNXCORE50
            new HashSetMapper(),
#endif
            new CollectionMapper(),
            new EnumerableMapper(),
#if MONODROID || MONOTOUCH || __IOS__ || NET4
            new ListSourceMapper(),
#endif
#if SILVERLIGHT || NETFX_CORE
            new StringMapper(),
#endif
            new AssignableMapper(),
#if NET4 || MONODROID || MONOTOUCH || __IOS__ || SILVERLIGHT
            new TypeConverterMapper(),
#endif
            new NullableSourceMapper(),
            //new NullableMapper(),
            new ImplicitConversionOperatorMapper(),
            new ExplicitConversionOperatorMapper(),
            new OpenGenericMapper()
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