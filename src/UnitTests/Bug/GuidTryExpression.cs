namespace AutoMapper.UnitTests.Bug;

public class GuidTryExpression : AutoMapperSpecBase
{
    private Destination _destination;
    private Guid _value = Guid.NewGuid();

    class Source
    {
        public Guid Value { get; set; }
    }
    class Destination
    {
        public string Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().ForMember(d => d.Value, o => o.MapFrom(s => s.Value));
    });

    protected override void Because_of()
    {
        var source = new Source
        {
            Value = _value
        };
        _destination = Mapper.Map<Source, Destination>(source);
    }

    [Fact]
    public void Should_map_int_to_nullable_decimal()
    {
        _destination.Value.ShouldBe(_value.ToString());
    }
}