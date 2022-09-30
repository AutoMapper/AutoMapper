namespace AutoMapper.UnitTests.Bug;

public class NullableIntToNullableDecimal : AutoMapperSpecBase
{
    private Destination _destination;

    class Source
    {
        public int? Number { get; set; }
    }
    class Destination
    {
        public decimal? Number { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        var source = new Source
        {
            Number = 23
        };
        _destination = Mapper.Map<Source, Destination>(source);
    }

    [Fact]
    public void Should_map_int_to_nullable_decimal()
    {
        _destination.Number.ShouldBe(23);
    }
}

public class NullNullableIntToNullableDecimal : AutoMapperSpecBase
{
    private Destination _destination;

    class Source
    {
        public int? Number { get; set; }
    }
    class Destination
    {
        public decimal? Number { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        var source = new Source
        {
        };
        _destination = Mapper.Map<Source, Destination>(source);
    }

    [Fact]
    public void Should_map_int_to_nullable_decimal()
    {
        _destination.Number.ShouldBeNull();
    }
}