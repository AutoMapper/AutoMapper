namespace AutoMapper.UnitTests.Mappers
{
    namespace CustomMapperTests
    {
        using AutoMapper.Mappers;
        using Xunit;

        public class When_adding_a_custom_mapper : NonValidatingSpecBase
        {
            public When_adding_a_custom_mapper()
            {
                MapperRegistry.Mappers.Insert(0, new TestObjectMapper());
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ClassA, ClassB>()
                    .ForMember(dest => dest.Destination, opt => opt.MapFrom(src => src.Source));
            });

            [Fact]
            public void Should_have_valid_configuration()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Configuration.AssertConfigurationIsValid);
            }


            public class TestObjectMapper : IObjectMapper
            {
                public object Map(ResolutionContext context)
                {
                    return new DestinationType();
                }

                public bool IsMatch(TypePair context)
                {
                    return context.SourceType == typeof(SourceType) && context.DestinationType == typeof(DestinationType);
                }
            }

            public class ClassA
            {
                public SourceType Source { get; set; }
            }

            public class ClassB
            {
                public DestinationType Destination { get; set; }
            }

            public class SourceType
            {
                public int Value { get; set; }
            }

            public class DestinationType
            {
                public bool Value { get; set; }
            }
        }

    }
}