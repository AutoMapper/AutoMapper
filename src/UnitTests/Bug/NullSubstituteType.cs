namespace AutoMapper.UnitTests.Bug;

public class NullSubstituteType : AutoMapperSpecBase
{
    private Destination _destination;

    class Source
    {
        public long? Number { get; set; }
    }
    class Destination
    {
        public long? Number { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().ForMember(d => d.Number, o => o.NullSubstitute(0));
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source());
    }

    [Fact]
    public void Should_substitute_zero_for_null()
    {
        _destination.Number.ShouldBe(0);
    }
}