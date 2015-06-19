namespace AutoMapper.UnitTests.Mappers
{
    namespace CustomMapperTests
    {
        //TODO: will start by pulling the static-based tests forward as best I can...
        //TODO: will want to establish non-static instance-based versions as well...
        using Xunit;

        public class When_adding_a_custom_mapper : NonValidatingSpecBase
        {
            protected override void Establish_context()
            {
                Mapper.Initialize(cfg =>
                {
                    cfg.ObjectMappers.Insert(0, new TestObjectMapper());

                    cfg.CreateMap<ClassA, ClassB>()
                        .ForMember(dest => dest.Destination, opt => opt.MapFrom(src => src.Source));
                });
            }

            [Fact]
            public void Should_have_valid_configuration()
            {
                typeof (AutoMapperConfigurationException).ShouldNotBeThrownBy(Mapper.AssertConfigurationIsValid);
            }

            /// <summary>
            /// 
            /// </summary>
            public class TestObjectMapper : IObjectMapper
            {
                /// <summary>
                /// 
                /// </summary>
                /// <param name="context"></param>
                /// <returns></returns>
                public object Map(ResolutionContext context)
                {
                    return new DestinationType();
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="context"></param>
                /// <returns></returns>
                public bool IsMatch(ResolutionContext context)
                {
                    return context.SourceType == typeof (SourceType)
                           && context.DestinationType == typeof (DestinationType);
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