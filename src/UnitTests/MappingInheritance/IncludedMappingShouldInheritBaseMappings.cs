using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
    [TestFixture]
    public class IncludedMappingShouldInheritBaseMappings
    {
        [SetUp]
        public void SetUp()
        {
            Mapper.Reset();
        }

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
        public void included_mapping_should_inherit_base_ignore_mappings_should_not_throw()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.Ignore())
                .Include<ModelSubObject, DtoSubObject>();
            Mapper.CreateMap<ModelSubObject, DtoSubObject>();

            Mapper.AssertConfigurationIsValid();
        }

        [Test]
        public void more_specific_map_should_override_base_ignore_passes_validation()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.Ignore())
                .Include<ModelSubObject, DtoSubObject>();
            Mapper.CreateMap<ModelSubObject, DtoSubObject>()
                .ForMember(d=>d.BaseString, m=>m.MapFrom(s=>s.DifferentBaseString));

            Mapper.AssertConfigurationIsValid();
        }

        [Test]
        public void more_specific_map_should_override_base_ignore_with_one_parameter()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.Ignore())
                .Include<ModelSubObject, DtoSubObject>();
            Mapper.CreateMap<ModelSubObject, DtoSubObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString));

            var dto = Mapper.Map<DtoSubObject>(new ModelSubObject
            {
                DifferentBaseString = "123",
                SubString = "456"
            });

            Assert.AreEqual("123", dto.BaseString);
            Assert.AreEqual("456", dto.SubString);
        }

        [Test]
        public void more_specific_map_should_override_base_ignore()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.Ignore())
                .Include<ModelSubObject, DtoSubObject>();
            Mapper.CreateMap<ModelSubObject, DtoSubObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString));

            var dto = Mapper.Map<ModelSubObject, DtoSubObject>(new ModelSubObject
            {
                DifferentBaseString = "123",
                SubString = "456"
            });

            Assert.AreEqual("123", dto.BaseString);
            Assert.AreEqual("456", dto.SubString);
        }

        [Test]
        public void more_specific_map_should_override_base_mapping_passes_validation()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                .Include<ModelSubObject, DtoSubObject>();
            Mapper.CreateMap<ModelSubObject, DtoSubObject>()
                .ForMember(d => d.BaseString, m => m.UseValue("789"));

            Mapper.AssertConfigurationIsValid();
        }
        [Test]
        public void more_specific_map_should_override_base_mapping_with_one_parameter()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                .Include<ModelSubObject, DtoSubObject>();
            Mapper.CreateMap<ModelSubObject, DtoSubObject>()
                .ForMember(d=>d.BaseString, m=>m.UseValue("789"));

            var dto = Mapper.Map<DtoSubObject>(new ModelSubObject
                                                                   {
                                                                       DifferentBaseString = "123",
                                                                       SubString = "456"
                                                                   });

            Assert.AreEqual("789", dto.BaseString);
            Assert.AreEqual("456", dto.SubString);
        }
        
        [Test]
        public void more_specific_map_should_override_base_mapping()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                .Include<ModelSubObject, DtoSubObject>();
            Mapper.CreateMap<ModelSubObject, DtoSubObject>()
                .ForMember(d=>d.BaseString, m=>m.UseValue("789"));

            var dto = Mapper.Map<ModelSubObject, DtoSubObject>(new ModelSubObject
                                                                   {
                                                                       DifferentBaseString = "123",
                                                                       SubString = "456"
                                                                   });

            Assert.AreEqual("789", dto.BaseString);
            Assert.AreEqual("456", dto.SubString);
        }

        [Test]
        public void included_mapping_should_not_inherit_base_mappings_for_other_with_one_parameter()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                .Include<ModelSubObject, DtoSubObject>();

            Mapper.CreateMap<ModelSubObject, OtherDto>();

            var dto = Mapper.Map<OtherDto>(new ModelSubObject
            {
                DifferentBaseString = "123",
                SubString = "456"
            });

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
        [Test]
        public void include_should_allow_automapper_to_select_more_specific_included_type_with_one_parameter()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                .Include<ModelSubObject, DtoSubObject>();

            Mapper.CreateMap<ModelSubObject, DtoSubObject>();

            var dto = Mapper.Map<ModelObject, DtoObject>(new ModelSubObject
            {
                DifferentBaseString = "123",
                SubString = "456"
            });

            Assert.IsInstanceOfType(typeof(DtoSubObject), dto);
        }
        
        [Test]
        public void include_should_allow_automapper_to_select_more_specific_included_type()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                .Include<ModelSubObject, DtoSubObject>();

            Mapper.CreateMap<ModelSubObject, DtoSubObject>();

            var dto = Mapper.Map<ModelObject, DtoObject>(new ModelSubObject
            {
                DifferentBaseString = "123",
                SubString = "456"
            });

            Assert.IsInstanceOfType(typeof(DtoSubObject), dto);
        }
    }
}
