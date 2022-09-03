namespace AutoMapper.UnitTests.Bug;

public class DeepCloningBug : AutoMapperSpecBase
{
    private Outer _source;
    private Outer _dest;

    public class Outer
    {
        public Inner Foo { get; set; }
    }

    public class Inner
    {

    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Outer, Outer>();
        cfg.CreateMap<Inner, Inner>();
    });

    protected override void Because_of()
    {
        _source = new Outer { Foo = new Inner() };
        _dest = Mapper.Map<Outer>(_source);
    }

    [Fact]
    public void Should_map_new_top_object()
    {
        _dest.ShouldNotBeSameAs(_source);
    }

    [Fact]
    public void Should_map_new_second_level_object()
    {
        _dest.Foo.ShouldNotBeSameAs(_source.Foo);
    }
}