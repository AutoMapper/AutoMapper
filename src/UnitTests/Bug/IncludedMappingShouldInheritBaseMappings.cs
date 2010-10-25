using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
    [TestFixture]
    public class IncludedMappingShouldInheritBaseMappings
    {
        public class ModelObject
        {
            public string DifferentBaseString { get; set; }
        }

        public class ModelSubObject : ModelObject
        {
            public string SubString { get; set; }
        }

        public class DtoObject
        {
            public string BaseString { get; set; }
        }

        public class DtoSubObject : DtoObject
        {
            public string SubString { get; set; }
        }

        public class OtherDto
        {
            public string SubString { get; set; }
        }

        [Test]
        public void included_mapping_should_inherit_base_mappings_should_not_throw()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                .Include<ModelSubObject, DtoSubObject>();
            Mapper.CreateMap<ModelSubObject, DtoSubObject>();

            Mapper.AssertConfigurationIsValid();
        }

        [Test]
        public void included_mapping_should_inherit_base_mappings()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                .Include<ModelSubObject, DtoSubObject>();
            Mapper.CreateMap<ModelSubObject, DtoSubObject>();

            var dto = Mapper.Map<ModelSubObject, DtoSubObject>(new ModelSubObject
                                                                   {
                                                                       DifferentBaseString = "123",
                                                                       SubString = "456"
                                                                   });

            Assert.AreEqual("123", dto.BaseString);
            Assert.AreEqual("456", dto.SubString);
        }

        [Test]
        public void included_mapping_should_not_inherit_base_mappings_for_other()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                .Include<ModelSubObject, DtoSubObject>();

            Mapper.CreateMap<ModelSubObject, OtherDto>();

            var dto = Mapper.Map<ModelSubObject, OtherDto>(new ModelSubObject
            {
                DifferentBaseString = "123",
                SubString = "456"
            });

            Assert.AreEqual("456", dto.SubString);
        }

        [Test]
        public void included_mapping_should_not_inherit_base_mappings_for_other_should_not_throw()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                .Include<ModelSubObject, DtoSubObject>();

            Mapper.CreateMap<ModelSubObject, OtherDto>();

            Mapper.AssertConfigurationIsValid();
        }
    }
}
