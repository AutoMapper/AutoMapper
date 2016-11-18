namespace AutoMapper.UnitTests
{
    using Should;
    using Xunit;

    public class ConfigCompilation : NonValidatingSpecBase
    {
        public class Source { }
        public class Dest { }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>());

        [Fact]
        public void Should_compile_mappings()
        {
            Configuration.CompileMappings();

            Mapper.Map<Source, Dest>(new Source()).ShouldNotBeNull();
        }
    }
}