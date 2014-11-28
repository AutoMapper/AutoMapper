using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class IncludedBaseMappingShouldInheritBaseMappings
    {
        public IncludedBaseMappingShouldInheritBaseMappings()
        {
            SetUp();
        }
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

        [Fact]
        public void included_mapping_should_inherit_base_mappings_should_not_throw()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                ;
            Mapper.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>();

            Mapper.AssertConfigurationIsValid();
        }

        [Fact]
        public void included_mapping_should_inherit_base_ignore_mappings_should_not_throw()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.Ignore())
                ;
            Mapper.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
                ;

            Mapper.AssertConfigurationIsValid();
        }

        [Fact]
        public void more_specific_map_should_override_base_ignore_passes_validation()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.Ignore())
                ;
            Mapper.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                ;

            Mapper.AssertConfigurationIsValid();
        }

        [Fact]
        public void more_specific_map_should_override_base_ignore_with_one_parameter()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.Ignore())
                ;
            Mapper.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                ;

            var dto = Mapper.Map<DtoSubObject>(new ModelSubObject
            {
                DifferentBaseString = "123",
                SubString = "456"
            });

            "123".ShouldEqual(dto.BaseString);
            "456".ShouldEqual(dto.SubString);
        }

        [Fact]
        public void more_specific_map_should_override_base_ignore()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.Ignore())
                ;
            Mapper.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                ;

            var dto = Mapper.Map<ModelSubObject, DtoSubObject>(new ModelSubObject
            {
                DifferentBaseString = "123",
                SubString = "456"
            });

            "123".ShouldEqual(dto.BaseString);
            "456".ShouldEqual(dto.SubString);
        }

        [Fact]
        public void more_specific_map_should_override_base_mapping_passes_validation()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                ;
            Mapper.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.UseValue("789"));

            Mapper.AssertConfigurationIsValid();
        }
        [Fact]
        public void more_specific_map_should_override_base_mapping_with_one_parameter()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString));
            Mapper.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.UseValue("789"));

            var dto = Mapper.Map<DtoSubObject>(new ModelSubObject
                                                                   {
                                                                       DifferentBaseString = "123",
                                                                       SubString = "456"
                                                                   });

            "789".ShouldEqual(dto.BaseString);
            "456".ShouldEqual(dto.SubString);
        }
        
        [Fact]
        public void more_specific_map_should_override_base_mapping()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                ;
            Mapper.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.UseValue("789"));

            var dto = Mapper.Map<ModelSubObject, DtoSubObject>(new ModelSubObject
                                                                   {
                                                                       DifferentBaseString = "123",
                                                                       SubString = "456"
                                                                   });

            "789".ShouldEqual(dto.BaseString);
            "456".ShouldEqual(dto.SubString);
        }

        [Fact]
        public void include_should_allow_automapper_to_select_more_specific_included_type_with_one_parameter()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString));

            Mapper.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
                ;

            var dto = Mapper.Map<ModelObject, DtoObject>(new ModelSubObject
            {
                DifferentBaseString = "123",
                SubString = "456"
            });

            dto.ShouldBeType<DtoSubObject>();
        }
        
        [Fact]
        public void include_should_allow_automapper_to_select_more_specific_included_type()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString));

            Mapper.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>();

            var dto = Mapper.Map<ModelObject, DtoObject>(new ModelSubObject
            {
                DifferentBaseString = "123",
                SubString = "456"
            });

            dto.ShouldBeType<DtoSubObject>();
        }

        [Fact]
        public void include_should_apply_condition()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m =>
                {
                    m.Condition(src => !string.IsNullOrWhiteSpace(src.DifferentBaseString));
                    m.MapFrom(s => s.DifferentBaseString);
                })
                ;

            Mapper.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
                ;

            var dest = new DtoSubObject
            {
                BaseString = "12345"
            };
            Mapper.Map(new ModelSubObject
            {
                DifferentBaseString = "",
            }, dest);

            dest.BaseString.ShouldEqual("12345");
        }

        [Fact]
        public void include_should_apply_null_substitute()
        {
            Mapper.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m =>
                {
                    m.MapFrom(s => s.DifferentBaseString);
                    m.NullSubstitute("12345");
                })
                ;

            Mapper.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
                ;

            var dest = Mapper.Map<ModelSubObject, DtoSubObject>(new ModelSubObject());

            dest.BaseString.ShouldEqual("12345");
        }
    }
}
