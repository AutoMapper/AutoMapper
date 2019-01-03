using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests
{
    namespace AttributeBasedMaps
    {
        public class When_specifying_map_with_attribute : AutoMapperSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            [AutoMap(typeof(Source))]
            public class Dest
            {
                public int Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMissingTypeMaps = false;
                cfg.AddMaps(typeof(When_specifying_map_with_attribute));
            });

            [Fact]
            public void Should_map()
            {
                var source = new Source {Value = 5};
                var dest = Mapper.Map<Dest>(source);

                dest.Value.ShouldBe(5);
            }
        }
    }
}