using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper.Configuration;
using AutoMapper.Features;
using AutoMapper.Internal;
using AutoMapper.Internal.Mappers;
using AutoMapper.QueryableExtensions.Impl;
namespace AutoMapper;

using Validator = Action<ValidationContext>;
public interface IMapperConfigurationExpression : IProfileExpression
{
    /// <summary>
    /// Add an existing profile
    /// </summary>
    /// <param name="profile">Profile to add</param>
    void AddProfile(Profile profile);

    /// <summary>
    /// Add an existing profile type. Profile will be instantiated and added to the configuration.
    /// </summary>
    /// <typeparam name="TProfile">Profile type</typeparam>
    void AddProfile<TProfile>() where TProfile : Profile, new();

    /// <summary>
    /// Add an existing profile type. Profile will be instantiated and added to the configuration.
    /// </summary>
    /// <param name="profileType">Profile type</param>
    void AddProfile(Type profileType);

    /// <summary>
    /// Add profiles contained in an IEnumerable
    /// </summary>
    /// <param name="enumerableOfProfiles">IEnumerable of Profile</param>
    void AddProfiles(IEnumerable<Profile> enumerableOfProfiles);

    /// <summary>
    /// Add mapping definitions contained in assemblies.
    /// Looks for <see cref="Profile" /> definitions and classes decorated with <see cref="AutoMapAttribute" />
    /// </summary>
    /// <param name="assembliesToScan">Assemblies containing mapping definitions</param>
    void AddMaps(IEnumerable<Assembly> assembliesToScan);

    /// <summary>
    /// Add mapping definitions contained in assemblies.
    /// Looks for <see cref="Profile" /> definitions and classes decorated with <see cref="AutoMapAttribute" />
    /// </summary>
    /// <param name="assembliesToScan">Assemblies containing mapping definitions</param>
    void AddMaps(params Assembly[] assembliesToScan);

    /// <summary>
    /// Add mapping definitions contained in assemblies.
    /// Looks for <see cref="Profile" /> definitions and classes decorated with <see cref="AutoMapAttribute" />
    /// </summary>
    /// <param name="assemblyNamesToScan">Assembly names to load and scan containing mapping definitions</param>
    void AddMaps(IEnumerable<string> assemblyNamesToScan);

    /// <summary>
    /// Add mapping definitions contained in assemblies.
    /// Looks for <see cref="Profile" /> definitions and classes decorated with <see cref="AutoMapAttribute" />
    /// </summary>
    /// <param name="assemblyNamesToScan">Assembly names to load and scan containing mapping definitions</param>
    void AddMaps(params string[] assemblyNamesToScan);

    /// <summary>
    /// Add mapping definitions contained in assemblies.
    /// Looks for <see cref="Profile" /> definitions and classes decorated with <see cref="AutoMapAttribute" />
    /// </summary>
    /// <param name="typesFromAssembliesContainingMappingDefinitions">Types from assemblies containing mapping definitions</param>
    void AddMaps(IEnumerable<Type> typesFromAssembliesContainingMappingDefinitions);

    /// <summary>
    /// Add mapping definitions contained in assemblies.
    /// Looks for <see cref="Profile" /> definitions and classes decorated with <see cref="AutoMapAttribute" />
    /// </summary>
    /// <param name="typesFromAssembliesContainingMappingDefinitions">Types from assemblies containing mapping definitions</param>
    void AddMaps(params Type[] typesFromAssembliesContainingMappingDefinitions);

    /// <summary>
    /// Supply a factory method callback for creating resolvers and type converters
    /// </summary>
    /// <param name="constructor">Factory method</param>
    void ConstructServicesUsing(Func<Type, object> constructor);

    /// <summary>
    /// Create a named profile with the supplied configuration
    /// </summary>
    /// <param name="profileName">Profile name, must be unique</param>
    /// <param name="config">Profile configuration</param>
    void CreateProfile(string profileName, Action<IProfileExpression> config);
}
public class MapperConfigurationExpression : Profile, IGlobalConfigurationExpression
{
    private readonly List<Profile> _profiles = new();
    private readonly List<Validator> _validators = new();
    private readonly List<IObjectMapper> _mappers;
    private Func<Type, object> _serviceCtor = Activator.CreateInstance;

    public MapperConfigurationExpression() : base() => _mappers = MapperRegistry.Mappers();

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

    List<Validator> IGlobalConfigurationExpression.Validators => _validators;

    List<IProjectionMapper> IGlobalConfigurationExpression.ProjectionMappers { get; } = ProjectionBuilder.DefaultProjectionMappers();

    /// <summary>
    /// How many levels deep should recursive queries be expanded.
    /// Must be zero for EF6. Can be greater than zero for EF Core.
    /// </summary>
    int IGlobalConfigurationExpression.RecursiveQueriesMaxDepth { get; set; }

    IReadOnlyCollection<IProfileConfiguration> IGlobalConfigurationExpression.Profiles => _profiles;
    Func<Type, object> IGlobalConfigurationExpression.ServiceCtor => _serviceCtor;

    public void CreateProfile(string profileName, Action<IProfileExpression> config)
        => AddProfile(new Profile(profileName, config));

    List<IObjectMapper> IGlobalConfigurationExpression.Mappers => _mappers;

    Features<IGlobalFeature> IGlobalConfigurationExpression.Features { get; } = new Features<IGlobalFeature>();

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
        var allTypes = assembliesToScan.Where(a => !a.IsDynamic && a != typeof(Profile).Assembly).SelectMany(a => a.DefinedTypes).ToArray();
        var autoMapAttributeProfile = new Profile(nameof(AutoMapAttribute));

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