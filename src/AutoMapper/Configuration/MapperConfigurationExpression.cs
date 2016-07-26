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
            _profiles.Add(this);
        }

        public IEnumerable<Profile> Profiles => _profiles;
        public Func<Type, object> ServiceCtor { get; private set; } = ObjectCreator.CreateObject;

        public void CreateProfile(string profileName, Action<Profile> config)
        {
            var profile = new NamedProfile(profileName);

            config(profile);

            AddProfile(profile);
        }

        private class NamedProfile : Profile
        {
            public NamedProfile(string profileName) : base(profileName)
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
        {
            var allTypes = assembliesToScan.SelectMany(a => a.ExportedTypes).ToArray();

            var profiles =
                allTypes
                    .Where(t => typeof(Profile).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
                    .Where(t => !t.GetTypeInfo().IsAbstract);

            foreach (var profile in profiles)
            {
                AddProfile(profile);
            }
        }

        public void AddProfiles(IEnumerable<string> assemblyNamesToScan)
            => AddProfiles(assemblyNamesToScan.Select(name => Assembly.Load(new AssemblyName(name))));

        public void AddProfiles(IEnumerable<Type> typesFromAssembliesContainingProfiles) 
            => AddProfiles(typesFromAssembliesContainingProfiles.Select(t => t.GetTypeInfo().Assembly));

        public void ConstructServicesUsing(Func<Type, object> constructor) => ServiceCtor = constructor;
    }
}