namespace AutoMapper.UnitTests
{
    namespace AssemblyScanning
    {
        using Should;
        using Xunit;

        public class When_scanning_by_assembly : NonValidatingSpecBase
        {
            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.AddProfiles(new[] { typeof(When_scanning_by_assembly).Assembly });
            });

            [Fact]
            public void Should_load_profiles()
            {
                Configuration.GetAllTypeMaps().Length.ShouldBeGreaterThan(0);
            }
        }

        public class When_scanning_by_type : NonValidatingSpecBase
        {
            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.AddProfiles(new[] { typeof(When_scanning_by_assembly) });
            });

            [Fact]
            public void Should_load_profiles()
            {
                Configuration.GetAllTypeMaps().Length.ShouldBeGreaterThan(0);
            }
        }

        public class When_scanning_by_name : NonValidatingSpecBase
        {
            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.AddProfiles(new[] { "AutoMapper.UnitTests.Net4" });
            });

            [Fact]
            public void Should_load_profiles()
            {
                Configuration.GetAllTypeMaps().Length.ShouldBeGreaterThan(0);
            }
        }
    }
}