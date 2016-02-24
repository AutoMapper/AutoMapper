namespace AutoMapper
{
    using System;
    using Mappers;

    /// <summary>
    /// Main entry point for AutoMapper, for both creating maps and performing maps.
    /// </summary>
    public class Mapper : IMapper, IDynamicMapper
    {
        #region Static API
        private static readonly Func<MapperConfiguration> _configurationInit =
            () => new MapperConfiguration(cfg => { }, MapperRegistry.Mappers, TypeMapObjectMapperRegistry.Mappers);

        private static Lazy<MapperConfiguration> _configuration = new Lazy<MapperConfiguration>(_configurationInit);

        private static readonly Func<Mapper> _mappingEngineInit =
            () => new Mapper(_configuration.Value);

        private static Lazy<Mapper> _mappingEngine = new Lazy<Mapper>(_mappingEngineInit);

        /// <summary>
        /// When set, destination can have null values. Defaults to true.
        /// This does not affect simple types, only complex ones.
        /// </summary>
        [Obsolete("The static API will be removed in version 5.0. Use a MapperConfiguration instance and store statically as needed. Use CreateMapper to create a mapper instance.")]
        public static bool AllowNullDestinationValues
        {
            get { return Configuration.AllowNullDestinationValues; }
            set { Configuration.AllowNullDestinationValues = value; }
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
            return Instance.Map<TDestination>(source);
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
            return Instance.Map<TDestination>(source, opts);
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
            return Instance.Map<TSource, TDestination>(source);
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
            return Instance.Map(source, destination);
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
            return Instance.Map(source, destination, opts);
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
            return Instance.Map(source, opts);
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
            return Instance.Map(source, sourceType, destinationType);
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
            return Instance.Map(source, sourceType, destinationType, opts);
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
            return Instance.Map(source, destination, sourceType, destinationType);
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
            return Instance.Map(source, destination, sourceType, destinationType, opts);
        }

        /// <summary>
        /// Create a map between the <typeparamref name="TSource"/> and <typeparamref name="TDestination"/> types and execute the map
        /// </summary>
        /// <typeparam name="TSource">Source type to use</typeparam>
        /// <typeparam name="TDestination">Destination type to use</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <returns>Mapped destination object</returns>
        [Obsolete("Set the CreateMissingTypeMaps property on Mapper.ConfigurationProvider or your Profile instead")]
        public static TDestination DynamicMap<TSource, TDestination>(TSource source)
        {
            return DynamicInstance.DynamicMap<TSource, TDestination>(source);
        }

        /// <summary>
        /// Create a map between the <typeparamref name="TSource"/> and <typeparamref name="TDestination"/> types and execute the map to the existing destination object
        /// </summary>
        /// <typeparam name="TSource">Source type to use</typeparam>
        /// <typeparam name="TDestination">Destination type to use</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination">Destination object to map into</param>
        [Obsolete("Set the CreateMissingTypeMaps property on Mapper.ConfigurationProvider or your Profile instead")]
        public static void DynamicMap<TSource, TDestination>(TSource source, TDestination destination)
        {
            DynamicInstance.DynamicMap(source, destination);
        }

        /// <summary>
        /// Create a map between the <paramref name="source"/> object and <typeparamref name="TDestination"/> types and execute the map.
        /// Source type is inferred from the source object .
        /// </summary>
        /// <typeparam name="TDestination">Destination type to use</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <returns>Mapped destination object</returns>
        [Obsolete("Set the CreateMissingTypeMaps property on Mapper.ConfigurationProvider or your Profile instead")]
        public static TDestination DynamicMap<TDestination>(object source)
        {
            return DynamicInstance.DynamicMap<TDestination>(source);
        }

        /// <summary>
        /// Create a map between the <paramref name="sourceType"/> and <paramref name="destinationType"/> types and execute the map.
        /// Use this method when the source and destination types are not known until runtime.
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to use</param>
        /// <returns>Mapped destination object</returns>
        [Obsolete("Set the CreateMissingTypeMaps property on Mapper.ConfigurationProvider or your Profile instead")]
        public static object DynamicMap(object source, Type sourceType, Type destinationType)
        {
            return DynamicInstance.DynamicMap(source, sourceType, destinationType);
        }

        /// <summary>
        /// Create a map between the <paramref name="sourceType"/> and <paramref name="destinationType"/> types and execute the map to the existing destination object.
        /// Use this method when the source and destination types are not known until runtime.
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination"></param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to use</param>
        [Obsolete("Set the CreateMissingTypeMaps property on Mapper.ConfigurationProvider or your Profile instead")]
        public static void DynamicMap(object source, object destination, Type sourceType, Type destinationType)
        {
            DynamicInstance.DynamicMap(source, destination, sourceType, destinationType);
        }

        /// <summary>
        /// Initializes the mapper with the supplied configuration. Runtime optimization complete after this method is called.
        /// This is the preferred means to configure AutoMapper.
        /// </summary>
        /// <param name="action">Initialization callback</param>
        public static void Initialize(Action<IMapperConfiguration> action)
        {
            Reset();

            action(Configuration);

            ((MapperConfiguration)Configuration).Seal();
        }

        /// <summary>
        /// Creates a mapping configuration from the <typeparamref name="TSource"/> type to the <typeparamref name="TDestination"/> type
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <returns>Mapping expression for more configuration options</returns>
        [Obsolete("Dynamically creating maps will be removed in version 5.0. Use a MapperConfiguration instance and store statically as needed, or Mapper.Initialize. Use CreateMapper to create a mapper instance.")]
        public static IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            return Configuration.CreateMap<TSource, TDestination>();
        }

        /// <summary>
        /// Creates a mapping configuration from the <typeparamref name="TSource"/> type to the <typeparamref name="TDestination"/> type.
        /// Specify the member list to validate against during configuration validation.
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="memberList">Member list to validate</param>
        /// <returns>Mapping expression for more configuration options</returns>
        [Obsolete("Dynamically creating maps will be removed in version 5.0. Use a MapperConfiguration instance and store statically as needed, or Mapper.Initialize. Use CreateMapper to create a mapper instance.")]
        public static IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList)
        {
            return Configuration.CreateMap<TSource, TDestination>(memberList);
        }

        /// <summary>
        /// Create a mapping configuration from the source type to the destination type.
        /// Use this method when the source and destination type are known at runtime and not compile time.
        /// </summary>
        /// <param name="sourceType">Source type</param>
        /// <param name="destinationType">Destination type</param>
        /// <returns>Mapping expression for more configuration options</returns>
        [Obsolete("Dynamically creating maps will be removed in version 5.0. Use a MapperConfiguration instance and store statically as needed, or Mapper.Initialize. Use CreateMapper to create a mapper instance.")]
        public static IMappingExpression CreateMap(Type sourceType, Type destinationType)
        {
            return Configuration.CreateMap(sourceType, destinationType);
        }

        /// <summary>
        /// Creates a mapping configuration from the source type to the destination type.
        /// Specify the member list to validate against during configuration validation.
        /// </summary>
        /// <param name="sourceType">Source type</param>
        /// <param name="destinationType">Destination type</param>
        /// <param name="memberList">Member list to validate</param>
        /// <returns>Mapping expression for more configuration options</returns>
        [Obsolete("Dynamically creating maps will be removed in version 5.0. Use a MapperConfiguration instance and store statically as needed, or Mapper.Initialize. Use CreateMapper to create a mapper instance.")]
        public static IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList)
        {
            return Configuration.CreateMap(sourceType, destinationType, memberList);
        }

        /// <summary>
        /// Create a named profile for grouped mapping configuration
        /// </summary>
        /// <param name="profileName">Profile name</param>
        /// <returns>Profile configuration options</returns>
        [Obsolete("Dynamically creating maps will be removed in version 5.0. Use a MapperConfiguration instance and store statically as needed, or Mapper.Initialize. Use CreateMapper to create a mapper instance.")]
        public static IProfileExpression CreateProfile(string profileName)
        {
            return Configuration.CreateProfile(profileName);
        }

        /// <summary>
        /// Create a named profile for grouped mapping configuration, and configure the profile
        /// </summary>
        /// <param name="profileName">Profile name</param>
        /// <param name="profileConfiguration">Profile configuration callback</param>
        [Obsolete("Dynamically creating maps will be removed in version 5.0. Use a MapperConfiguration instance and store statically as needed, or Mapper.Initialize. Use CreateMapper to create a mapper instance.")]
        public static void CreateProfile(string profileName, Action<IProfileExpression> profileConfiguration)
        {
            Configuration.CreateProfile(profileName, profileConfiguration);
        }

        /// <summary>
        /// Add an existing profile
        /// </summary>
        /// <param name="profile">Profile to add</param>
        [Obsolete("Dynamically creating maps will be removed in version 5.0. Use a MapperConfiguration instance and store statically as needed, or Mapper.Initialize. Use CreateMapper to create a mapper instance.")]
        public static void AddProfile(Profile profile)
        {
            Configuration.AddProfile(profile);
        }

        /// <summary>
        /// Add an existing profile type. Profile will be instantiated and added to the configuration.
        /// </summary>
        /// <typeparam name="TProfile">Profile type</typeparam>
        [Obsolete("Dynamically creating maps will be removed in version 5.0. Use a MapperConfiguration instance and store statically as needed, or Mapper.Initialize. Use CreateMapper to create a mapper instance.")]
        public static void AddProfile<TProfile>() where TProfile : Profile, new()
        {
            Configuration.AddProfile<TProfile>();
        }

        /// <summary>
        /// Find the <see cref="TypeMap"/> for the configured source and destination type
        /// </summary>
        /// <param name="sourceType">Configured source type</param>
        /// <param name="destinationType">Configured destination type</param>
        /// <returns>Type map configuration</returns>
        [Obsolete("Dynamically creating maps will be removed in version 5.0. Use a MapperConfiguration instance and store statically as needed, or Mapper.Initialize. Use CreateMapper to create a mapper instance.")]
        public static TypeMap FindTypeMapFor(Type sourceType, Type destinationType)
        {
            return ConfigurationProvider.FindTypeMapFor(sourceType, destinationType);
        }

        /// <summary>
        /// Find the <see cref="TypeMap"/> for the configured source and destination type
        /// </summary>
        /// <typeparam name="TSource">Configured source type</typeparam>
        /// <typeparam name="TDestination">Configured destination type</typeparam>
        /// <returns>Type map configuration</returns>
        [Obsolete("Dynamically creating maps will be removed in version 5.0. Use a MapperConfiguration instance and store statically as needed, or Mapper.Initialize. Use CreateMapper to create a mapper instance.")]
        public static TypeMap FindTypeMapFor<TSource, TDestination>()
        {
            return ConfigurationProvider.FindTypeMapFor(typeof (TSource), typeof (TDestination));
        }

        /// <summary>
        /// Get all configured type maps created
        /// </summary>
        /// <returns>All configured type maps</returns>
        [Obsolete("Dynamically creating maps will be removed in version 5.0. Use a MapperConfiguration instance and store statically as needed, or Mapper.Initialize. Use CreateMapper to create a mapper instance.")]
        public static TypeMap[] GetAllTypeMaps()
        {
            return ConfigurationProvider.GetAllTypeMaps();
        }

        /// <summary>
        /// Dry run all configured type maps and throw <see cref="AutoMapperConfigurationException"/> for each problem
        /// </summary>
        public static void AssertConfigurationIsValid()
        {
            ConfigurationProvider.AssertConfigurationIsValid();
        }

        /// <summary>
        /// Dry run single type map
        /// </summary>
        /// <param name="typeMap">Type map to check</param>
        public static void AssertConfigurationIsValid(TypeMap typeMap)
        {
            ConfigurationProvider.AssertConfigurationIsValid(typeMap);
        }

        /// <summary>
        /// Dry run all type maps in given profile
        /// </summary>
        /// <param name="profileName">Profile name of type maps to test</param>
        public static void AssertConfigurationIsValid(string profileName)
        {
            ConfigurationProvider.AssertConfigurationIsValid(profileName);
        }

        /// <summary>
        /// Dry run all type maps in given profile
        /// </summary>
        /// <typeparam name="TProfile">Profile type</typeparam>
        public static void AssertConfigurationIsValid<TProfile>() where TProfile : Profile, new()
        {
            ConfigurationProvider.AssertConfigurationIsValid<TProfile>();
        }

        /// <summary>
        /// Clear out all existing configuration
        /// </summary>
        [Obsolete("Dynamically creating maps will be removed in version 5.0. Use a MapperConfiguration instance and store statically as needed, or Mapper.Initialize. Use CreateMapper to create a mapper instance.")]
        public static void Reset()
        {
            MapperRegistry.Reset();
            _configuration = new Lazy<MapperConfiguration>(_configurationInit);
            _mappingEngine = new Lazy<Mapper>(_mappingEngineInit);
        }

        /// <summary>
        /// Mapping engine used to perform mappings
        /// </summary>
        public static IMappingEngine Engine => _mappingEngine.Value._engine;
        public static IMapper Instance => _mappingEngine.Value;
        internal static IConfigurationProvider ConfigurationProvider => _configuration.Value;
        private static IDynamicMapper DynamicInstance => _mappingEngine.Value;

        /// <summary>
        /// Store for all configuration
        /// </summary>
        [Obsolete("Dynamically creating maps will be removed in version 5.0. Use a MapperConfiguration instance and store statically as needed, or Mapper.Initialize. Use CreateMapper to create a mapper instance.")]
        public static IMapperConfiguration Configuration => _configuration.Value;


        /// <summary>
        /// Globally ignore all members starting with a prefix
        /// </summary>
        /// <param name="startingwith">Prefix of members to ignore. Call this before all other maps created.</param>
        [Obsolete("Dynamically creating maps will be removed in version 5.0. Use a MapperConfiguration instance and store statically as needed, or Mapper.Initialize. Use CreateMapper to create a mapper instance.")]
        public static void AddGlobalIgnore(string startingwith)
        {
            Configuration.AddGlobalIgnore(startingwith);
        }
        #endregion

        #region Instance API

        private readonly IMappingEngine _engine;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly Func<Type, object> _serviceCtor;

        internal Mapper(IConfigurationProvider configurationProvider)
            : this(configurationProvider, configurationProvider.ServiceCtor)
        {
        }

        internal Mapper(IConfigurationProvider configurationProvider, Func<Type, object> serviceCtor)
        {
            _configurationProvider = configurationProvider;
            _serviceCtor = serviceCtor;
            _engine = new MappingEngine(configurationProvider, this);
        }

        TDestination IMapper.Map<TDestination>(object source)
        {
            return ((IMapper)this).Map<TDestination>(source, DefaultMappingOptions);
        }

        TDestination IMapper.Map<TDestination>(object source, Action<IMappingOperationOptions> opts)
        {
            var mappedObject = default(TDestination);
            if (source != null)
            {
                var sourceType = source.GetType();
                var destinationType = typeof(TDestination);

                mappedObject = (TDestination)((IMapper)this).Map(source, sourceType, destinationType, opts);
            }
            return mappedObject;
        }

        TDestination IMapper.Map<TSource, TDestination>(TSource source)
        {
            Type modelType = typeof(TSource);
            Type destinationType = typeof(TDestination);

            return (TDestination)((IMapper)this).Map(source, modelType, destinationType, DefaultMappingOptions);
        }

        TDestination IMapper.Map<TSource, TDestination>(TSource source,
            Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            Type modelType = typeof(TSource);
            Type destinationType = typeof(TDestination);

            var options = new MappingOperationOptions<TSource, TDestination>();
            opts(options);

            return (TDestination)MapCore(source, modelType, destinationType, options);
        }

        TDestination IMapper.Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return ((IMapper)this).Map(source, destination, DefaultMappingOptions);
        }

        TDestination IMapper.Map<TSource, TDestination>(TSource source, TDestination destination,
            Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            Type modelType = typeof(TSource);
            Type destinationType = typeof(TDestination);

            var options = new MappingOperationOptions<TSource, TDestination>();
            opts(options);

            return (TDestination)MapCore(source, destination, modelType, destinationType, options);
        }

        object IMapper.Map(object source, Type sourceType, Type destinationType)
        {
            return ((IMapper)this).Map(source, sourceType, destinationType, DefaultMappingOptions);
        }

        object IMapper.Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            var options = new MappingOperationOptions();

            opts(options);

            return MapCore(source, sourceType, destinationType, options);
        }

        object IMapper.Map(object source, object destination, Type sourceType, Type destinationType)
        {
            return ((IMapper)this).Map(source, destination, sourceType, destinationType, DefaultMappingOptions);
        }

        object IMapper.Map(object source, object destination, Type sourceType, Type destinationType,
            Action<IMappingOperationOptions> opts)
        {
            var options = new MappingOperationOptions();

            opts(options);

            return MapCore(source, destination, sourceType, destinationType, options);
        }

        TDestination IDynamicMapper.DynamicMap<TSource, TDestination>(TSource source)
        {
            Type modelType = typeof(TSource);
            Type destinationType = typeof(TDestination);

            return (TDestination)((IDynamicMapper)this).DynamicMap(source, modelType, destinationType);
        }

        void IDynamicMapper.DynamicMap<TSource, TDestination>(TSource source, TDestination destination)
        {
            Type modelType = typeof(TSource);
            Type destinationType = typeof(TDestination);

            ((IDynamicMapper)this).DynamicMap(source, destination, modelType, destinationType);
        }

        TDestination IDynamicMapper.DynamicMap<TDestination>(object source)
        {
            Type modelType = source?.GetType() ?? typeof(object);
            Type destinationType = typeof(TDestination);

            return (TDestination)((IDynamicMapper)this).DynamicMap(source, modelType, destinationType);
        }

        object IDynamicMapper.DynamicMap(object source, Type sourceType, Type destinationType)
        {
            Configuration.CreateMissingTypeMaps = true;
            var typeMap = _configurationProvider.ResolveTypeMap(source, null, sourceType, destinationType);

            var context = new ResolutionContext(typeMap, source, sourceType, destinationType, new MappingOperationOptions(), _engine);

            return _engine.Map(context);
        }

        void IDynamicMapper.DynamicMap(object source, object destination, Type sourceType, Type destinationType)
        {
            Configuration.CreateMissingTypeMaps = true;
            var typeMap = _configurationProvider.ResolveTypeMap(source, destination, sourceType, destinationType);

            var context = new ResolutionContext(typeMap, source, destination, sourceType, destinationType, new MappingOperationOptions(), _engine);

            _engine.Map(context);
        }

        private object MapCore(object source, Type sourceType, Type destinationType, MappingOperationOptions options)
        {
            TypeMap typeMap = _configurationProvider.ResolveTypeMap(source, null, sourceType, destinationType);

            var context = new ResolutionContext(typeMap, source, sourceType, destinationType, options, _engine);

            return _engine.Map(context);
        }

        private object MapCore(object source, object destination, Type sourceType, Type destinationType,
            MappingOperationOptions options)
        {
            TypeMap typeMap = _configurationProvider.ResolveTypeMap(source, destination, sourceType, destinationType);

            var context = new ResolutionContext(typeMap, source, destination, sourceType, destinationType, options, _engine);

            return _engine.Map(context);
        }

        private void DefaultMappingOptions(IMappingOperationOptions opts) => opts.ConstructServicesUsing(_serviceCtor);

        IConfigurationProvider IMapper.ConfigurationProvider => _configurationProvider;

        #endregion
    }
}