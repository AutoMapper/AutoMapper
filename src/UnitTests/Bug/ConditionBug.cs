namespace AutoMapper.UnitTests.Bug
{
    namespace ConditionBug
    {
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

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateMap<Source, Destination>()
                    .ForMember(dest => dest.Value, opt =>
                    {
                        opt.PreCondition(src => src.Value.Count > 1);
                        opt.MapFrom(src => src.Value[1].SubValue);
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

                destination.Value.ShouldBe("x");
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

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
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

                dest.Value.ShouldBe(0);
            }
        }
    }

    namespace ConditionPropertyBug
    {
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

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
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

                dest.BasePrice.ShouldBe(0);
            }

            [Fact]
            public void Should_execute_the_mapping_when_the_condition_property_is_true()
            {
                var src = new Source {BasePrice = 15};
                var dest = Mapper.Map<Source, Destination>(src);

                dest.BasePrice.ShouldBe(src.BasePrice);
            }
        }
    }


    namespace SourceValueConditionPropertyBug
    {
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
            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateMap<Source, Dest>()
                    .ForMember(d => d.Value, opt => opt.Condition((src, dest, srcVal, destVal) => destVal == null));
            });

            [Fact]
            public void Should_map_value_when_null()
            {
                var destination = new Dest();
                Mapper.Map(new Source {Value = 5}, destination);
                destination.Value.ShouldBe(5);
            }

            [Fact]
            public void Should_not_map_value_when_not_null()
            {
                var destination = new Dest { Value = 6};
                Mapper.Map(new Source {Value = 5}, destination);
                destination.Value.ShouldBe(6);
            }
        }
    }

    namespace SourceValueExceptionConditionPropertyBug
    {
        public enum Property
        {
            Value1 = 0,
            Value2 = 1,
            Value3 = 2,
            Value4 = 3
        }

        public class Source
        {
            public Dictionary<Property, bool> Accessed = new Dictionary<Property, bool>
            {
                {Property.Value1, false},
                {Property.Value2, false},
                {Property.Value3, false},
                {Property.Value4, false}
            };

            public int Value1
            {
                get
                {
                    Accessed[Property.Value1] = true;
                    return 5;
                }
            }

            public int Value2
            {
                get
                {
                    Accessed[Property.Value2] = true;
                    return 10;
                }
            }

            public int Value3
            {
                get
                {
                    Accessed[Property.Value3] = true;
                    return 15;
                }
            }

            public int Value4
            {
                get
                {
                    Accessed[Property.Value4] = true;
                    return 20;
                }
            }
        }

        public class Dest
        {
            public int Value1 { get; set; }

            public int Value2 { get; set; }

            public int Value3 { get; set; }

            public int Value4 { get; set; }

            public bool MarkerBool { get; set; }
        }

        public class ConditionTests : NonValidatingSpecBase
        {
            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateMap<Source, Dest>()
                    .ForMember(d => d.Value1, opt => opt.PreCondition((Source src) => false))
                    .ForMember(d => d.Value2, opt => opt.PreCondition((ResolutionContext rc) => false))
                    .ForMember(d => d.Value3, opt => opt.PreCondition((src, rc) => false))//;
                    .ForMember(d => d.Value4, opt => opt.PreCondition((src, dest, rc) =>
                    {
                        dest.MarkerBool = true;
                        return false;
                    }));
            });

            [Fact]
            public void Should_not_map_when_precondition_with_source_parameter_is_false()
            {
                var source = new Source();
                Mapper.Map<Source, Dest>(source);
                source.Accessed[Property.Value1].ShouldBeFalse();
            }

            [Fact]
            public void Should_not_map_when_precondition_with_resolutioncontext_parameter_is_false()
            {
                var source = new Source();
                Mapper.Map<Source, Dest>(source);
                source.Accessed[Property.Value2].ShouldBeFalse();
            }

            [Fact]
            public void Should_not_map_when_precondition_with_source_and_resolutioncontext_parameters_is_false()
            {
                var source = new Source();
                Mapper.Map<Source, Dest>(source);
                source.Accessed[Property.Value3].ShouldBeFalse();
            }

            [Fact]
            public void Should_not_map_and_should_produce_sideeffect_when_precondition_with_source_and_desc_parameters_is_false()
            {
                var source = new Source();
                var dest = Mapper.Map<Source, Dest>(source);
                source.Accessed[Property.Value4].ShouldBeFalse();
                dest.MarkerBool.ShouldBeTrue();
            }

        }
    }

}
