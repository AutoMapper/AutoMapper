namespace AutoMapper.UnitTests.Projection;
public class NullSubstitutes : AutoMapperSpecBase
{
    private List<Dest> _dests;

    public class Source
    {
        public int? Value { get; set; }
    }

    public class Dest
    {
        public int? Value { get; set; }            
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Dest>().ForMember(m => m.Value, opt => opt.NullSubstitute(5));
    });

    protected override void Because_of()
    {
        var source = new[] {new Source()}.AsQueryable();

        _dests = source.ProjectTo<Dest>(Configuration).ToList();
    }

    [Fact]
    public void Can_substitute_null_values()
    {
        _dests[0].Value.ShouldBe(5);
    }
}

public class NullSubstitutesWithMapFrom : AutoMapperSpecBase
{
    private List<Dest> _dests;

    public class Source
    {
        public int? Value { get; set; }
    }

    public class Dest
    {
        public int? ValuePropertyNotMatching { get; set; }            
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Dest>().ForMember(m => m.ValuePropertyNotMatching, opt =>
        {
            opt.MapFrom(src => src.Value);
            opt.NullSubstitute(5);
        });
    });

    protected override void Because_of()
    {
        var source = new[] {new Source()}.AsQueryable();

        _dests = source.ProjectTo<Dest>(Configuration).ToList();
    }

    [Fact]
    public void Can_substitute_null_values()
    {
        _dests[0].ValuePropertyNotMatching.ShouldBe(5);
    }
}