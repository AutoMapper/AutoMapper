using System.Linq;

namespace AutoMapper.UnitTests
{
    namespace AssemblyScanning
    {
        using Shouldly;
        using Xunit;

        public class When_scanning_by_assembly : NonValidatingSpecBase
        {
            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.AddProfiles(new[] { typeof(When_scanning_by_assembly).Assembly() });
            });

            [Fact]
            public void Should_load_profiles()
            {
                Configuration.GetAllTypeMaps().Length.ShouldBeGreaterThan(0);
            }

            [Fact]
            public void Should_load_internal_profiles()
            {
                Configuration.Profiles.Where(t => t.Name == InternalProfile.Name).ShouldNotBeEmpty();
            }
        }

        internal class InternalProfile : Profile
        {
            public const string Name = "InternalProfile";

            public InternalProfile() : base(Name)
            {
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
                cfg.AddProfiles(new[] { typeof(When_scanning_by_name).Assembly().FullName });
            });

            [Fact]
            public void Should_load_profiles()
            {
                Configuration.GetAllTypeMaps().Length.ShouldBeGreaterThan(0);
            }
        }
    }
}