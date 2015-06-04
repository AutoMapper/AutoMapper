namespace AutoMapper
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Impl;
    using Internal;
    using Mappers;

    public class MappingEngine : IMappingEngine, IMappingEngineRunner
    {
        private static readonly IDictionaryFactory DictionaryFactory = PlatformAdapter.Resolve<IDictionaryFactory>();

        private static readonly IProxyGeneratorFactory ProxyGeneratorFactory =
            PlatformAdapter.Resolve<IProxyGeneratorFactory>();

        private bool _disposed;
        private readonly IObjectMapper[] _mappers;
        private readonly IDictionary<TypePair, IObjectMapper> _objectMapperCache;
        private readonly Func<Type, object> _serviceCtor;

        public MappingEngine(IConfigurationProvider configurationProvider)
            : this(
                configurationProvider, DictionaryFactory.CreateDictionary<TypePair, IObjectMapper>(),
                configurationProvider.ServiceCtor)
        {
        }

        public MappingEngine(IConfigurationProvider configurationProvider,
            IDictionary<TypePair, IObjectMapper> objectMapperCache, Func<Type, object> serviceCtor)
        {
            ConfigurationProvider = configurationProvider;
            _objectMapperCache = objectMapperCache;
            _serviceCtor = serviceCtor;
            _mappers = configurationProvider.GetMappers();
            ConfigurationProvider.TypeMapCreated += ClearTypeMap;
        }

        public IConfigurationProvider ConfigurationProvider { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (ConfigurationProvider != null)
                        ConfigurationProvider.TypeMapCreated -= ClearTypeMap;
                }

                _disposed = true;
            }
        }

        public TDestination Map<TDestination>(object source)
        {
            return Map<TDestination>(source, DefaultMappingOptions);
        }

        public TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> opts)
        {
            var mappedObject = default(TDestination);
            if (source != null)
            {
                var sourceType = source.GetType();
                var destinationType = typeof (TDestination);

                mappedObject = (TDestination) Map(source, sourceType, destinationType, opts);
            }
            return mappedObject;
        }

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            Type modelType = typeof (TSource);
            Type destinationType = typeof (TDestination);

            return (TDestination) Map(source, modelType, destinationType, DefaultMappingOptions);
        }

        public TDestination Map<TSource, TDestination>(TSource source,
            Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            Type modelType = typeof (TSource);
            Type destinationType = typeof (TDestination);

            var options = new MappingOperationOptions<TSource, TDestination>();
            opts(options);

            return (TDestination) MapCore(source, modelType, destinationType, options);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return Map(source, destination, DefaultMappingOptions);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination,
            Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            Type modelType = typeof (TSource);
            Type destinationType = typeof (TDestination);

            var options = new MappingOperationOptions<TSource, TDestination>();
            opts(options);

            return (TDestination) MapCore(source, destination, modelType, destinationType, options);
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

        private object MapCore(object source, Type sourceType, Type destinationType, MappingOperationOptions options)
        {
            TypeMap typeMap = ConfigurationProvider.ResolveTypeMap(source, null, sourceType, destinationType);

            var context = new ResolutionContext(typeMap, source, sourceType, destinationType, options, this);

            return ((IMappingEngineRunner) this).Map(context);
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

        private object MapCore(object source, object destination, Type sourceType, Type destinationType,
            MappingOperationOptions options)
        {
            TypeMap typeMap = ConfigurationProvider.ResolveTypeMap(source, destination, sourceType, destinationType);

            var context = new ResolutionContext(typeMap, source, destination, sourceType, destinationType, options, this);

            return ((IMappingEngineRunner) this).Map(context);
        }


        public TDestination DynamicMap<TSource, TDestination>(TSource source)
        {
            Type modelType = typeof (TSource);
            Type destinationType = typeof (TDestination);

            return (TDestination) DynamicMap(source, modelType, destinationType);
        }

        public void DynamicMap<TSource, TDestination>(TSource source, TDestination destination)
        {
            Type modelType = typeof (TSource);
            Type destinationType = typeof (TDestination);

            DynamicMap(source, destination, modelType, destinationType);
        }

        public TDestination DynamicMap<TDestination>(object source)
        {
            Type modelType = source?.GetType() ?? typeof (object);
            Type destinationType = typeof (TDestination);

            return (TDestination) DynamicMap(source, modelType, destinationType);
        }

        public object DynamicMap(object source, Type sourceType, Type destinationType)
        {
            var typeMap = ConfigurationProvider.ResolveTypeMap(source, null, sourceType, destinationType) ??
                          ConfigurationProvider.CreateTypeMap(sourceType, destinationType);

            var context = new ResolutionContext(typeMap, source, sourceType, destinationType,
                new MappingOperationOptions
                {
                    CreateMissingTypeMaps = true
                }, this);

            return ((IMappingEngineRunner) this).Map(context);
        }

        public void DynamicMap(object source, object destination, Type sourceType, Type destinationType)
        {
            var typeMap = ConfigurationProvider.ResolveTypeMap(source, destination, sourceType, destinationType) ??
                          ConfigurationProvider.CreateTypeMap(sourceType, destinationType);

            var context = new ResolutionContext(typeMap, source, destination, sourceType, destinationType,
                new MappingOperationOptions
                {
                    CreateMissingTypeMaps = true
                }, this);

            ((IMappingEngineRunner) this).Map(context);
        }

        public TDestination Map<TSource, TDestination>(ResolutionContext parentContext, TSource source)
        {
            Type destinationType = typeof (TDestination);
            Type sourceType = typeof (TSource);
            TypeMap typeMap = ConfigurationProvider.ResolveTypeMap(source, null, sourceType, destinationType);
            var context = parentContext.CreateTypeContext(typeMap, source, null, sourceType, destinationType);
            return (TDestination) ((IMappingEngineRunner) this).Map(context);
        }

        object IMappingEngineRunner.Map(ResolutionContext context)
        {
            try
            {
                var contextTypePair = new TypePair(context.SourceType, context.DestinationType);

                Func<TypePair, IObjectMapper> missFunc =
                    tp => _mappers.FirstOrDefault(mapper => mapper.IsMatch(context));

                IObjectMapper mapperToUse = _objectMapperCache.GetOrAdd(contextTypePair, missFunc);

                if (mapperToUse == null)
                {
                    if (context.Options.CreateMissingTypeMaps)
                    {
                        var typeMap = ConfigurationProvider.CreateTypeMap(context.SourceType, context.DestinationType);

                        context = context.CreateTypeContext(typeMap, context.SourceValue, context.DestinationValue,
                            context.SourceType, context.DestinationType);

                        mapperToUse = _objectMapperCache.GetOrAdd(contextTypePair, missFunc);
                    }
                    else
                    {
                        if (context.SourceValue != null)
                            throw new AutoMapperMappingException(context,
                                "Missing type map configuration or unsupported mapping.");

                        return ObjectCreator.CreateDefaultValue(context.DestinationType);
                    }
                }

                return mapperToUse.Map(context, this);
            }
            catch (AutoMapperMappingException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AutoMapperMappingException(context, ex);
            }
        }

        object IMappingEngineRunner.CreateObject(ResolutionContext context)
        {
            var typeMap = context.TypeMap;
            var destinationType = context.DestinationType;

            if (typeMap != null)
                if (typeMap.DestinationCtor != null)
                    return typeMap.DestinationCtor(context);
                else if (typeMap.ConstructDestinationUsingServiceLocator)
                    return context.Options.ServiceCtor(destinationType);
                else if (typeMap.ConstructorMap != null)
                    return typeMap.ConstructorMap.ResolveValue(context, this);

            if (context.DestinationValue != null)
                return context.DestinationValue;

            if (destinationType.IsInterface())
                destinationType = ProxyGeneratorFactory.Create().GetProxyType(destinationType);

            return !ConfigurationProvider.MapNullSourceValuesAsNull
                ? ObjectCreator.CreateNonNullValue(destinationType)
                : ObjectCreator.CreateObject(destinationType);
        }

        bool IMappingEngineRunner.ShouldMapSourceValueAsNull(ResolutionContext context)
        {
            if (context.DestinationType.IsValueType() && !context.DestinationType.IsNullableType())
                return false;

            var typeMap = context.GetContextTypeMap();
            if (typeMap != null)
                return ConfigurationProvider.GetProfileConfiguration(typeMap.Profile).MapNullSourceValuesAsNull;

            return ConfigurationProvider.MapNullSourceValuesAsNull;
        }

        bool IMappingEngineRunner.ShouldMapSourceCollectionAsNull(ResolutionContext context)
        {
            var typeMap = context.GetContextTypeMap();
            if (typeMap != null)
                return ConfigurationProvider.GetProfileConfiguration(typeMap.Profile).MapNullSourceCollectionsAsNull;

            return ConfigurationProvider.MapNullSourceCollectionsAsNull;
        }

        private void ClearTypeMap(object sender, TypeMapCreatedEventArgs e)
        {
            IObjectMapper existing;

            _objectMapperCache.TryRemove(new TypePair(e.TypeMap.SourceType, e.TypeMap.DestinationType), out existing);
        }

        private void DefaultMappingOptions(IMappingOperationOptions opts)
        {
            opts.ConstructServicesUsing(_serviceCtor);
        }
    }
}