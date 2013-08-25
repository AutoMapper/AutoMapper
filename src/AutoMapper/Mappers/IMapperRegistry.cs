using System.Collections.Generic;

namespace AutoMapper.Mappers
{
    public interface IMapperRegistry
    {
        IEnumerable<IObjectMapper> GetMappers();
    }
}
