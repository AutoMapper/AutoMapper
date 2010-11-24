using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
    [TestFixture]
    public class MultipleMappingsOfSameTypeFails
    {
        private class MyClass
        {
            public ActivityBase Information { get; set; }
            public InformationClass CurrentInformation { get; set; }
        }

        private class MySpecificClass :MyClass{}

        private class MyDto
        {
            public InformationDto Information { get; set; }
        }

        private class MySpecificDto : MyDto{}
        private class InformationDto{}
        private class ActivityBase{}
        private class InformationBase{}
        private class InformationClass{}

        [Test]
        public void multiple_inherited_base_mappings_of_same_type_fails()
        {
            Mapper.CreateMap<MyClass, MyDto>()
                .ForMember(d=> d.Information, m => m.MapFrom(s=>s.CurrentInformation))
                .Include<MySpecificClass, MySpecificDto>();
            Mapper.CreateMap<MySpecificClass, MySpecificDto>();

            Mapper.CreateMap<InformationClass, InformationDto>();

            Mapper.AssertConfigurationIsValid();
        }
    }

    
}
