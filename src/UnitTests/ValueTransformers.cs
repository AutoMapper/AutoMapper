using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests
{
    namespace ValueTransformers
    {
        public class BasicTransforming : AutoMapperSpecBase
        {
            public class Source
            {
                public string Value { get; set; }
            }

            public class Dest
            {
                public string Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Dest>();
                cfg.ApplyTransform<string>(dest => dest + " is straight up dope");
            });

            [Fact]
            public void Should_transform_value()
            {
                var source = new Source
                {
                    Value = "Jimmy"
                };
                var dest = Mapper.Map<Source, Dest>(source);

                dest.Value.ShouldBe("Jimmy is straight up dope");
            }
        }
    }
}