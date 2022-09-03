namespace AutoMapper.UnitTests.Bug;

public class When_mapping_for_derived_class_is_duplicated : NonValidatingSpecBase
{
    public class ModelObject
    {
        public string BaseString { get; set; }
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

    [Fact]
    public void should_not_throw_duplicated_key_exception()
    {
        new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ModelSubObject, DtoObject>()
                .Include<ModelSubObject, DtoSubObject>();

            cfg.CreateMap<ModelSubObject, DtoSubObject>();

            cfg.CreateMap<ModelSubObject, DtoObject>()
                .Include<ModelSubObject, DtoSubObject>();

            cfg.CreateMap<ModelSubObject, DtoSubObject>();
        });
    }
}
