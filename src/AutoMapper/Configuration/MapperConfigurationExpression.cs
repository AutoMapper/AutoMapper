using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper.Mappers;

namespace AutoMapper.Configuration
{
    public class MapperConfigurationExpression : Profile, IMapperConfigurationExpression, IConfiguration
    {
        private readonly IList<Profile> _profiles = new List<Profile>();

        public MapperConfigurationExpression() : base()
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
            public NamedProfile(string profileName) : base(profileName)
            {
            }

            public NamedProfile(string profileName, Action<IProfileExpression> config) : base(profileName, config)
            {
            }
        }

        public void AddProfile(Profile profile)
        {
            _profiles.Add(profile);
        }

        public void AddProfile<TProfile>() where TProfile : Profile, new() => AddProfile(new TProfile());

        public void AddProfile(Type profileType) => AddProfile((Profile)Activator.CreateInstance(profileType));

        public void AddProfiles(IEnumerable<Assembly> assembliesToScan)
            => AddMaps(assembliesToScan);

        public void AddProfiles(params Assembly[] assembliesToScan)
            => AddMaps(assembliesToScan);

        public void AddProfiles(IEnumerable<string> assemblyNamesToScan)
            => AddMaps(assemblyNamesToScan.Select(name => Assembly.Load(new AssemblyName(name))));

        public void AddProfiles(params string[] assemblyNamesToScan)
            => AddMaps(assemblyNamesToScan.Select(name => Assembly.Load(new AssemblyName(name))));

        public void AddProfiles(IEnumerable<Type> typesFromAssembliesContainingProfiles)
            => AddMaps(typesFromAssembliesContainingProfiles.Select(t => t.GetTypeInfo().Assembly));

        public void AddProfiles(params Type[] typesFromAssembliesContainingProfiles)
            => AddMaps(typesFromAssembliesContainingProfiles.Select(t => t.GetTypeInfo().Assembly));

        public void AddMaps(IEnumerable<Assembly> assembliesToScan)
            => AddMapsCore(assembliesToScan);

        public void AddMaps(params Assembly[] assembliesToScan)
            => AddMapsCore(assembliesToScan);

        public void AddMaps(IEnumerable<string> assemblyNamesToScan)
            => AddMapsCore(assemblyNamesToScan.Select(name => Assembly.Load(new AssemblyName(name))));

        public void AddMaps(params string[] assemblyNamesToScan)
            => AddMapsCore(assemblyNamesToScan.Select(name => Assembly.Load(new AssemblyName(name))));

        public void AddMaps(IEnumerable<Type> typesFromAssembliesContainingMappingDefinitions)
            => AddMapsCore(typesFromAssembliesContainingMappingDefinitions.Select(t => t.GetTypeInfo().Assembly));

        public void AddMaps(params Type[] typesFromAssembliesContainingMappingDefinitions)
            => AddMapsCore(typesFromAssembliesContainingMappingDefinitions.Select(t => t.GetTypeInfo().Assembly));

        private void AddMapsCore(IEnumerable<Assembly> assembliesToScan)
        {
            var allTypes = assembliesToScan.Where(a => !a.IsDynamic).SelectMany(a => a.GetDefinedTypes()).ToArray();

            var profiles =
                allTypes
                    .Where(t => typeof(Profile).GetTypeInfo().IsAssignableFrom(t))
                    .Where(t => !t.IsAbstract)
                    .Select(t => t.AsType());

            foreach (var profile in profiles)
            {
                AddProfile(profile);
            }

            var autoMapAttributeProfile = new NamedProfile("AutoMapAttributeProfile");
            foreach (var type in allTypes)
            {
                foreach (var autoMapAttribute in type.GetCustomAttributes<AutoMapAttribute>())
                {
                    autoMapAttributeProfile.CreateMap(autoMapAttribute.SourceType, type);
                }
            }

            AddProfile(autoMapAttributeProfile);
        }

        public void ConstructServicesUsing(Func<Type, object> constructor) => ServiceCtor = constructor;
    }
}