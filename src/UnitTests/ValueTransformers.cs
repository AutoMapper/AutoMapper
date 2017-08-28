﻿using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests
{
    namespace ValueTransformers
    {
        public class TransformsWithDifferentTypes : AutoMapperSpecBase
        {
            public class Source
            {
                public string ObjectValue { get; set; }
                public string Value { get; set; }
            }

            public class Dest
            {
                public string ObjectValue { get; set; }
                public string Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Dest>();
                cfg.ApplyTransform<string>(dest => dest + " is straight up dope");
                cfg.ApplyTransform<object>(dest => new object());
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

        public class TransformingValueTypes : AutoMapperSpecBase
        {
            public class Source
            {
                public int Value { get; set; }
            }

            public class Dest
            {
                public int Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.ApplyTransform<int>(dest => dest * 2);
                cfg.CreateProfile("Other", p =>
                {
                    p.CreateMap<Source, Dest>();
                    p.ApplyTransform<int>(dest => dest + 3);
                });
            });

            [Fact]
            public void ShouldApplyProfileFirstThenRoot()
            {
                var source = new Source
                {
                    Value = 5
                };
                var dest = Mapper.Map<Source, Dest>(source);

                dest.Value.ShouldBe((5 + 3) * 2);
            }
        }

        public class StackingRootAndProfileAndMemberConfig : AutoMapperSpecBase
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
                    p.CreateMap<Source, Dest>()
                     .ApplyTransform<IMappingExpression<Source,Dest>, string>(dest => dest + ", for real,");
                    p.ApplyTransform<string>(dest => dest + " is straight up dope");
                });
            });

            [Fact]
            public void ShouldApplyTypeMapThenProfileThenRoot()
            {
                var source = new Source
                {
                    Value = "Jimmy"
                };
                var dest = Mapper.Map<Source, Dest>(source);

                dest.Value.ShouldBe("Jimmy, for real, is straight up dope! No joke!");
            }
        }

        public class StackingTypeMapAndRootAndProfileAndMemberConfig : AutoMapperSpecBase
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
                    p.CreateMap<Source, Dest>()
                     .ApplyTransform<IMappingExpression<Source, Dest>, string>(dest => dest + ", for real,")
                     .ForMember(d => d.Value, opt => opt.ApplyTransform<string>(d => d + ", seriously"));
                    p.ApplyTransform<string>(dest => dest + " is straight up dope");
                });
            });

            [Fact]
            public void ShouldApplyTypeMapThenProfileThenRoot()
            {
                var source = new Source
                {
                    Value = "Jimmy"
                };
                var dest = Mapper.Map<Source, Dest>(source);

                dest.Value.ShouldBe("Jimmy, seriously, for real, is straight up dope! No joke!");
            }
        }

    }
}