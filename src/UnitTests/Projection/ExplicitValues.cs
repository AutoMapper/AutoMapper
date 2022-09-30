namespace AutoMapper.UnitTests.Projection;
public class ExplicitValues : AutoMapperSpecBase
{
    private List<Dest> _dests;

    public class Source
    {
        public int Value { get; set; }
    }

    public class Dest
    {
        public int Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Dest>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => 5));
    });

    protected override void Because_of()
    {
        var source = new[] { new Source { Value = 10 } }.AsQueryable();

        _dests = source.ProjectTo<Dest>(Configuration).ToList();
    }

    [Fact]
    public void Should_substitute_value()
    {
        _dests[0].Value.ShouldBe(5);
    }
}