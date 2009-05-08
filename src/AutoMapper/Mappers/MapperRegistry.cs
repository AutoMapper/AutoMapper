using System;

namespace AutoMapper.Mappers
{
    public static class MapperRegistry
    {
        public static Func<IObjectMapper[]> AllMappers = () => new IObjectMapper[]
        {
            new CustomTypeMapMapper(),
            new TypeMapMapper(),
            new NewOrDefaultMapper(),
            new StringMapper(),
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