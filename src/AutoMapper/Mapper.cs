namespace AutoMapper
{
    using System;
    using System.Linq;
    using Configuration;
    using Execution;
    using Mappers;

    public class Mapper : IRuntimeMapper
    {
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
                    throw new InvalidOperationException("Mapper not initialized. Call Initialize with appropriate configuration.");

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
                    throw new InvalidOperationException("Mapper not initialized. Call Initialize with appropriate configuration.");

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

        #endregion

        private readonly IConfigurationProvider _configurationProvider;
        private readonly Func<Type, object> _serviceCtor;
        private readonly MappingOperationOptions _defaultMappingOptions;

        public Mapper(IConfigurationProvider configurationProvider)
            : this(configurationProvider, configurationProvider.ServiceCtor)
        {
        }

        public Mapper(IConfigurationProvider configurationProvider, Func<Type, object> serviceCtor)
        {
            _configurationProvider = configurationProvider;
            _serviceCtor = serviceCtor;
            _defaultMappingOptions = new MappingOperationOptions(_serviceCtor);
        }

        Func<Type, object> IMapper.ServiceCtor => _serviceCtor;

        IConfigurationProvider IMapper.ConfigurationProvider => _configurationProvider;

        TDestination IMapper.Map<TDestination>(object source)
        {
            var types = new TypePair(source.GetType(), typeof(TDestination));

            var func = _configurationProvider.GetUntypedMapperFunc(new MapRequest(types, types));

            var context = new ResolutionContext(source, null, types, _defaultMappingOptions, this);

            return (TDestination) func(source, null, context);
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
            var types = TypePair.Create(source, null, typeof(TSource), typeof (TDestination));

            var func = _configurationProvider.GetMapperFunc<TSource, TDestination>(types);

            var destination = default(TDestination);

            var context = new ResolutionContext(source, destination, types, _defaultMappingOptions, this);

            return func(source, destination, context);
        }

        TDestination IMapper.Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            var types = TypePair.Create(source, null, typeof(TSource), typeof(TDestination));

            var func = _configurationProvider.GetMapperFunc<TSource, TDestination>(types);

            var destination = default(TDestination);

            var typedOptions = new MappingOperationOptions<TSource, TDestination>(_serviceCtor);

            opts(typedOptions);

            var context = new ResolutionContext(source, destination, types, typedOptions, this);

            return func(source, destination, context);
        }

        TDestination IMapper.Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            var types = TypePair.Create(source, destination, typeof(TSource), typeof(TDestination));

            var func = _configurationProvider.GetMapperFunc<TSource, TDestination>(types);

            var context = new ResolutionContext(source, destination, types, _defaultMappingOptions, this);

            return func(source, destination, context);
        }

        TDestination IMapper.Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            var types = TypePair.Create(source, destination, typeof(TSource), typeof(TDestination));

            var func = _configurationProvider.GetMapperFunc<TSource, TDestination>(types);

            var typedOptions = new MappingOperationOptions<TSource, TDestination>(_serviceCtor);

            opts(typedOptions);

            var context = new ResolutionContext(source, destination, types, typedOptions, this);

            return func(source, destination, context);
        }

        object IMapper.Map(object source, Type sourceType, Type destinationType)
        {
            var types = TypePair.Create(source, null, sourceType, destinationType);

            var func = _configurationProvider.GetUntypedMapperFunc(new MapRequest(new TypePair(sourceType, destinationType), types));

            var context = new ResolutionContext(source, null, types, _defaultMappingOptions, this);

            return func(source, null, context);
        }

        object IMapper.Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            var types = TypePair.Create(source, null, sourceType, destinationType);

            var func = _configurationProvider.GetUntypedMapperFunc(new MapRequest(new TypePair(sourceType, destinationType), types));

            var options = new MappingOperationOptions(_serviceCtor);
            opts(options);

            var context = new ResolutionContext(source, null, types, options, this);

            return func(source, null, context);
        }

        object IMapper.Map(object source, object destination, Type sourceType, Type destinationType)
        {
            var types = TypePair.Create(source, destination, sourceType, destinationType);

            var func = _configurationProvider.GetUntypedMapperFunc(new MapRequest(new TypePair(sourceType, destinationType), types));

            var context = new ResolutionContext(source, destination, types, _defaultMappingOptions, this);

            return func(source, destination, context);
        }

        object IMapper.Map(object source, object destination, Type sourceType, Type destinationType,
            Action<IMappingOperationOptions> opts)
        {
            var types = TypePair.Create(source, destination, sourceType, destinationType);

            var func = _configurationProvider.GetUntypedMapperFunc(new MapRequest(new TypePair(sourceType, destinationType), types));

            var options = new MappingOperationOptions(_serviceCtor);
            opts(options);

            var context = new ResolutionContext(source, destination, types, options, this);

            return func(source, destination, context);
        }

        object IRuntimeMapper.Map(ResolutionContext context)
        {
            var types = TypePair.Create(context.SourceValue, context.DestinationValue, context.SourceType, context.DestinationType);

            var func = _configurationProvider.GetUntypedMapperFunc(new MapRequest(new TypePair(context.SourceType, context.DestinationType), types));

            return func(context.SourceValue, context.DestinationValue, context);
        }

        object IRuntimeMapper.Map(object source, object destination, Type sourceType, Type destinationType, ResolutionContext parent)
        {
            var types = TypePair.Create(source, destination, sourceType, destinationType);

            var func = _configurationProvider.GetUntypedMapperFunc(new MapRequest(new TypePair(sourceType, destinationType), types));

            var context = new ResolutionContext(source, destination, types, parent);

            return func(source, destination, context);
        }

        TDestination IRuntimeMapper.Map<TSource, TDestination>(TSource source, TDestination destination, ResolutionContext parent)
        {
            var types = TypePair.Create(source, destination, typeof(TSource), typeof(TDestination));

            var func = _configurationProvider.GetMapperFunc<TSource, TDestination>(types);

            var context = new ResolutionContext(source, destination, types, parent);

            return func(source, destination, context);
        }

        object IRuntimeMapper.CreateObject(ResolutionContext context)
        {
            if(context.DestinationValue != null)
            {
                return context.DestinationValue;
            }
            return !_configurationProvider.AllowNullDestinationValues
                ? ObjectCreator.CreateNonNullValue(context.DestinationType)
                : ObjectCreator.CreateObject(context.DestinationType);
        }

        TDestination IRuntimeMapper.CreateObject<TDestination>(ResolutionContext context)
        {
            if (context.DestinationValue != null)
            {
                return (TDestination) context.DestinationValue;
            }
            return (TDestination) (!_configurationProvider.AllowNullDestinationValues
                ? ObjectCreator.CreateNonNullValue(typeof(TDestination))
                : ObjectCreator.CreateObject(typeof(TDestination)));
        }

        bool IRuntimeMapper.ShouldMapSourceValueAsNull(ResolutionContext context)
        {
            if (context.DestinationType.IsValueType() && !context.DestinationType.IsNullableType())
                return false;

            var typeMap = context.GetContextTypeMap();

            return typeMap?.Profile.AllowNullDestinationValues ?? _configurationProvider.AllowNullDestinationValues;
        }

        bool IRuntimeMapper.ShouldMapSourceCollectionAsNull(ResolutionContext context)
        {
            var typeMap = context.GetContextTypeMap();

            return typeMap?.Profile.AllowNullCollections ?? _configurationProvider.AllowNullCollections;
        }
    }
}