using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    namespace AddingConfigurationForNonMatchingDestinationMember
    {
        public class ReverseMapDestinationMembersValidationBug : NonValidatingSpecBase
        {
            public class Source
            {
                public int SomeValue { get; set; }
                public int SomeValue2 { get; set; }
            }

            public class Destination
            {
                public int SomeValue { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateMap<Source, Destination>(MemberList.Destination).ReverseMap()
                    .ValidateMemberList(MemberList.Destination);
            });

            [Fact]
            public void Should_show_configuration_error()
            {
                typeof(AutoMapperConfigurationException).ShouldBeThrownBy(AssertConfigurationIsValid);
            }
        }
    }
}