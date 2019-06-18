using System;
using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    namespace AddingConfigurationForNonMatchingDestinationMember
    {
        public class AddingConfigurationForNonMatchingDestinationMemberBug : NonValidatingSpecBase
        {
            public class Source
            {
                
            }

            public class Destination
            {
                public string Value { get; set; }
            }

            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Destination>()
                    .ForMember(dest => dest.Value, opt => opt.NullSubstitute("Foo"));
            });

            [Fact]
            public void Should_show_configuration_error()
            {
                typeof (AutoMapperConfigurationException).ShouldBeThrownBy(Configuration.AssertConfigurationIsValid);
            }
        }
    }
}