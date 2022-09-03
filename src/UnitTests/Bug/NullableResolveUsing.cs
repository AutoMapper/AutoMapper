namespace AutoMapper.UnitTests.Bug;

public class NullableResolveUsing : AutoMapperSpecBase
{
    private Destination _destination;

    class Source
    {
        public decimal? Number { get; set; }
    }
    class Destination
    {
        public decimal? OddNumber { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().ForMember(d => d.OddNumber, o => o.MapFrom(s => s.Number));
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Destination>(new Source());
    }

    [Fact]
    public void Should_map_nullable_decimal_with_ResolveUsing()
    {
        _destination.OddNumber.ShouldBeNull();
    }
}