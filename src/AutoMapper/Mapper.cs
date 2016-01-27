namespace AutoMapper
{
    using System;

    /// <summary>
    /// Main entry point for AutoMapper, for both creating maps and performing maps.
    /// </summary>
    public class Mapper : IMapper
    {
        private readonly IMappingEngine _engine;
        private readonly Func<Type, object> _serviceCtor;

        internal Mapper(IConfigurationProvider configurationProvider)
            : this(configurationProvider, configurationProvider.ServiceCtor)
        {
        }

        internal Mapper(IConfigurationProvider configurationProvider, Func<Type, object> serviceCtor)
        {
            ConfigurationProvider = configurationProvider;
            _serviceCtor = serviceCtor;
            _engine = new MappingEngine(configurationProvider, this);
        }

        public IConfigurationProvider ConfigurationProvider { get; }

        public TDestination Map<TDestination>(object source) => Map<TDestination>(source, DefaultMappingOptions);

        public TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> opts)
        {
            var mappedObject = default(TDestination);

            if (source == null) return mappedObject;

            var sourceType = source.GetType();
            var destinationType = typeof(TDestination);

            mappedObject = (TDestination)Map(source, sourceType, destinationType, opts);
            return mappedObject;
        }

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            Type modelType = typeof(TSource);
            Type destinationType = typeof(TDestination);

            return (TDestination)Map(source, modelType, destinationType, DefaultMappingOptions);
        }

        public TDestination Map<TSource, TDestination>(TSource source,
            Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            Type modelType = typeof(TSource);
            Type destinationType = typeof(TDestination);

            var options = new MappingOperationOptions<TSource, TDestination>();
            opts(options);

            return (TDestination)MapCore(source, modelType, destinationType, options);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return Map(source, destination, DefaultMappingOptions);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination,
            Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            Type modelType = typeof(TSource);
            Type destinationType = typeof(TDestination);

            var options = new MappingOperationOptions<TSource, TDestination>();
            opts(options);

            return (TDestination)MapCore(source, destination, modelType, destinationType, options);
        }

        public object Map(object source, Type sourceType, Type destinationType)
        {
            return Map(source, sourceType, destinationType, DefaultMappingOptions);
        }

        public object Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            var options = new MappingOperationOptions();

            opts(options);

            return MapCore(source, sourceType, destinationType, options);
        }

        public object Map(object source, object destination, Type sourceType, Type destinationType)
        {
            return Map(source, destination, sourceType, destinationType, DefaultMappingOptions);
        }

        public object Map(object source, object destination, Type sourceType, Type destinationType,
            Action<IMappingOperationOptions> opts)
        {
            var options = new MappingOperationOptions();

            opts(options);

            return MapCore(source, destination, sourceType, destinationType, options);
        }


        private object MapCore(object source, Type sourceType, Type destinationType, MappingOperationOptions options)
        {
            TypeMap typeMap = ConfigurationProvider.ResolveTypeMap(source, null, sourceType, destinationType);

            var context = new ResolutionContext(typeMap, source, sourceType, destinationType, options, _engine);

            return _engine.Map(context);
        }

        private object MapCore(object source, object destination, Type sourceType, Type destinationType,
            MappingOperationOptions options)
        {
            TypeMap typeMap = ConfigurationProvider.ResolveTypeMap(source, destination, sourceType, destinationType);

            var context = new ResolutionContext(typeMap, source, destination, sourceType, destinationType, options, _engine);

            return _engine.Map(context);
        }

        private void DefaultMappingOptions(IMappingOperationOptions opts) => opts.ConstructServicesUsing(_serviceCtor);

    }
}