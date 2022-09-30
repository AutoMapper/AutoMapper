namespace AutoMapper.UnitTests;
public class ConfigCompilation : NonValidatingSpecBase
{
    public class Source { }
    public class Dest { }
    public class Source2 { }
    public class Dest2 { }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Dest>();
        cfg.CreateMap<Source2, Dest2>();
        cfg.CreateMap(typeof(IEnumerable<>), typeof(IEnumerable<>)).ConvertUsing(s => s);
    });

    [Fact]
    public void Should_compile_mappings()
    {
        Configuration.CompileMappings();

        Mapper.Map<Source, Dest>(new Source()).ShouldNotBeNull();
    }
}