namespace AutoMapper.UnitTests.Mappers
{
    namespace CustomMapperTests
    {
        using AutoMapper.Mappers;
        using Xunit;

        public class When_adding_a_custom_mapper : NonValidatingSpecBase
        {
            protected override void Establish_context()
            {
                MapperRegistry.Mappers.Insert(0, new TestObjectMapper());

                Mapper.CreateMap<ClassA, ClassB>()
                    .ForMember(dest => dest.Destination, opt => opt.MapFrom(src => src.Source));
            }

            [Fact]
            public void Should_have_valid_configuration()
            {
                typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Mapper.AssertConfigurationIsValid);
            }


            public class TestObjectMapper : IObjectMapper
            {
                public object Map(ResolutionContext context, IMappingEngineRunner mapper)
                {
                    return new DestinationType();
                }

                public bool IsMatch(ResolutionContext context)
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