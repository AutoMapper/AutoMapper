using System.Linq;

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

            [Fact]
            public void Should_not_load_internal_profile()
            {
                Configuration.Profiles.Where(t => t.Name == InternalProfile.Name).ShouldBeEmpty();
            }
        }

        internal class InternalProfile : Profile
        {
            public const string Name = "InternalProfile";

            public InternalProfile() : base(Name)
            {
            }
        }

        public class When_scanning_by_assembly_after_setting_internal_profiles_inclusion : NonValidatingSpecBase
        {
            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.Advanced.ScanForInternalProfiles = true;
                cfg.AddProfiles(new[] { typeof(When_scanning_by_assembly_after_setting_internal_profiles_inclusion).Assembly });
            });

            [Fact]
            public void Should_contain_internal_profile()
            {
                Configuration.Profiles.Where(t => t.Name == InternalProfile.Name).ShouldBeOfLength(1);
            }
        }

        public class When_scanning_by_assembly_befor_setting_internal_profiles_inclusion : NonValidatingSpecBase
        {
            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg.AddProfiles(new[] { typeof(When_scanning_by_assembly_befor_setting_internal_profiles_inclusion).Assembly });
                cfg.Advanced.ScanForInternalProfiles = true;
            });

            [Fact]
            public void Should_contain_internal_profile()
            {
                Configuration.Profiles.Where(t => t.Name == InternalProfile.Name).ShouldBeOfLength(1);
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