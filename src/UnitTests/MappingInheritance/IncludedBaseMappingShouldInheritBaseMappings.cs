﻿using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    using AutoMapper;
    using CustomMapping;

    public class IncludedMappingShouldInheritBaseMappings : SpecBase
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

        [Fact]
        public void included_mapping_should_inherit_base_mappings_should_not_throw()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, DtoObject>()
                    .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                    .Include<ModelSubObject, DtoSubObject>();
                cfg.CreateMap<ModelSubObject, DtoSubObject>();
            });
            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void included_mapping_should_inherit_base_ignore_mappings_should_not_throw()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, DtoObject>()
                    .ForMember(d => d.BaseString, m => m.Ignore())
                    .Include<ModelSubObject, DtoSubObject>();
                cfg.CreateMap<ModelSubObject, DtoSubObject>();
            });
            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void more_specific_map_should_override_base_ignore_passes_validation()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, DtoObject>()
                    .ForMember(d => d.BaseString, m => m.Ignore())
                    .Include<ModelSubObject, DtoSubObject>();
                cfg.CreateMap<ModelSubObject, DtoSubObject>()
                    .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString));
            });
            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void more_specific_map_should_override_base_ignore_with_one_parameter()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, DtoObject>()
                    .ForMember(d => d.BaseString, m => m.Ignore())
                    .Include<ModelSubObject, DtoSubObject>();
                cfg.CreateMap<ModelSubObject, DtoSubObject>()
                    .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString));
            });

            var mapper = config.CreateMapper();

            var dto = mapper.Map<DtoSubObject>(new ModelSubObject
            {
                DifferentBaseString = "123",
                SubString = "456"
            });

            "123".ShouldBe(dto.BaseString);
            "456".ShouldBe(dto.SubString);
        }

        [Fact]
        public void more_specific_map_should_override_base_ignore()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, DtoObject>()
                    .ForMember(d => d.BaseString, m => m.Ignore())
                    .Include<ModelSubObject, DtoSubObject>();
                cfg.CreateMap<ModelSubObject, DtoSubObject>()
                    .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString));
            });
            var mapper = config.CreateMapper();
            var dto = mapper.Map<ModelSubObject, DtoSubObject>(new ModelSubObject
            {
                DifferentBaseString = "123",
                SubString = "456"
            });

            "123".ShouldBe(dto.BaseString);
            "456".ShouldBe(dto.SubString);
        }

        [Fact]
        public void more_specific_map_should_override_base_mapping_passes_validation()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, DtoObject>()
                    .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                    .Include<ModelSubObject, DtoSubObject>();
                cfg.CreateMap<ModelSubObject, DtoSubObject>()
                    .ForMember(d => d.BaseString, m => m.MapFrom(src => "789"));
            });
            config.AssertConfigurationIsValid();
        }
        [Fact]
        public void more_specific_map_should_override_base_mapping_with_one_parameter()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, DtoObject>()
                    .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                    .Include<ModelSubObject, DtoSubObject>();
                cfg.CreateMap<ModelSubObject, DtoSubObject>()
                    .ForMember(d => d.BaseString, m => m.MapFrom(src => "789"));
            });

            var mapper = config.CreateMapper();
            var dto = mapper.Map<DtoSubObject>(new ModelSubObject
                                                                   {
                                                                       DifferentBaseString = "123",
                                                                       SubString = "456"
                                                                   });

            "789".ShouldBe(dto.BaseString);
            "456".ShouldBe(dto.SubString);
        }
        
        [Fact]
        public void more_specific_map_should_override_base_mapping()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, DtoObject>()
                    .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                    .Include<ModelSubObject, DtoSubObject>();
                cfg.CreateMap<ModelSubObject, DtoSubObject>()
                    .ForMember(d => d.BaseString, m => m.MapFrom(src => "789"));
            });
            var mapper = config.CreateMapper();
            var dto = mapper.Map<ModelSubObject, DtoSubObject>(new ModelSubObject
                                                                   {
                                                                       DifferentBaseString = "123",
                                                                       SubString = "456"
                                                                   });

            "789".ShouldBe(dto.BaseString);
            "456".ShouldBe(dto.SubString);
        }

        [Fact]
        public void included_mapping_should_not_inherit_base_mappings_for_other_with_one_parameter()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, DtoObject>()
                    .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                    .Include<ModelSubObject, DtoSubObject>();

                cfg.CreateMap<ModelSubObject, OtherDto>();
                cfg.CreateMap<ModelSubObject, DtoSubObject>();
            });

            var mapper = config.CreateMapper();
            var dto = mapper.Map<OtherDto>(new ModelSubObject
            {
                DifferentBaseString = "123",
                SubString = "456"
            });

            "456".ShouldBe(dto.SubString);
        }

        [Fact]
        public void included_mapping_should_not_inherit_base_mappings_for_other()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, DtoObject>()
                    .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                    .Include<ModelSubObject, DtoSubObject>();

                cfg.CreateMap<ModelSubObject, OtherDto>();
                cfg.CreateMap<ModelSubObject, DtoSubObject>();
            });
            var mapper = config.CreateMapper();
            var dto = mapper.Map<ModelSubObject, OtherDto>(new ModelSubObject
            {
                DifferentBaseString = "123",
                SubString = "456"
            });

            "456".ShouldBe(dto.SubString);
        }

        [Fact]
        public void included_mapping_should_not_inherit_base_mappings_for_other_should_not_throw()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, DtoObject>()
                    .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                    .Include<ModelSubObject, DtoSubObject>();

                cfg.CreateMap<ModelSubObject, OtherDto>();
                cfg.CreateMap<ModelSubObject, DtoSubObject>();
            });
            config.AssertConfigurationIsValid();
        }
        [Fact]
        public void include_should_allow_automapper_to_select_more_specific_included_type_with_one_parameter()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, DtoObject>()
                    .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                    .Include<ModelSubObject, DtoSubObject>();

                cfg.CreateMap<ModelSubObject, DtoSubObject>();
            });
            var mapper = config.CreateMapper();
            var dto = mapper.Map<ModelObject, DtoObject>(new ModelSubObject
            {
                DifferentBaseString = "123",
                SubString = "456"
            });

            dto.ShouldBeOfType<DtoSubObject>();
        }
        
        [Fact]
        public void include_should_allow_automapper_to_select_more_specific_included_type()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, DtoObject>()
                    .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                    .Include<ModelSubObject, DtoSubObject>();

                cfg.CreateMap<ModelSubObject, DtoSubObject>();
            });
            var mapper = config.CreateMapper();
            var dto = mapper.Map<ModelObject, DtoObject>(new ModelSubObject
            {
                DifferentBaseString = "123",
                SubString = "456"
            });

            dto.ShouldBeOfType<DtoSubObject>();
        }

        [Fact]
        public void include_should_apply_condition()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, DtoObject>()
                    .ForMember(d => d.BaseString, m =>
                    {
                        m.Condition(src => !string.IsNullOrWhiteSpace(src.DifferentBaseString));
                        m.MapFrom(s => s.DifferentBaseString);
                    })
                    .Include<ModelSubObject, DtoSubObject>();

                cfg.CreateMap<ModelSubObject, DtoSubObject>();
            });
            var dest = new DtoSubObject
            {
                BaseString = "12345"
            };
            var mapper = config.CreateMapper();
            mapper.Map(new ModelSubObject
            {
                DifferentBaseString = "",
            }, dest);

            dest.BaseString.ShouldBe("12345");
        }

        [Fact]
        public void include_should_apply_null_substitute()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ModelObject, DtoObject>()
                    .ForMember(d => d.BaseString, m =>
                    {
                        m.MapFrom(s => s.DifferentBaseString);
                        m.NullSubstitute("12345");
                    })
                    .Include<ModelSubObject, DtoSubObject>();

                cfg.CreateMap<ModelSubObject, DtoSubObject>();
            });
            var mapper = config.CreateMapper();
            var dest = mapper.Map<ModelSubObject, DtoSubObject>(new ModelSubObject());

            dest.BaseString.ShouldBe("12345");
        }
    }

    public class OverrideDifferentMapFrom : AutoMapperSpecBase
    {
        class Source
        {
        }
        class Destination
        {
            public int Value { get; set; }
        }
        class DestinationDerived : Destination
        {
        }
        protected override MapperConfiguration CreateConfiguration() => new(cfg=>
        {
            cfg.CreateMap<Source, Destination>().ForMember(d => d.Value, o => o.MapFrom((s, d) => 1));
            cfg.CreateMap<Source, DestinationDerived>().IncludeBase<Source, Destination>().ForMember(d => d.Value, o => o.MapFrom(s => 2));
        });
        [Fact]
        public void Should_use_derived_mapfrom() => Map<DestinationDerived>(new Source()).Value.ShouldBe(2);
    }
}
