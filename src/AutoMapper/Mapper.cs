namespace AutoMapper
{
    using System;
    using Configuration;
    using Mappers;
    using ObjectMappingOperationOptions = MappingOperationOptions<object, object>;

    public class Mapper : IRuntimeMapper
    {
        private const string InvalidOperationMessage = "Mapper not initialized. Call Initialize with appropriate configuration. If you are trying to use mapper instances through a container or otherwise, make sure you do not have any calls to the static Mapper.Map methods, and if you're using ProjectTo or UseAsDataSource extension methods, make sure you pass in the appropriate IConfigurationProvider instance.";

        #region Static API

        private static IConfigurationProvider _configuration;
        private static IMapper _instance;

        /// <summary>
        /// Configuration provider for performing maps
        /// </summary>
        public static IConfigurationProvider Configuration
        {
            get
            {
                if (_configuration == null)
                    throw new InvalidOperationException(InvalidOperationMessage);

                return _configuration;
            }
            private set { _configuration = value; }
        }

        /// <summary>
        /// Static mapper instance. You can also create a <see cref="Mapper"/> instance directly using the <see cref="Configuration"/> instance.
        /// </summary>
        public static IMapper Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException(InvalidOperationMessage);

                return _instance;
            }
            private set { _instance = value; }
        }

        /// <summary>
        /// Initialize static configuration instance
        /// </summary>
        /// <param name="config">Configuration action</param>
        public static void Initialize(Action<IMapperConfigurationExpression> config)
        {
            Configuration = new MapperConfiguration(config);
            Instance = new Mapper(Configuration);
        }

        /// <summary>
        /// Initialize static configuration instance
        /// </summary>
        /// <param name="config">Configuration action</param>
        public static void Initialize(MapperConfigurationExpression config)
        {
            Configuration = new MapperConfiguration(config);
            Instance = new Mapper(Configuration);
        }

        /// <summary>
        /// Execute a mapping from the source object to a new destination object.
        /// The source type is inferred from the source object.
        /// </summary>
        /// <typeparam name="TDestination">Destination type to create</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <returns>Mapped destination object</returns>
        public static TDestination Map<TDestination>(object source) => Instance.Map<TDestination>(source);

        /// <summary>
        /// Execute a mapping from the source object to a new destination object with supplied mapping options.
        /// </summary>
        /// <typeparam name="TDestination">Destination type to create</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>Mapped destination object</returns>
        public static TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> opts)
            => Instance.Map<TDestination>(source, opts);

        /// <summary>
        /// Execute a mapping from the source object to a new destination object.
        /// </summary>
        /// <typeparam name="TSource">Source type to use, regardless of the runtime type</typeparam>
        /// <typeparam name="TDestination">Destination type to create</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <returns>Mapped destination object</returns>
        public static TDestination Map<TSource, TDestination>(TSource source)
            => Instance.Map<TSource, TDestination>(source);

        /// <summary>
        /// Execute a mapping from the source object to a new destination object with supplied mapping options.
        /// </summary>
        /// <typeparam name="TSource">Source type to use</typeparam>
        /// <typeparam name="TDestination">Destination type to create</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>Mapped destination object</returns>
        public static TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions<TSource, TDestination>> opts)
            => Instance.Map(source, opts);

        /// <summary>
        /// Execute a mapping from the source object to the existing destination object.
        /// </summary>
        /// <typeparam name="TSource">Source type to use</typeparam>
        /// <typeparam name="TDestination">Dsetination type</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination">Destination object to map into</param>
        /// <returns>The mapped destination object, same instance as the <paramref name="destination"/> object</returns>
        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
            => Instance.Map(source, destination);

        /// <summary>
        /// Execute a mapping from the source object to the existing destination object with supplied mapping options.
        /// </summary>
        /// <typeparam name="TSource">Source type to use</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination">Destination object to map into</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>The mapped destination object, same instance as the <paramref name="destination"/> object</returns>
        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions<TSource, TDestination>> opts)
            => Instance.Map(source, destination, opts);

        /// <summary>
        /// Execute a mapping from the source object to a new destination object with explicit <see cref="System.Type"/> objects
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to create</param>
        /// <returns>Mapped destination object</returns>
        public static object Map(object source, Type sourceType, Type destinationType)
            => Instance.Map(source, sourceType, destinationType);

        /// <summary>
        /// Execute a mapping from the source object to a new destination object with explicit <see cref="System.Type"/> objects and supplied mapping options.
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to create</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>Mapped destination object</returns>
        public static object Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
            => Instance.Map(source, sourceType, destinationType, opts);

        /// <summary>
        /// Execute a mapping from the source object to existing destination object with explicit <see cref="System.Type"/> objects
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination">Destination object to map into</param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to use</param>
        /// <returns>Mapped destination object, same instance as the <paramref name="destination"/> object</returns>
        public static object Map(object source, object destination, Type sourceType, Type destinationType)
            => Instance.Map(source, destination, sourceType, destinationType);

        /// <summary>
        /// Execute a mapping from the source object to existing destination object with supplied mapping options and explicit <see cref="System.Type"/> objects
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination">Destination object to map into</param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to use</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>Mapped destination object, same instance as the <paramref name="destination"/> object</returns>
        public static object Map(object source, object destination, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
            => Instance.Map(source, destination, sourceType, destinationType, opts);

        /// <summary>
        /// Dry run all configured type maps and throw <see cref="AutoMapperConfigurationException"/> for each problem
        /// </summary>
        public static void AssertConfigurationIsValid() => Configuration.AssertConfigurationIsValid();

        #endregion

        private readonly IConfigurationProvider _configurationProvider;
        private readonly Func<Type, object> _serviceCtor;
        private readonly ResolutionContext _defaultContext;

        public Mapper(IConfigurationProvider configurationProvider)
            : this(configurationProvider, configurationProvider.ServiceCtor)
        {
        }

        public Mapper(IConfigurationProvider configurationProvider, Func<Type, object> serviceCtor)
        {
            _configurationProvider = configurationProvider;
            _serviceCtor = serviceCtor;
            _defaultContext = new ResolutionContext(new ObjectMappingOperationOptions(serviceCtor), this);
        }

        public ResolutionContext DefaultContext => _defaultContext;

        Func<Type, object> IMapper.ServiceCtor => _serviceCtor;

        IConfigurationProvider IMapper.ConfigurationProvider => _configurationProvider;

        TDestination IMapper.Map<TDestination>(object source)
        {
            if (source == null)
                return default(TDestination);

            var types = new TypePair(source.GetType(), typeof(TDestination));

            var func = _configurationProvider.GetUntypedMapperFunc(new MapRequest(types, types));

            return (TDestination) func(source, null, _defaultContext);
        }

        TDestination IMapper.Map<TDestination>(object source, Action<IMappingOperationOptions> opts)
        {
            var mappedObject = default(TDestination);

            if (source == null) return mappedObject;

            var sourceType = source.GetType();
            var destinationType = typeof(TDestination);

            mappedObject = (TDestination)((IMapper)this).Map(source, sourceType, destinationType, opts);
            return mappedObject;
        }

        TDestination IMapper.Map<TSource, TDestination>(TSource source)
        {
            var types = TypePair.Create(source, typeof(TSource), typeof (TDestination));

            var func = _configurationProvider.GetMapperFunc<TSource, TDestination>(types);

            var destination = default(TDestination);

            return func(source, destination, _defaultContext);
        }

        TDestination IMapper.Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            var types = TypePair.Create(source, typeof(TSource), typeof(TDestination));

            var func = _configurationProvider.GetMapperFunc<TSource, TDestination>(types);

            var destination = default(TDestination);

            var typedOptions = new MappingOperationOptions<TSource, TDestination>(_serviceCtor);

            opts(typedOptions);

            typedOptions.BeforeMapAction(source, destination);

            var context = new ResolutionContext(typedOptions, this);

            destination = func(source, destination, context);

            typedOptions.AfterMapAction(source, destination);

            return destination;
        }

        TDestination IMapper.Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            var types = TypePair.Create(source, destination, typeof(TSource), typeof(TDestination));

            var func = _configurationProvider.GetMapperFunc<TSource, TDestination>(types);

            return func(source, destination, _defaultContext);
        }

        TDestination IMapper.Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            var types = TypePair.Create(source, destination, typeof(TSource), typeof(TDestination));

            var func = _configurationProvider.GetMapperFunc<TSource, TDestination>(types);

            var typedOptions = new MappingOperationOptions<TSource, TDestination>(_serviceCtor);

            opts(typedOptions);

            typedOptions.BeforeMapAction(source, destination);

            var context = new ResolutionContext(typedOptions, this);

            destination = func(source, destination, context);

            typedOptions.AfterMapAction(source, destination);

            return destination;
        }

        object IMapper.Map(object source, Type sourceType, Type destinationType)
        {
            var types = TypePair.Create(source, sourceType, destinationType);

            var func = _configurationProvider.GetUntypedMapperFunc(new MapRequest(new TypePair(sourceType, destinationType), types));

            return func(source, null, _defaultContext);
        }

        object IMapper.Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            var types = TypePair.Create(source, sourceType, destinationType);

            var func = _configurationProvider.GetUntypedMapperFunc(new MapRequest(new TypePair(sourceType, destinationType), types));

            var options = new ObjectMappingOperationOptions(_serviceCtor);

            opts(options);

            options.BeforeMapAction(source, null);

            var context = new ResolutionContext(options, this);

            var destination = func(source, null, context);

            options.AfterMapAction(source, destination);

            return destination;
        }

        object IMapper.Map(object source, object destination, Type sourceType, Type destinationType)
        {
            var types = TypePair.Create(source, destination, sourceType, destinationType);

            var func = _configurationProvider.GetUntypedMapperFunc(new MapRequest(new TypePair(sourceType, destinationType), types));

            return func(source, destination, _defaultContext);
        }

        object IMapper.Map(object source, object destination, Type sourceType, Type destinationType,
            Action<IMappingOperationOptions> opts)
        {
            var types = TypePair.Create(source, destination, sourceType, destinationType);

            var func = _configurationProvider.GetUntypedMapperFunc(new MapRequest(new TypePair(sourceType, destinationType), types));

            var options = new ObjectMappingOperationOptions(_serviceCtor);

            opts(options);

            options.BeforeMapAction(source, destination);

            var context = new ResolutionContext(options, this);

            destination = func(source, destination, context);

            options.AfterMapAction(source, destination);

            return destination;
        }

        object IRuntimeMapper.Map(object source, object destination, Type sourceType, Type destinationType, ResolutionContext context)
        {
            var types = TypePair.Create(source, destination, sourceType, destinationType);

            var func = _configurationProvider.GetUntypedMapperFunc(new MapRequest(new TypePair(sourceType, destinationType), types));

            return func(source, destination, context);
        }

        TDestination IRuntimeMapper.Map<TSource, TDestination>(TSource source, TDestination destination, ResolutionContext context)
        {
            var types = TypePair.Create(source, destination, typeof(TSource), typeof(TDestination));

            var func = _configurationProvider.GetMapperFunc<TSource, TDestination>(types);

            return func(source, destination, context);
        }
    }
}