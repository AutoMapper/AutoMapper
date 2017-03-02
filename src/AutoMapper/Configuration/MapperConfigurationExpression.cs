namespace AutoMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Mappers;

    public class MapperConfigurationExpression : Profile, IMapperConfigurationExpression, IConfiguration
    {
        private readonly IList<Profile> _profiles = new List<Profile>();

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
        {
            profile.Initialize();
            _profiles.Add(profile);
        }

        public void AddProfile<TProfile>() where TProfile : Profile, new() => AddProfile(new TProfile());

        public void AddProfile(Type profileType) => AddProfile((Profile)Activator.CreateInstance(profileType));

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
        {
            var allTypes = assembliesToScan.Where(a => !a.IsDynamic).SelectMany(a => a.ExportedTypes).ToArray();

            var profiles =
                allTypes
                    .Where(t => typeof(Profile).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
                    .Where(t => !t.GetTypeInfo().IsAbstract);

            foreach (var profile in profiles)
            {
                AddProfile(profile);
            }

        }


        public void ConstructServicesUsing(Func<Type, object> constructor) => ServiceCtor = constructor;
    }
}