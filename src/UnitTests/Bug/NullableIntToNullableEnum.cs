namespace AutoMapper.UnitTests.Bug;

public class NullableIntToNullableEnum : AutoMapperSpecBase
{
    Destination _destination;

    public enum Values
    {
        One = 1,
        Two = 2,
        Three = 3
    }

    public class Source
    {
        public int? Value { get; set; }
    }

    public class Destination
    {
        public Values? Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Destination>(new Source());
    }

    [Fact]
    public void Should_map_null_to_null()
    {
        _destination.Value.ShouldBeNull();
    }
}