namespace AutoMapper
{
    using System;
    using Internal;

    /// <summary>
    /// Main entry point for AutoMapper, for both creating maps and performing maps.
    /// </summary>
    public static class Mapper
    {
        /// <summary>
        /// Gets the context factory.
        /// </summary>
        private static IMapperContextFactory ContextFactory { get; }
            = PlatformAdapter.Resolve<IMapperContextFactory>();

        /// <summary>
        /// Gets the default Mapper Context.
        /// </summary>
        public static IMapperContext Context { get; }
            = ContextFactory.CreateMapperContext();

        /// <summary>
        /// When set, destination can have null values. Defaults to true.
        /// This does not affect simple types, only complex ones.
        /// </summary>
        public static bool AllowNullDestinationValues
        {
            get { return Context.AllowNullDestinationValues; }
            set { Context.AllowNullDestinationValues = value; }
        }

        /// <summary>
        /// Execute a mapping from the source object to a new destination object.
        /// The source type is inferred from the source object.
        /// </summary>
        /// <typeparam name="TDestination">Destination type to create</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <returns>Mapped destination object</returns>
        public static TDestination Map<TDestination>(object source)
        {
            return Context.Map<TDestination>(source);
        }

        /// <summary>
        /// Execute a mapping from the source object to a new destination object with supplied mapping options.
        /// </summary>
        /// <typeparam name="TDestination">Destination type to create</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>Mapped destination object</returns>
        public static TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> opts)
        {
            return Context.Map<TDestination>(source, opts);
        }

        /// <summary>
        /// Execute a mapping from the source object to a new destination object.
        /// </summary>
        /// <typeparam name="TSource">Source type to use, regardless of the runtime type</typeparam>
        /// <typeparam name="TDestination">Destination type to create</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <returns>Mapped destination object</returns>
        public static TDestination Map<TSource, TDestination>(TSource source)
        {
            return Context.Map<TSource, TDestination>(source);
        }

