namespace AutoMapper.UnitTests.Bug;

public class NullableUntypedMapFrom : AutoMapperSpecBase
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
        cfg.CreateMap<Source, Destination>().ForMember(d => d.OddNumber, o => o.MapFrom(s => (object)s.Number));
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Destination>(new Source { Number = 12 });
    }

    [Fact]
    public void Should_map_nullable_decimal()
    {
        _destination.OddNumber.ShouldBe(12);
    }
}