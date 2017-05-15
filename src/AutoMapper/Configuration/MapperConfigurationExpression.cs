using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper.Mappers;

namespace AutoMapper.Configuration
{
    public class MapperConfigurationExpression : Profile, IMapperConfigurationExpression, IConfiguration
    {
        private IEnumerable<Profile> _profiles = Enumerable.Empty<Profile>();

        public MapperConfigurationExpression() : base("")
        {
            IncludeSourceExtensionMethods(typeof(Enumerable));

            Mappers = MapperRegistry.Mappers();
        }

        public IEnumerable<IProfileConfiguration> Profiles => _profiles;
        public Func<Type, object> ServiceCtor { get; private set; } = Activator.CreateInstance;

        public void CreateProfile(string profileName, Action<IProfileExpression> config)
            => AddProfile(new NamedProfile(profileName, config));

        public IList<IObjectMapper> Mappers { get; }

        public AdvancedConfiguration Advanced { get; } = new AdvancedConfiguration();

        private class NamedProfile : Profile
        {
            public NamedProfile(string profileName, Action<IProfileExpression> config) : base(profileName, config)
            {
            }
        }

        public void AddProfile(Profile profile)
            => _profiles = _profiles.Concat(new[] { profile });

        public void AddProfile<TProfile>() where TProfile : Profile, new()
            => _profiles = _profiles.Concat(new[] {new TProfile()});

        public void AddProfile(Type profileType)
            => _profiles = _profiles.Concat(new[] {(Profile) Activator.CreateInstance(profileType)});

        public void AddProfiles(IEnumerable<Assembly> assembliesToScan)
            => AddProfilesCore(assembliesToScan);

        public void AddProfiles(params Assembly[] assembliesToScan)
            => AddProfilesCore(assembliesToScan);

        public void AddProfiles(IEnumerable<string> assemblyNamesToScan)
            => AddProfilesCore(assemblyNamesToScan.Select(name => Assembly.Load(new AssemblyName(name))));

        public void AddProfiles(params string[] assemblyNamesToScan)
            => AddProfilesCore(assemblyNamesToScan.Select(name => Assembly.Load(new AssemblyName(name))));

        public void AddProfiles(IEnumerable<Type> typesFromAssembliesContainingProfiles)
            => AddProfilesCore(typesFromAssembliesContainingProfiles.Select(t => t.GetTypeInfo().Assembly));

        public void AddProfiles(params Type[] typesFromAssembliesContainingProfiles)
            => AddProfilesCore(typesFromAssembliesContainingProfiles.Select(t => t.GetTypeInfo().Assembly));

        private void AddProfilesCore(IEnumerable<Assembly> assembliesToScan)
            => _profiles = _profiles.Concat(new ProfileScannerIterator(assembliesToScan, Advanced));

        public void ConstructServicesUsing(Func<Type, object> constructor) => ServiceCtor = constructor;

        private class ProfileScannerIterator : IEnumerable<Profile>
        {
            private readonly IEnumerable<Assembly> _assembliesToScan;
            private readonly AdvancedConfiguration _config;

            public ProfileScannerIterator(IEnumerable<Assembly> assembliesToScan, AdvancedConfiguration config)
            {
                _assembliesToScan = assembliesToScan;
                _config = config;
            }

            private IEnumerable<Profile> GetEnumerableProfilesIncludingInternals()
            {
                return from assembly in _assembliesToScan
                       where !assembly.IsDynamic
                       from typeInfo in assembly.DefinedTypes
                       where typeof(Profile).GetTypeInfo().IsAssignableFrom(typeInfo) && !typeInfo.IsAbstract
                       select (Profile) Activator.CreateInstance(typeInfo.AsType());
            }
            private IEnumerable<Profile> GetEnumerableProfiles()
            {
                return from assembly in _assembliesToScan
                       where !assembly.IsDynamic
                       from type in assembly.ExportedTypes.Select(t => t.GetTypeInfo())
                       where typeof(Profile).GetTypeInfo().IsAssignableFrom(type) && !type.IsAbstract
                       select(Profile) Activator.CreateInstance(type.AsType());
            }

            public IEnumerator<Profile> GetEnumerator()
            {
                var profiles = _config.ScanForInternalProfiles
                    ? GetEnumerableProfilesIncludingInternals()
                    : GetEnumerableProfiles();

                return profiles.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}