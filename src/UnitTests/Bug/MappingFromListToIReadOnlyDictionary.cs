using System.Collections.Generic;

namespace AutoMapper.UnitTests.Bug
{
    public class MappingFromListToIReadOnlyDictionary : AutoMapperSpecBase
    {
        private class Source
        {
            public List<int> Field { get; set; }
        }

        private class Destination
        {
            public IReadOnlyDictionary<int,int> Field { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
        });
    }
}
