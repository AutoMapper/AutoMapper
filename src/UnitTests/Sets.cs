namespace AutoMapper.UnitTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Should;
    using Xunit;

    public class MappingSets : AutoMapperSpecBase
    {
        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>());

        [Fact]
        public void Should_map_set_of_items()
        {
            var source = new HashSet<Source> {new Source {Value = 5}};

            var dest = Mapper.Map<ISet<Source>, ISet<Dest>>(source);

            dest.Count.ShouldEqual(1);
            dest.First().Value.ShouldEqual(5);
        }
    }
}