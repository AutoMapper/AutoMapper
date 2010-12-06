using NUnit.Framework;

namespace AutoMapper.UnitTests.MappingInheritance
{
    [TestFixture]
    public class IgnoreShouldBeInheritedIfConventionCannotMap
    {
        [SetUp]
        public void SetUp()
        {
            Mapper.Reset();
        }

        private class BaseDomain
        {

        }

        private class StandardDomain : BaseDomain
        {
            
        }

        private class SpecificDomain : StandardDomain
        {
        }

        private class MoreSpecificDomain : SpecificDomain
        {
            
        }

        private class Dto
        {
            public string SpecificProperty { get; set; }
        }

        [Test]
        public void inhertited_ignore_should_be_overridden_passes_validation()
        {
            Mapper.CreateMap<BaseDomain, Dto>()
                .ForMember(d => d.SpecificProperty, m => m.Ignore())
                .Include<StandardDomain, Dto>();

            Mapper.CreateMap<StandardDomain, Dto>()
                .Include<SpecificDomain, Dto>();

            Mapper.CreateMap<SpecificDomain, Dto>()
                .Include<MoreSpecificDomain, Dto>();

            Mapper.CreateMap<MoreSpecificDomain, Dto>();

            Mapper.AssertConfigurationIsValid();
        }
    }
}
