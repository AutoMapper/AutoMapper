namespace AutoMapper.UnitTests.Bug;

public class IncludeBaseInheritance : AutoMapperSpecBase
{
    DestinationLevel2 _destination;

    public class SourceLevel0
    {
        public string SPropertyLevel0 { get; set; }
    }

    public class SourceLevel1 : SourceLevel0
    {
        public string SPropertyLevel1 { get; set; }
    }

    public class SourceLevel2 : SourceLevel1
    {
        public string DPropertyLevel0 { get; set; }
        public string SPropertyLevel2 { get; set; }
    }

    public class DestinationLevel0
    {
        public string DPropertyLevel0 { get; set; }
    }

    public class DestinationLevel1 : DestinationLevel0
    {
        public string DPropertyLevel1 { get; set; }
    }

    public class DestinationLevel2 : DestinationLevel1
    {
        public string DPropertyLevel2 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<SourceLevel0, DestinationLevel0>()
            .ForMember(dest => dest.DPropertyLevel0, o => o.MapFrom(src => src.SPropertyLevel0));

        cfg.CreateMap<SourceLevel1, DestinationLevel1>()
            .IncludeBase<SourceLevel0, DestinationLevel0>()
            .ForMember(dest => dest.DPropertyLevel1, o => o.MapFrom(src => src.SPropertyLevel1));

        cfg.CreateMap<SourceLevel2, DestinationLevel2>()
            .IncludeBase<SourceLevel1, DestinationLevel1>()
            .ForMember(dest => dest.DPropertyLevel2, o => o.MapFrom(src => src.SPropertyLevel2));
    });

    protected override void Because_of()
    {
        var source = new SourceLevel2
        {
            SPropertyLevel0 = "SPropertyLevel0",
            SPropertyLevel1 = "SPropertyLevel1",
            SPropertyLevel2 = "SPropertyLevel2",
            DPropertyLevel0 = "DPropertyLevel0",
        };
        _destination = Mapper.Map<DestinationLevel2>(source);
    }

    [Fact]
    public void Should_inherit_resolvers()
    {
        _destination.DPropertyLevel0.ShouldBe("SPropertyLevel0");
    }
}