        /// <summary>
        /// Execute a mapping from the source object to the existing destination object.
        /// </summary>
        /// <typeparam name="TSource">Source type to use</typeparam>
        /// <typeparam name="TDestination">Dsetination type</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination">Destination object to map into</param>
        /// <returns>The mapped destination object, same instance as the <paramref name="destination"/> object</returns>
        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return Context.Map(source, destination);
        }

        /// <summary>
        /// Execute a mapping from the source object to the existing destination object with supplied mapping options.
        /// </summary>
        /// <typeparam name="TSource">Source type to use</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination">Destination object to map into</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>The mapped destination object, same instance as the <paramref name="destination"/> object</returns>
        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination,
            Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            return Context.Map(source, destination, opts);
        }

        /// <summary>
        /// Execute a mapping from the source object to a new destination object with supplied mapping options.
        /// </summary>
        /// <typeparam name="TSource">Source type to use</typeparam>
        /// <typeparam name="TDestination">Destination type to create</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>Mapped destination object</returns>
        public static TDestination Map<TSource, TDestination>(TSource source,
            Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            return Context.Map(source, opts);
        }

        /// <summary>
        /// Execute a mapping from the source object to a new destination object with explicit <see cref="System.Type"/> objects
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to create</param>
        /// <returns>Mapped destination object</returns>
        public static object Map(object source, Type sourceType, Type destinationType)
        {
            return Context.Map(source, sourceType, destinationType);
        }

        /// <summary>
        /// Execute a mapping from the source object to a new destination object with explicit <see cref="System.Type"/> objects and supplied mapping options.
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to create</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>Mapped destination object</returns>
        public static object Map(object source, Type sourceType, Type destinationType,
            Action<IMappingOperationOptions> opts)
        {
            return Context.Map(source, sourceType, destinationType, opts);
        }

        /// <summary>
        /// Execute a mapping from the source object to existing destination object with explicit <see cref="System.Type"/> objects
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination">Destination object to map into</param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to use</param>
        /// <returns>Mapped destination object, same instance as the <paramref name="destination"/> object</returns>
        public static object Map(object source, object destination, Type sourceType, Type destinationType)
        {
            return Context.Map(source, destination, sourceType, destinationType);
        }

        /// <summary>
        /// Execute a mapping from the source object to existing destination object with supplied mapping options and explicit <see cref="System.Type"/> objects
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination">Destination object to map into</param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to use</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>Mapped destination object, same instance as the <paramref name="destination"/> object</returns>
        public static object Map(object source, object destination, Type sourceType, Type destinationType,
            Action<IMappingOperationOptions> opts)
        {
            return Context.Map(source, destination, sourceType, destinationType, opts);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="parentContext"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TDestination Map<TSource, TDestination>(ResolutionContext parentContext, TSource source)
        {
            return Context.Runner.Map<TSource, TDestination>(parentContext, source);
        }

        /// <summary>
        /// Create a map between the <typeparamref name="TSource"/> and <typeparamref name="TDestination"/> types and execute the map
        /// </summary>
        /// <typeparam name="TSource">Source type to use</typeparam>
        /// <typeparam name="TDestination">Destination type to use</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <returns>Mapped destination object</returns>
        public static TDestination DynamicMap<TSource, TDestination>(TSource source)
        {
            return Context.DynamicMap<TSource, TDestination>(source);
        }

        /// <summary>
        /// Create a map between the <typeparamref name="TSource"/> and <typeparamref name="TDestination"/> types and execute the map to the existing destination object
        /// </summary>
        /// <typeparam name="TSource">Source type to use</typeparam>
        /// <typeparam name="TDestination">Destination type to use</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination">Destination object to map into</param>
        public static void DynamicMap<TSource, TDestination>(TSource source, TDestination destination)
        {
            Context.DynamicMap(source, destination);
        }

        /// <summary>
        /// Create a map between the <paramref name="source"/> object and <typeparamref name="TDestination"/> types and execute the map.
        /// Source type is inferred from the source object .
        /// </summary>
        /// <typeparam name="TDestination">Destination type to use</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <returns>Mapped destination object</returns>
        public static TDestination DynamicMap<TDestination>(object source)
        {
            return Context.DynamicMap<TDestination>(source);
        }

        /// <summary>
        /// Create a map between the <paramref name="sourceType"/> and <paramref name="destinationType"/> types and execute the map.
        /// Use this method when the source and destination types are not known until runtime.
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to use</param>
        /// <returns>Mapped destination object</returns>
        public static object DynamicMap(object source, Type sourceType, Type destinationType)
        {
            return Context.DynamicMap(source, sourceType, destinationType);
        }

        /// <summary>
        /// Create a map between the <paramref name="sourceType"/> and <paramref name="destinationType"/> types and execute the map to the existing destination object.
        /// Use this method when the source and destination types are not known until runtime.
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination"></param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to use</param>
        public static void DynamicMap(object source, object destination, Type sourceType, Type destinationType)
        {
            Context.DynamicMap(source, destination, sourceType, destinationType);
        }

        //TODO: I'm not positive but the interface here could be cleaned up a TON: Initialize seems redundant with what can be done just besides, i.e. CreateMap: it'll all be Context oriented anyway...
        //TODO: thinking to narrow the interface significantly: maybe obsolete the methods like CreateMap, etc: and just encourage Initialize: better still Context itself
        /// <summary>
        /// Initializes the mapper with the supplied configuration. Runtime optimization complete after this method is called.
        /// This is the preferred means to configure AutoMapper.
        /// </summary>
        /// <param name="action">Initialization callback</param>
        [Obsolete]
        public static void Initialize(Action<IConfiguration> action)
        {
            Context.Initialize(action);
        }

        /// <summary>
        /// Creates a mapping configuration from the <typeparamref name="TSource"/> type to the <typeparamref name="TDestination"/> type
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <returns>Mapping expression for more configuration options</returns>
        public static IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            return Context.CreateMap<TSource, TDestination>();
        }

        /// <summary>
        /// Creates a mapping configuration from the <typeparamref name="TSource"/> type to the <typeparamref name="TDestination"/> type.
        /// Specify the member list to validate against during configuration validation.
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="memberList">Member list to validate</param>
        /// <returns>Mapping expression for more configuration options</returns>
        public static IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList)
        {
            return Context.CreateMap<TSource, TDestination>(memberList);
        }

        /// <summary>
        /// Create a mapping configuration from the source type to the destination type.
        /// Use this method when the source and destination type are known at runtime and not compile time.
        /// </summary>
        /// <param name="sourceType">Source type</param>
        /// <param name="destinationType">Destination type</param>
        /// <returns>Mapping expression for more configuration options</returns>
        public static IMappingExpression CreateMap(Type sourceType, Type destinationType)
        {
            return Context.CreateMap(sourceType, destinationType);
        }

        /// <summary>
        /// Creates a mapping configuration from the source type to the destination type.
        /// Specify the member list to validate against during configuration validation.
        /// </summary>
        /// <param name="sourceType">Source type</param>
        /// <param name="destinationType">Destination type</param>
        /// <param name="memberList">Member list to validate</param>
        /// <returns>Mapping expression for more configuration options</returns>
        public static IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList)
        {
            return Context.CreateMap(sourceType, destinationType, memberList);
        }

        /// <summary>
        /// Create a named profile for grouped mapping configuration
        /// </summary>
        /// <param name="profileName">Profile name</param>
        /// <returns>Profile configuration options</returns>
        public static IProfileExpression CreateProfile(string profileName)
        {
            return Context.CreateProfile(profileName);
        }

        /// <summary>
        /// Create a named profile for grouped mapping configuration, and configure the profile
        /// </summary>
        /// <param name="profileName">Profile name</param>
        /// <param name="profileConfiguration">Profile configuration callback</param>
        public static void CreateProfile(string profileName, Action<IProfileExpression> profileConfiguration)
        {
            Context.CreateProfile(profileName, profileConfiguration);
        }

        /// <summary>
        /// Add an existing profile
        /// </summary>
        /// <param name="profile">Profile to add</param>
        public static void AddProfile(Profile profile)
        {
            Context.AddProfile(profile);
        }

        /// <summary>
        /// Add an existing profile type. Profile will be instantiated and added to the configuration.
        /// </summary>
        /// <typeparam name="TProfile">Profile type</typeparam>
        public static void AddProfile<TProfile>() where TProfile : Profile, new()
        {
            Context.AddProfile<TProfile>();
        }

        /// <summary>
        /// Find the <see cref="TypeMap"/> for the configured source and destination type
        /// </summary>
        /// <param name="sourceType">Configured source type</param>
        /// <param name="destinationType">Configured destination type</param>
        /// <returns>Type map configuration</returns>
        public static TypeMap FindTypeMapFor(Type sourceType, Type destinationType)
        {
            return Context.FindTypeMapFor(sourceType, destinationType);
        }

        /// <summary>
        /// Find the <see cref="TypeMap"/> for the configured source and destination type
        /// </summary>
        /// <typeparam name="TSource">Configured source type</typeparam>
        /// <typeparam name="TDestination">Configured destination type</typeparam>
        /// <returns>Type map configuration</returns>
        public static TypeMap FindTypeMapFor<TSource, TDestination>()
        {
            return Context.FindTypeMapFor(typeof (TSource), typeof (TDestination));
        }

        /// <summary>
        /// Get all configured type maps created
        /// </summary>
        /// <returns>All configured type maps</returns>
        public static TypeMap[] GetAllTypeMaps()
        {
            return Context.GetAllTypeMaps();
        }

        /// <summary>
        /// Dry run all configured type maps and throw <see cref="AutoMapperConfigurationException"/> for each problem
        /// </summary>
        public static void AssertConfigurationIsValid()
        {
            Context.AssertConfigurationIsValid();
        }

        /// <summary>
        /// Dry run single type map
        /// </summary>
        /// <param name="typeMap">Type map to check</param>
        public static void AssertConfigurationIsValid(TypeMap typeMap)
        {
            Context.AssertConfigurationIsValid(typeMap);
        }

        /// <summary>
        /// Dry run all type maps in given profile
        /// </summary>
        /// <param name="profileName">Profile name of type maps to test</param>
        public static void AssertConfigurationIsValid(string profileName)
        {
            Context.AssertConfigurationIsValid(profileName);
        }

        /// <summary>
        /// Dry run all type maps in given profile
        /// </summary>
        /// <typeparam name="TProfile">Profile type</typeparam>
        public static void AssertConfigurationIsValid<TProfile>() where TProfile : Profile, new()
        {
            Context.AssertConfigurationIsValid<TProfile>();
        }

        /// <summary>
        /// Clear out all existing configuration
        /// </summary>
        public static void Reset()
        {
            Context.Reset();
        }

        /// <summary>
        /// Globally ignore all members starting with a prefix
        /// </summary>
        /// <param name="startingwith">Prefix of members to ignore. Call this before all other maps created.</param>
        public static void AddGlobalIgnore(string startingwith)
        {
            Context.AddGlobalIgnore(startingwith);
        }
    }
}