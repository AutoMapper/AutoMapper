namespace AutoMapper.UnitTests
{
    namespace AssemblyScanning
    {
        public class When_scanning_by_assembly : NonValidatingSpecBase
        {
            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(new[] { typeof(When_scanning_by_assembly).Assembly, typeof(Mapper).Assembly });
            });

            [Fact]
            public void Should_load_profiles()
            {
                Configuration.GetAllTypeMaps().Count.ShouldBeGreaterThan(0);
            }

            [Fact]
            public void Should_load_internal_profiles() => GetProfiles().Where(t => t.Name == InternalProfile.Name).ShouldNotBeEmpty();
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
            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.AddMaps(new[] { typeof(When_scanning_by_assembly) });
            });

            [Fact]
            public void Should_load_profiles()
            {
                Configuration.GetAllTypeMaps().Count.ShouldBeGreaterThan(0);
            }
        }

        public class When_scanning_by_name : NonValidatingSpecBase
        {
            private static readonly Assembly AutoMapperAssembly = typeof(When_scanning_by_name).Assembly;

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
                cfg.AddMaps(new[] { AutoMapperAssembly.FullName });
                AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
            });

            private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args) => args.Name == AutoMapperAssembly.FullName ? AutoMapperAssembly : null;

            [Fact]
            public void Should_load_profiles()
            {
                Configuration.GetAllTypeMaps().Count.ShouldBeGreaterThan(0);
            }
        }
    }
}