namespace AutoMapper.UnitTests.Bug
{
    namespace ConditionBug
    {
        using System.Collections.Generic;
        using Should;
        using Xunit;

        public class Example : AutoMapperSpecBase
        {
            public class SubSource
            {
                public string SubValue { get; set; }
            }

            public class Source
            {
                public Source()
                {
                    Value = new List<SubSource>();
                }

                public List<SubSource> Value { get; set; }
            }

            public class Destination
            {
                public string Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>()
                    .ForMember(dest => dest.Value, opt =>
                    {
                        opt.PreCondition(src => src.Value.Count > 1);
                        opt.ResolveUsing(src => src.Value[1].SubValue);
                    });
            });

            [Fact]
            public void Should_skip_the_mapping_when_the_condition_is_false()
            {
                var src = new Source();
                src.Value.Add(new SubSource {SubValue = "x"});
                var destination = Mapper.Map<Source, Destination>(src);

                destination.Value.ShouldBeNull();
            }

            [Fact]
            public void Should_execute_the_mapping_when_the_condition_is_true()
            {
                var src = new Source();
                src.Value.Add(new SubSource {SubValue = "x"});
                src.Value.Add(new SubSource {SubValue = "x"});
                var destination = Mapper.Map<Source, Destination>(src);

                destination.Value.ShouldEqual("x");
            }
        }

        public class PrimitiveExample : AutoMapperSpecBase
        {
            public class Source
            {
                public int? Value { get; set; }
            }

            public class Destination
            {
                public int Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
                cfg.CreateMap<Source, Destination>()
                    .ForMember(d => d.Value, opt =>
                    {
                        opt.PreCondition(src => src.Value.HasValue);
                        opt.MapFrom(src => src.Value.Value + 10);
                    }));


            [Fact]
            public void Should_skip_when_condition_not_met()
            {
                var dest = Mapper.Map<Source, Destination>(new Source());

                dest.Value.ShouldEqual(0);
            }
        }
    }

    namespace ConditionPropertyBug
    {
        using System;
        using Should;
        using Xunit;

        public class Example : AutoMapperSpecBase
        {
            public class Source
            {
                private int basePrice;
                public bool HasBasePrice { get; set; }
                public int BasePrice
                {
                    get
                    {
                        if (!HasBasePrice)
                            throw new InvalidOperationException("Has no base price");

                        return basePrice;
                    }
                    set
                    {
                        basePrice = value;
                        HasBasePrice = true;
                    }
                }
            }

            public class Destination
            {
                public int BasePrice { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
                cfg.CreateMap<Source, Destination>()
                    .ForMember(itemDTO => itemDTO.BasePrice,
                        config =>
                        {
                            config.PreCondition(item => item.HasBasePrice);
                            config.MapFrom(item => item.BasePrice);
                        }));

            [Fact]
            public void Should_skip_the_mapping_when_the_condition_property_is_false()
            {
                var src = new Source();
                var dest = Mapper.Map<Source, Destination>(src);

                dest.BasePrice.ShouldEqual(0);
            }

            [Fact]
            public void Should_execute_the_mapping_when_the_condition_property_is_true()
            {
                var src = new Source {BasePrice = 15};
                var dest = Mapper.Map<Source, Destination>(src);

                dest.BasePrice.ShouldEqual(src.BasePrice);
            }
        }
    }


    namespace SourceValueConditionPropertyBug
    {
        using Should;
        using Xunit;

        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public int? Value { get; set; }
        }

        public class ConditionTests : AutoMapperSpecBase
        {
            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Dest>()
                    .ForMember(d => d.Value, opt => opt.Condition((src, dest, srcVal, destVal) => destVal == null));
            });

            [Fact]
            public void Should_map_value_when_null()
            {
                var destination = new Dest();
                Mapper.Map(new Source {Value = 5}, destination);
                destination.Value.ShouldEqual(5);
            }

            [Fact]
            public void Should_not_map_value_when_not_null()
            {
                var destination = new Dest { Value = 6};
                Mapper.Map(new Source {Value = 5}, destination);
                destination.Value.ShouldEqual(6);
            }
        }
    }

    namespace SourceValueExceptionConditionPropertyBug
    {
        using System;
        using Should;
        using Xunit;

        public class Source
        {
            public bool Accessed = false;
            public int Value
            {
                get
                {
                    Accessed = true;
                    return 5;
                }
            }
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        public class ConditionTests : NonValidatingSpecBase
        {
            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Dest>()
                    .ForMember(d => d.Value, opt => opt.PreCondition((ResolutionContext rc) => false));
            });

            [Fact]
            public void Should_not_map()
            {
                var source = new Source();
                Mapper.Map<Source, Dest>(source);
                source.Accessed.ShouldBeFalse();
            }
        }
    }

}
