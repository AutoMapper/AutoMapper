using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class ReadOnlyFieldMappingBug : AutoMapperSpecBase
    {
        public class Source
        {
            public string Value { get; set; }
        }

        public class Destination
        {
            public readonly string Value;

            public Destination(string value)
            {
                Value = value;
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            // BUG. ArgumentException : Expression must be writeable
            cfg.CreateMap<Source, Destination>();
        });

        [Fact]
        public void Should_map_over_constructor()
        {
            var source = new Source { Value = "value" };

            var dest = Mapper.Map<Destination>(source);

            dest.Value.ShouldBe(source.Value);
        }
    }
}