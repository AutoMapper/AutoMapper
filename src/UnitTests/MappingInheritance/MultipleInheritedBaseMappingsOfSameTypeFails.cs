namespace AutoMapper.UnitTests.Bug;

public class MultipleMappingsOfSameTypeFails
{
    public class MyClass
    {
        public ActivityBase Information { get; set; }
        public InformationClass CurrentInformation { get; set; }
    }

    public class MySpecificClass :MyClass{}

    public class MyDto
    {
        public InformationDto Information { get; set; }
    }

    public class MySpecificDto : MyDto{}
    public class InformationDto{}
    public class ActivityBase{}
    public class InformationBase{}
    public class InformationClass{}

    [Fact]
    public void multiple_inherited_base_mappings_of_same_type_fails()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<MyClass, MyDto>()
                .ForMember(d => d.Information, m => m.MapFrom(s => s.CurrentInformation))
                .Include<MySpecificClass, MySpecificDto>();
            cfg.CreateMap<MySpecificClass, MySpecificDto>();

            cfg.CreateMap<InformationClass, InformationDto>();
        });


        config.AssertConfigurationIsValid();
    }
}
