namespace AutoMapper.UnitTests.MappingInheritance;
public class InheritanceWithoutIncludeShouldWork : AutoMapperSpecBase
{
    public class FooBase { }
    public class Foo : FooBase { }
    public class FooDto { public int Value { get; set; } }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<FooBase, FooDto>().ForMember(d => d.Value, opt => opt.MapFrom(src => 10));
        cfg.CreateMap<Foo, FooDto>().ForMember(d => d.Value, opt => opt.MapFrom(src => 5));
    });

    [Fact]
    public void Should_map_derived()
    {
        Map(new Foo()).Value.ShouldBe(5);
    }

    [Fact]
    public void Should_map_base()
    {
        Map(new FooBase()).Value.ShouldBe(10);
    }

    private FooDto Map(FooBase foo)
    {
        return Mapper.Map<FooBase, FooDto>(foo);
    }
}