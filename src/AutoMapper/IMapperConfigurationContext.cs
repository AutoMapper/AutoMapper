namespace AutoMapper
{
    using System;
    using Mappers;

    /// <summary>
    /// 
    /// </summary>
    public interface IMapperConfigurationContext
    {
        /// <summary>
        /// Gets the context ObjectMappers.
        /// </summary>
        IObjectMapperCollection ObjectMappers { get; }

        /// <summary>
        /// Gets the store for context Configuration.
        /// </summary>
        IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the configuration provider from its context
        /// </summary>
        IConfigurationProvider ConfigurationProvider { get; }

        /// <summary>
        /// Globally ignore all members starting with a prefix.
        /// </summary>
        /// <param name="startingWith">Prefix of members to ignore. Call this before all other maps created.</param>
        void AddGlobalIgnore(string startingWith);

        /// <summary>
        /// Initializes the mapper with the supplied configuration. Runtime optimization complete after this method is called.
        /// This is the preferred means to configure AutoMapper.
        /// </summary>
        /// <param name="action">Initialization callback</param>
        void Initialize(Action<IConfiguration> action);

        /// <summary>
        /// Gets or sets whether to AllowNullDestinationValues. When set, destination can have null values.
        /// Defaults to true. This does not affect simple types, only complex ones.
        /// </summary>
        bool AllowNullDestinationValues { get; set; }

        /// <summary>
        /// Creates a mapping configuration from the <typeparamref name="TSource"/> type to the <typeparamref name="TDestination"/> type
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <returns>Mapping expression for more configuration options</returns>
        IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>();

        /// <summary>
        /// Creates a mapping configuration from the <typeparamref name="TSource"/> type to the <typeparamref name="TDestination"/> type.
        /// Specify the member list to validate against during configuration validation.
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="memberList">Member list to validate</param>
        /// <returns>Mapping expression for more configuration options</returns>
        IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList);

        /// <summary>
        /// Create a mapping configuration from the source type to the destination type.
        /// Use this method when the source and destination type are known at runtime and not compile time.
        /// </summary>
        /// <param name="sourceType">Source type</param>
        /// <param name="destinationType">Destination type</param>
        /// <returns>Mapping expression for more configuration options</returns>
        IMappingExpression CreateMap(Type sourceType, Type destinationType);

        /// <summary>
        /// Creates a mapping configuration from the source type to the destination type.
        /// Specify the member list to validate against during configuration validation.
        /// </summary>
        /// <param name="sourceType">Source type</param>
        /// <param name="destinationType">Destination type</param>
        /// <param name="memberList">Member list to validate</param>
        /// <returns>Mapping expression for more configuration options</returns>
        IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList);

        /// <summary>
        /// Create a named profile for grouped mapping configuration
        /// </summary>
        /// <param name="profileName">Profile name</param>
        /// <returns>Profile configuration options</returns>
        IProfileExpression CreateProfile(string profileName);

        /// <summary>
        /// Create a named profile for grouped mapping configuration, and configure the profile
        /// </summary>
        /// <param name="profileName">Profile name</param>
        /// <param name="profileConfiguration">Profile configuration callback</param>
        void CreateProfile(string profileName, Action<IProfileExpression> profileConfiguration);

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
        /// Find the <see cref="TypeMap"/> for the configured source and destination type
        /// </summary>
        /// <param name="sourceType">Configured source type</param>
        /// <param name="destinationType">Configured destination type</param>
        /// <returns>Type map configuration</returns>
        TypeMap FindTypeMapFor(Type sourceType, Type destinationType);

        /// <summary>
        /// Find the <see cref="TypeMap"/> for the configured source and destination type
        /// </summary>
        /// <typeparam name="TSource">Configured source type</typeparam>
        /// <typeparam name="TDestination">Configured destination type</typeparam>
        /// <returns>Type map configuration</returns>
        TypeMap FindTypeMapFor<TSource, TDestination>();

        /// <summary>
        /// Get all configured type maps created
        /// </summary>
        /// <returns>All configured type maps</returns>
        TypeMap[] GetAllTypeMaps();

        /// <summary>
        /// Dry run all configured type maps and throw <see cref="AutoMapperConfigurationException"/> for each problem
        /// </summary>
        void AssertConfigurationIsValid();

        /// <summary>
        /// Dry run single type map
        /// </summary>
        /// <param name="typeMap">Type map to check</param>
        void AssertConfigurationIsValid(TypeMap typeMap);

        /// <summary>
        /// Dry run all type maps in given profile
        /// </summary>
        /// <param name="profileName">Profile name of type maps to test</param>
        void AssertConfigurationIsValid(string profileName);

        /// <summary>
        /// Dry run all type maps in given profile.
        /// </summary>
        /// <typeparam name="TProfile">Profile type</typeparam>
        void AssertConfigurationIsValid<TProfile>()
            where TProfile : Profile, new();
    }
}
