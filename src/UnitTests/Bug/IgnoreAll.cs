using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class When_configuring_all_members_and_some_do_not_match
    {
        public class ModelObjectNotMatching
        {
            public string Foo_notfound { get; set; }
            public string Bar_notfound;
        }

        public class ModelDto
        {
            public string Foo { get; set; }
            public string Bar;
        }

        public When_configuring_all_members_and_some_do_not_match()
        {
            SetUp();
        }
        public void SetUp()
        {
            Mapper.Reset();
        }

        [Fact]
        public void Should_still_apply_configuration_to_missing_members()
        {
            Mapper.CreateMap<ModelObjectNotMatching, ModelDto>()
                .ForAllMembers(opt => opt.Ignore());
            Mapper.AssertConfigurationIsValid();
        }
    }
}