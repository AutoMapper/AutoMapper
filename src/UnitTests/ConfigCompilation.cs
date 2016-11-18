namespace AutoMapper.UnitTests
{
    using Should;
    using Xunit;

    public class ConfigCompilation : NonValidatingSpecBase
    {
        public class Source { }
        public class Dest { }
        public class Source2 { }
        public class Dest2 { }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>();
            cfg.CreateMap<Source2, Dest2>();
        });

        [Fact]
        public void Should_compile_mappings()
        {
            Configuration.CompileMappings();

            Mapper.Map<Source, Dest>(new Source()).ShouldNotBeNull();
        }
    }
}