namespace AutoMapper.UnitTests.Bug;

public class NullToString : AutoMapperSpecBase
{
    private Destination _destination;

    class Source
    {
        public InnerSource Inner { get; set; }
    }
    class InnerSource
    {
    }
    class Destination
    {
        public string Inner { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source());
    }

    [Fact]
    public void Should_map_int_to_nullable_decimal()
    {
        _destination.Inner.ShouldBeNull();
    }
}