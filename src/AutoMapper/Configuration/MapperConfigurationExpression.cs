using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper.Features;
using AutoMapper.Internal;
using AutoMapper.Internal.Mappers;
using AutoMapper.QueryableExtensions.Impl;

namespace AutoMapper.Configuration
{
    using Validator = Action<ValidationContext>;
    public class MapperConfigurationExpression : Profile, IGlobalConfigurationExpression
    {
        private readonly List<Profile> _profiles = new List<Profile>();
        private readonly List<Validator> _validators = new List<Validator>();
        private readonly List<Action<IGlobalConfiguration>> _beforeSealActions = new List<Action<IGlobalConfiguration>>();
        private readonly List<IObjectMapper> _mappers;
        private Func<Type, object> _serviceCtor = Activator.CreateInstance;

        public MapperConfigurationExpression() : base() => _mappers = MapperRegistry.Mappers();

        IEnumerable<Action<IGlobalConfiguration>> IGlobalConfigurationExpression.BeforeSealActions => _beforeSealActions;

        /// <summary>
        /// Add Action called against the IGlobalConfiguration before it gets sealed
        /// </summary>
        void IGlobalConfigurationExpression.BeforeSeal(Action<IGlobalConfiguration> action) => 
            _beforeSealActions.Add(action ?? throw new ArgumentNullException(nameof(action)));

        /// <summary>
        /// Add an action to be called when validating the configuration.
        /// </summary>
        /// <param name="validator">the validation callback</param>
        void IGlobalConfigurationExpression.Validator(Validator validator) => 
            _validators.Add(validator ?? throw new ArgumentNullException(nameof(validator)));

        /// <summary>
        /// Allow the same map to exist in different profiles.
        /// The default is to throw an exception, true means the maps are merged.
        /// </summary>
        bool IGlobalConfigurationExpression.AllowAdditiveTypeMapCreation { get; set; }

        /// <summary>
        /// How many levels deep should AutoMapper try to inline the execution plan for child classes.
        /// See <a href="https://automapper.readthedocs.io/en/latest/Understanding-your-mapping.html">the docs</a> for details.
        /// </summary>
        int IGlobalConfigurationExpression.MaxExecutionPlanDepth { get; set; } = 1;

        Validator[] IGlobalConfigurationExpression.GetValidators() => _validators.ToArray();

        List<IProjectionMapper> IGlobalConfigurationExpression.ProjectionMappers { get; } = ProjectionBuilder.DefaultProjectionMappers();

        /// <summary>
        /// How many levels deep should recursive queries be expanded.
        /// Must be zero for EF6. Can be greater than zero for EF Core.
        /// </summary>
        int IGlobalConfigurationExpression.RecursiveQueriesMaxDepth { get; set; }

        IReadOnlyCollection<IProfileConfiguration> IGlobalConfigurationExpression.Profiles => _profiles;
        Func<Type, object> IGlobalConfigurationExpression.ServiceCtor => _serviceCtor;

        public void CreateProfile(string profileName, Action<IProfileExpression> config)
            => AddProfile(new NamedProfile(profileName, config));

        List<IObjectMapper> IGlobalConfigurationExpression.Mappers => _mappers;

        Features<IGlobalFeature> IGlobalConfigurationExpression.Features { get; } = new Features<IGlobalFeature>();

        private class NamedProfile : Profile
        {
            public NamedProfile(string profileName) : base(profileName)
            {
            }

            public NamedProfile(string profileName, Action<IProfileExpression> config) : base(profileName, config)
            {
            }
        }

        public void AddProfile(Profile profile) => _profiles.Add(profile);

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

        public void ConstructServicesUsing(Func<Type, object> constructor) => _serviceCtor = constructor;
    }
}