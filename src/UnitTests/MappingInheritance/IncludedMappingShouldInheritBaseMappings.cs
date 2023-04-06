namespace AutoMapper.UnitTests;
public class IncludeBaseIndirectBase : AutoMapperSpecBase
{
    public class FooBaseBase
    {
    }
    public class FooBase : FooBaseBase
    {
    }
    public class Foo : FooBase
    {
    }
    public class FooDtoBaseBase
    {
        public DateTime Date { get; set; }
    }
    public class FooDtoBase : FooDtoBaseBase
    {
    }
    public class FooDto : FooDtoBase
    {
    }
    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.CreateMap<Foo, FooDto>().IncludeBase<FooBase, FooDtoBase>();
        c.CreateMap<FooBase, FooDtoBase>().IncludeBase<FooBaseBase, FooDtoBaseBase>();
        c.CreateMap<FooBaseBase, FooDtoBaseBase>().ForMember(d => d.Date, o => o.MapFrom(s => DateTime.MaxValue));
    });
    [Fact]
    public void Should_work() => Map<FooDto>(new Foo()).Date.ShouldBe(DateTime.MaxValue);
}
public class ReadonlyCollectionPropertiesOverride : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<SourceBase, DestinationBase>()
            .Include<Source, Destination>()
            .ForMember(d=>d.CodeList, o => o.UseDestinationValue());
        cfg.CreateMap<Source, Destination>().ForMember(d=>d.CodeList, o => o.DoNotUseDestinationValue());
    });
    public class SourceBase
    {
        public ICollection<string> CodeList { get; } = new List<string>();
    }
    public class Source : SourceBase
    {
    }
    public class DestinationBase
    {
        public ICollection<string> CodeList { get; set; } = new HashSet<string>();
    }
    public class Destination : DestinationBase
    {
    }
    [Fact]
    public void ShouldMapOk() => Mapper.Map<Destination>(new Source { CodeList = { "DMItemCode1" } }).CodeList.ShouldNotBeOfType<HashSet<string>>();
}
public class ReadonlyCollectionProperties : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg=>
    {
        cfg.CreateMap<DomainModelBase, ModelBase>()
            .ForMember(d => d.CodeList, o => o.MapFrom(s => s.CodeList))
            .ForMember(d => d.KeyValuesOtherName, o => o.MapFrom(s => new[] { new KeyValueModel { Key = "key1", Value = "value1" } }))
            .Include<DomainModel, Model>();
        cfg.CreateMap<DomainModel, Model>();
    });
    public class DomainModelBase
    {
        public ICollection<string> CodeList { get; } = new List<string>();
    }
    public class DomainModel : DomainModelBase
    {
    }
    public class ModelBase
    {
        public ICollection<KeyValueModel> KeyValuesOtherName { get; } = new List<KeyValueModel>();
        public ICollection<string> CodeList { get; } = new List<string>();
    }
    public class Model : ModelBase
    {
    }
    public class KeyValueModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
    [Fact]
    public void ShouldMapOk()
    {
        var domainModel = new DomainModel { CodeList = { "DMItemCode1" } };
        var result = Mapper.Map<Model>(domainModel);
        result.CodeList.First().ShouldBe("DMItemCode1");
        var keyValue = result.KeyValuesOtherName.First();
        keyValue.Key.ShouldBe("key1");
        keyValue.Value.ShouldBe("value1");
    }
}
public class IncludedBaseMappingShouldInheritBaseMappings : NonValidatingSpecBase
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
                ;
            cfg.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>();
        });

        config.AssertConfigurationIsValid();
    }
    [Fact]
    public void included_mapping_should_not_care_about_order()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>();
            cfg.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                ;
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
                ;
            cfg.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
                ;
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
                ;
            cfg.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                ;
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
                ;
            cfg.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                ;
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
                ;
            cfg.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString))
                ;
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
                ;
            cfg.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
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
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString));
            cfg.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
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
                ;
            cfg.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
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
    public void include_should_allow_automapper_to_select_more_specific_included_type_with_one_parameter()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ModelObject, DtoObject>()
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString));

            cfg.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
                ;
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
                .ForMember(d => d.BaseString, m => m.MapFrom(s => s.DifferentBaseString));

            cfg.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>();
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
                ;

            cfg.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
                ;
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
                ;

            cfg.CreateMap<ModelSubObject, DtoSubObject>()
                .IncludeBase<ModelObject, DtoObject>()
                ;
        });
        var mapper = config.CreateMapper();
        var dest = mapper.Map<ModelSubObject, DtoSubObject>(new ModelSubObject());

        dest.BaseString.ShouldBe("12345");
    }
}
