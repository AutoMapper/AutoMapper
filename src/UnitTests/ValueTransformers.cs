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

        public class StackingTransformers : AutoMapperSpecBase
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
                cfg.ApplyTransform<string>(dest => dest + "! No joke!");
            });

            [Fact]
            public void Should_stack_transformers_in_order()
            {
                var source = new Source
                {
                    Value = "Jimmy"
                };
                var dest = Mapper.Map<Source, Dest>(source);

                dest.Value.ShouldBe("Jimmy is straight up dope! No joke!");
            }
        }

        public class DifferentProfiles : AutoMapperSpecBase
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
                cfg.CreateProfile("Other", p => p.ApplyTransform<string>(dest => dest + "! No joke!"));
            });

            [Fact]
            public void Should_not_apply_other_transform()
            {
                var source = new Source
                {
                    Value = "Jimmy"
                };
                var dest = Mapper.Map<Source, Dest>(source);

                dest.Value.ShouldBe("Jimmy is straight up dope");
            }
        }

        public class StackingRootConfigAndProfileTransform : AutoMapperSpecBase
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
                cfg.ApplyTransform<string>(dest => dest + "! No joke!");
                cfg.CreateProfile("Other", p =>
                {
                    p.CreateMap<Source, Dest>();
                    p.ApplyTransform<string>(dest => dest + " is straight up dope");
                });
            });

            [Fact]
            public void ShouldApplyProfileFirstThenRoot()
            {
                var source = new Source
                {
                    Value = "Jimmy"
                };
                var dest = Mapper.Map<Source, Dest>(source);

                dest.Value.ShouldBe("Jimmy is straight up dope! No joke!");
            }
        }

    }
}