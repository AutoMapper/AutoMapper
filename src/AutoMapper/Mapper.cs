namespace AutoMapper
{
    using System;

    public class Mapper : IMapper
    {
        #region Static API

        //public static IConfigurationProvider Configuration { get; private set; }
        //public static IMapper Instance { get; private set; }

        //public static void Initialize(Action<IMapperConfiguration> config)
        //{
        //    Configuration = new MapperConfiguration(config);
        //    Instance = new Mapper(Configuration);
        //}

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

            return (TDestination)MapCore(source, modelType, destinationType, options);
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

            return MapCore(source, sourceType, destinationType, options);
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
    }
}