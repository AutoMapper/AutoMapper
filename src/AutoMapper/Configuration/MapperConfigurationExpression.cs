using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper.Features;
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

        public Features<IGlobalFeature> Features { get; } = new Features<IGlobalFeature>();

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

        public void AddProfiles(IEnumerable<Profile> enumerableOfProfiles)
        {
            foreach (var profile in enumerableOfProfiles)
            {
                AddProfile(profile);
            }
        }

        public void AddMaps(IEnumerable<Assembly> assembliesToScan)
            => AddMapsCore(assembliesToScan);

        public void AddMaps(params Assembly[] assembliesToScan)
            => AddMapsCore(assembliesToScan);

        public void AddMaps(IEnumerable<string> assemblyNamesToScan)
            => AddMapsCore(assemblyNamesToScan.Select(Assembly.Load));

        public void AddMaps(params string[] assemblyNamesToScan)
            => AddMaps((IEnumerable<string>)assemblyNamesToScan);

        public void AddMaps(IEnumerable<Type> typesFromAssembliesContainingMappingDefinitions)
            => AddMapsCore(typesFromAssembliesContainingMappingDefinitions.Select(t => t.GetTypeInfo().Assembly));

        public void AddMaps(params Type[] typesFromAssembliesContainingMappingDefinitions)
            => AddMaps((IEnumerable<Type>)typesFromAssembliesContainingMappingDefinitions);

        private void AddMapsCore(IEnumerable<Assembly> assembliesToScan)
        {
            var allTypes = assembliesToScan.Where(a => !a.IsDynamic && a != typeof(NamedProfile).Assembly).SelectMany(a => a.DefinedTypes).ToArray();
            var autoMapAttributeProfile = new NamedProfile(nameof(AutoMapAttribute));

            foreach (var type in allTypes)
            {
                if (typeof(Profile).IsAssignableFrom(type) && !type.IsAbstract)
                {
                    AddProfile(type.AsType());
                }

                foreach (var autoMapAttribute in type.GetCustomAttributes<AutoMapAttribute>())
                {
                    var mappingExpression = (MappingExpression) autoMapAttributeProfile.CreateMap(autoMapAttribute.SourceType, type);
                
                    foreach (var memberInfo in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
                    {
                        foreach (var memberConfigurationProvider in memberInfo.GetCustomAttributes().OfType<IMemberConfigurationProvider>())
                        {
                            mappingExpression.ForMember(memberInfo, cfg => memberConfigurationProvider.ApplyConfiguration(cfg));
                        }
                    }

                    autoMapAttribute.ApplyConfiguration(mappingExpression);
                }
            }

            AddProfile(autoMapAttributeProfile);
        }

        public void ConstructServicesUsing(Func<Type, object> constructor) => ServiceCtor = constructor;
    }
}
