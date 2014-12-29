using System.Collections.Generic;
using AutoMapper.EquivilencyExpression;

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
            new PrimitiveArrayMapper(),
            new ArrayMapper(),
            new EnumerableToDictionaryMapper(),
            new DictionaryMapper(),
            new ReadOnlyCollectionMapper(),
            new EquivlentExpressionAddRemoveCollectionMapper(),
            new ObjectToEquivalencyExpressionByEquivalencyExistingMapper(),
            new CollectionMapper(),
            new EnumerableMapper(),
            new AssignableMapper(),
            new NullableSourceMapper(),
            new NullableMapper(),
            new ImplicitConversionOperatorMapper(),
            new ExplicitConversionOperatorMapper()
        };

        private static readonly List<IObjectMapper> _mappers = new List<IObjectMapper>(_initialMappers);

        /// <summary>
        /// Extension point for modifying list of object mappers
        /// </summary>
        public static IList<IObjectMapper> Mappers
        {
            get { return _mappers; }
        }

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