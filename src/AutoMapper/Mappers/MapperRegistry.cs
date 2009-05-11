using System;
using System.Collections.Generic;

namespace AutoMapper.Mappers
{
    public static class MapperRegistry
    {
        public static Func<IEnumerable<IObjectMapper>> AllMappers = () => new IObjectMapper[]
        {
            new CustomTypeMapMapper(),
            new TypeMapMapper(),
            new NewOrDefaultMapper(),
            new StringMapper(),
            new FlagsEnumMapper(),
            new EnumMapper(),
            new AssignableMapper(),
            new ArrayMapper(),
            new DictionaryMapper(),
            new EnumerableMapper(),
            new TypeConverterMapper(),
            new NullableMapper(),
        };
    }
}