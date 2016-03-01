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
        public static void Initialize(Action<IMapperConfiguration> config)
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
        /// Execute a merge mapping from the source object(s) to a new destination object.
        /// </summary>
        /// <typeparam name="TSource">Source type to use, regardless of the runtime type</typeparam>
        /// <typeparam name="TDestination">Destination type to create</typeparam>
        /// <param name="sources">Source object(s) to map from</param>
        /// <returns>Mapped destination object</returns>
        public TDestination Merge<TSource, TDestination>(params TSource[] sources)
            => Instance.Merge<TSource, TDestination>(sources);

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

        private readonly IMappingEngine _engine;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly Func<Type, object> _serviceCtor;

        public Mapper(IConfigurationProvider configurationProvider)
            : this(configurationProvider, configurationProvider.ServiceCtor)
        {
        }

        public Mapper(IConfigurationProvider configurationProvider, Func<Type, object> serviceCtor)
        {
            _configurationProvider = configurationProvider;
            _serviceCtor = serviceCtor;
            _engine = new MappingEngine(configurationProvider, this);
        }

        Func<Type, object> IMapper.ServiceCtor => _serviceCtor;

        IConfigurationProvider IMapper.ConfigurationProvider => _configurationProvider;

        TDestination IMapper.Map<TDestination>(object source) => ((IMapper)this).Map<TDestination>(source, _ => { });

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
            Type modelType = typeof(TSource);
            Type destinationType = typeof(TDestination);

            return (TDestination)((IMapper)this).Map(source, modelType, destinationType, _ => { });
        }

        TDestination IMapper.Map<TSource, TDestination>(TSource source,
            Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            Type modelType = typeof(TSource);
            Type destinationType = typeof(TDestination);

            var options = new MappingOperationOptions<TSource, TDestination>(_serviceCtor);
            opts(options);

            return (TDestination)MapCore(source, null, modelType, destinationType, options);
        }

        TDestination IMapper.Map<TSource, TDestination>(TSource source, TDestination destination)
            => ((IMapper)this).Map(source, destination, _ => { });

        TDestination IMapper.Map<TSource, TDestination>(TSource source, TDestination destination,
            Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            Type modelType = typeof(TSource);
            Type destinationType = typeof(TDestination);

            var options = new MappingOperationOptions<TSource, TDestination>(_serviceCtor);
            opts(options);

            return (TDestination)MapCore(source, destination, modelType, destinationType, options);
        }

        object IMapper.Map(object source, Type sourceType, Type destinationType)
            => ((IMapper)this).Map(source, sourceType, destinationType, _ => { });

        object IMapper.Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            var options = new MappingOperationOptions(_serviceCtor);

            opts(options);

            return MapCore(source, null, sourceType, destinationType, options);
        }

        object IMapper.Map(object source, object destination, Type sourceType, Type destinationType)
            => ((IMapper)this).Map(source, destination, sourceType, destinationType, _ => { });

        object IMapper.Map(object source, object destination, Type sourceType, Type destinationType,
            Action<IMappingOperationOptions> opts)
        {
            var options = new MappingOperationOptions(_serviceCtor);

            opts(options);

            return MapCore(source, destination, sourceType, destinationType, options);
        }

        object IRuntimeMapper.Map(object source, object destination, Type sourceType, Type destinationType, ResolutionContext parent)
            => MapCore(source, destination, sourceType, destinationType, parent);

        object IRuntimeMapper.CreateObject(ResolutionContext context)
        {
            var typeMap = context.TypeMap;
            var destinationType = typeMap?.DestinationType ?? context.DestinationType;

            if (typeMap != null)
                if (typeMap.DestinationCtor != null)
                    return typeMap.DestinationCtor(context);
                else if (typeMap.ConstructDestinationUsingServiceLocator)
                    return context.Options.ServiceCtor(destinationType);
                else if (typeMap.ConstructorMap != null && typeMap.ConstructorMap.CtorParams.All(p => p.CanResolve))
                    return typeMap.ConstructorMap.ResolveValue(context);

            if (context.DestinationValue != null)
                return context.DestinationValue;

            if (destinationType.IsInterface())
#if PORTABLE
                throw new PlatformNotSupportedException("Mapping to interfaces through proxies not supported.");
#else
                destinationType = new ProxyGenerator().GetProxyType(destinationType);
#endif

            return !_configurationProvider.AllowNullDestinationValues
                ? ObjectCreator.CreateNonNullValue(destinationType)
                : ObjectCreator.CreateObject(destinationType);
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

        private object MapCore(object source, object destination, Type sourceType, Type destinationType,
            MappingOperationOptions options)
        {
            TypeMap typeMap = _configurationProvider.ResolveTypeMap(source, destination, sourceType, destinationType);

            var context = new ResolutionContext(source, destination, sourceType, destinationType, typeMap, options, this);

            return _engine.Map(context);
        }

        private object MapCore(object source, object destination, Type sourceType, Type destinationType, ResolutionContext parent)
        {
            TypeMap typeMap = _configurationProvider.ResolveTypeMap(source, destination, sourceType, destinationType);

            var context = new ResolutionContext(source, destination, sourceType, destinationType, typeMap, parent);

            return _engine.Map(context);
        }
    }
}