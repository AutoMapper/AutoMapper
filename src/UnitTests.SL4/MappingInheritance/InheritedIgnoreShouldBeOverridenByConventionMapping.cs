using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class InheritedIgnoreShouldBeOverriddenByConventionMapping
    {
        public class BaseDomain
        {
            
        }

        public class SpecificDomain : BaseDomain
        {
            public string SpecificProperty { get; set; }            
        }

        public class Dto
        {
            public string SpecificProperty { get; set; }
        }

        [Fact]
        public void inhertited_ignore_should_be_pass_validation()
        {
            Mapper.CreateMap<BaseDomain, Dto>()
                .ForMember(d => d.SpecificProperty, m => m.Ignore())
                .Include<SpecificDomain, Dto>();

            Mapper.CreateMap<SpecificDomain, Dto>();

            Mapper.AssertConfigurationIsValid();
        }

        [Fact]
        public void inhertited_ignore_should_be_overridden_by_successful_convention_mapping()
        {
            Mapper.CreateMap<BaseDomain, Dto>()
                .ForMember(d=>d.SpecificProperty, m=>m.Ignore())
                .Include<SpecificDomain, Dto>();

            Mapper.CreateMap<SpecificDomain, Dto>();

            var dto = Mapper.Map<BaseDomain, Dto>(new SpecificDomain {SpecificProperty = "Test"});

            dto.SpecificProperty.ShouldEqual("Test");
        }
        
        [Fact]
        public void inhertited_ignore_should_be_overridden_by_successful_convention_mapping_with_one_parameter()
        {
            Mapper.CreateMap<BaseDomain, Dto>()
                .ForMember(d => d.SpecificProperty, m => m.Ignore())
                .Include<SpecificDomain, Dto>();

            Mapper.CreateMap<SpecificDomain, Dto>();

            var dto = Mapper.Map<Dto>(new SpecificDomain { SpecificProperty = "Test" });

            dto.SpecificProperty.ShouldEqual("Test");
        }
    }
}
