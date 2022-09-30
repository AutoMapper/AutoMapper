namespace AutoMapper.UnitTests.Bug;

public class NullSubstituteInnerClass : AutoMapperSpecBase
{
    private FooDto _destination;

    public class Foo
    {
        public int Id { get; set; }
        public Bar Bar { get; set; }
    }

    public class Bar
    {
        public string Name { get; set; }
    }


    public class FooDto
    {
        public int Id { get; set; }
        public BarDto Bar { get; set; }
    }

    public class BarDto
    {
        public string Name { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Bar, BarDto>();
        cfg.CreateMap<Foo, FooDto>()
            .ForMember(dest => dest.Bar, opts => opts.NullSubstitute(new Bar()));
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Foo, FooDto>(new Foo()
        {
            Id = 5,
            Bar = null
        });
    }

    [Fact]
    public void Should_map_int_to_nullable_decimal()
    {
        _destination.Bar.ShouldNotBeNull();
    }
}