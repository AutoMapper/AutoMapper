namespace AutoMapper
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Impl;
    using Internal;
    using Mappers;
    using QueryableExtensions;

    /// <summary>
    /// 
    /// </summary>
    public class MappingEngine : IMappingEngine, IMappingEngineRunner
    {
        /// <summary>
        /// Gets the runner.
        /// </summary>
        public IMappingEngineRunner Runner => this;

        /// <summary>
        /// Gets the configuration provider.
        /// </summary>
        public IConfigurationProvider ConfigurationProvider => _mapperContext.ConfigurationProvider;

        /// <summary>
        /// 
        /// </summary>
        private readonly IMapperContext _mapperContext;

        /// <summary>
        /// Gets the cache.
        /// </summary>
        public IDictionary<ExpressionRequest, LambdaExpression> ExpressionCache { get; }
            = DictionaryFactory.CreateDictionary<ExpressionRequest, LambdaExpression>();

        /// <summary>
        /// Gets the dictionary factory.
        /// </summary>
        private static IDictionaryFactory DictionaryFactory { get; }
            = PlatformAdapter.Resolve<IDictionaryFactory>();

        /// <summary>
        /// 
        /// </summary>
        private static IProxyGeneratorFactory ProxyGeneratorFactory { get; }
            = PlatformAdapter.Resolve<IProxyGeneratorFactory>();

        private bool _disposed;
        private readonly IDictionary<TypePair, IObjectMapper> _objectMapperCache;
        private readonly Func<Type, object> _serviceCtor;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapperContext"></param>
        public MappingEngine(IMapperContext mapperContext)
            : this(mapperContext, DictionaryFactory.CreateDictionary<TypePair, IObjectMapper>(),
                mapperContext.ConfigurationProvider.ServiceCtor)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapperContext"></param>
        /// <param name="objectMapperCache"></param>
        /// <param name="serviceCtor"></param>
        public MappingEngine(IMapperContext mapperContext,
            IDictionary<TypePair, IObjectMapper> objectMapperCache, Func<Type, object> serviceCtor)
        {
            _mapperContext = mapperContext;
            _objectMapperCache = objectMapperCache;
            _serviceCtor = serviceCtor;
            _mapperContext.ConfigurationProvider.TypeMapCreated += ClearTypeMap;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_mapperContext.ConfigurationProvider != null)
                        _mapperContext.ConfigurationProvider.TypeMapCreated -= ClearTypeMap;
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IDictionary<ExpressionRequest, int> GetNewTypePairCount()
        {
            return DictionaryFactory.CreateDictionary<ExpressionRequest, int>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public TDestination Map<TDestination>(object source)
        {
            return Map<TDestination>(source, DefaultMappingOptions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="opts"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public TDestination Map<TSource, TDestination>(TSource source)
        {
            var modelType = typeof (TSource);
            var destinationType = typeof (TDestination);

            return (TDestination) Map(source, modelType, destinationType, DefaultMappingOptions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="opts"></param>
        /// <returns></returns>
        public TDestination Map<TSource, TDestination>(TSource source,
            Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            var modelType = typeof (TSource);
            var destinationType = typeof (TDestination);

            var options = new MappingOperationOptions<TSource, TDestination>();
            opts(options);

            return (TDestination) MapCore(source, modelType, destinationType, options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return Map(source, destination, DefaultMappingOptions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="opts"></param>
        /// <returns></returns>
        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination,
            Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            var modelType = typeof (TSource);
            var destinationType = typeof (TDestination);

            var options = new MappingOperationOptions<TSource, TDestination>();
            opts(options);

            return (TDestination) MapCore(source, destination, modelType, destinationType, options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public object Map(object source, Type sourceType, Type destinationType)
        {
            return Map(source, sourceType, destinationType, DefaultMappingOptions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <param name="opts"></param>
        /// <returns></returns>
        public object Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            var options = new MappingOperationOptions();

            opts(options);

            return MapCore(source, sourceType, destinationType, options);
        }

        //
        private object MapCore(object source, Type sourceType, Type destinationType, MappingOperationOptions options)
        {
            var typeMap = _mapperContext.ConfigurationProvider.ResolveTypeMap(source, null, sourceType, destinationType);

            var context = new ResolutionContext(typeMap, source, sourceType, destinationType, options, _mapperContext);

            return Runner.Map(context);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public object Map(object source, object destination, Type sourceType, Type destinationType)
        {
            return Map(source, destination, sourceType, destinationType, DefaultMappingOptions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <param name="opts"></param>
        /// <returns></returns>
        public object Map(object source, object destination, Type sourceType, Type destinationType,
            Action<IMappingOperationOptions> opts)
        {
            var options = new MappingOperationOptions();

            opts(options);

            return MapCore(source, destination, sourceType, destinationType, options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private object MapCore(object source, object destination, Type sourceType, Type destinationType,
            MappingOperationOptions options)
        {
            var typeMap = _mapperContext.ConfigurationProvider.ResolveTypeMap(source, destination, sourceType, destinationType);

            var context = new ResolutionContext(typeMap, source, destination, sourceType, destinationType, options, _mapperContext);

            return Runner.Map(context);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public TDestination DynamicMap<TSource, TDestination>(TSource source)
        {
            var modelType = typeof (TSource);
            var destinationType = typeof (TDestination);

            return (TDestination) DynamicMap(source, modelType, destinationType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public void DynamicMap<TSource, TDestination>(TSource source, TDestination destination)
        {
            var modelType = typeof (TSource);
            var destinationType = typeof (TDestination);

            DynamicMap(source, destination, modelType, destinationType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public TDestination DynamicMap<TDestination>(object source)
        {
            //TODO: ruh roh! another C# 6.0 "language feature" ... interesting, but isn't there a better way?
            //Type modelType = source?.GetType() ?? typeof (object);
            var modelType = source != null ? source.GetType() : typeof (object);
            var destinationType = typeof (TDestination);

            return (TDestination) DynamicMap(source, modelType, destinationType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public object DynamicMap(object source, Type sourceType, Type destinationType)
        {
            var typeMap = _mapperContext.ConfigurationProvider.ResolveTypeMap(source, null, sourceType, destinationType)
                          ?? _mapperContext.ConfigurationProvider.CreateTypeMap(sourceType, destinationType);

            var context = new ResolutionContext(typeMap, source, sourceType, destinationType,
                new MappingOperationOptions {CreateMissingTypeMaps = true}, _mapperContext);

            return Runner.Map(context);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        public void DynamicMap(object source, object destination, Type sourceType, Type destinationType)
        {
            var typeMap = _mapperContext.ConfigurationProvider.ResolveTypeMap(source, destination, sourceType,
                destinationType)
                          ?? _mapperContext.ConfigurationProvider.CreateTypeMap(sourceType, destinationType);

            var context = new ResolutionContext(typeMap, source, destination, sourceType, destinationType,
                new MappingOperationOptions{CreateMissingTypeMaps = true}, _mapperContext);

            Runner.Map(context);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="parentContext"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public TDestination Map<TSource, TDestination>(ResolutionContext parentContext, TSource source)
        {
            var destinationType = typeof (TDestination);
            var sourceType = typeof (TSource);
            var typeMap = _mapperContext.ConfigurationProvider.ResolveTypeMap(source, null, sourceType, destinationType);
            var context = parentContext.CreateTypeContext(typeMap, source, null, sourceType, destinationType);
            return (TDestination) ((IMappingEngineRunner) this).Map(context);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        object IMappingEngineRunner.Map(ResolutionContext context)
        {
            try
            {
                var contextTypePair = new TypePair(context.SourceType, context.DestinationType);

                Func<TypePair, IObjectMapper> missFunc =
                    tp => _mapperContext.ObjectMappers.FirstOrDefault(mapper => mapper.IsMatch(context));

                var mapperToUse = _objectMapperCache.GetOrAdd(contextTypePair, missFunc);

                if (mapperToUse == null)
                {
                    if (context.Options.CreateMissingTypeMaps)
                    {
                        var typeMap = _mapperContext.ConfigurationProvider.CreateTypeMap(context.SourceType, context.DestinationType);

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

                return mapperToUse.Map(context);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
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
                    return typeMap.ConstructorMap.ResolveValue(context);

            if (context.DestinationValue != null)
                return context.DestinationValue;

            if (destinationType.IsInterface())
                destinationType = ProxyGeneratorFactory.Create().GetProxyType(destinationType);

            return !_mapperContext.ConfigurationProvider.MapNullSourceValuesAsNull
                ? ObjectCreator.CreateNonNullValue(destinationType)
                : ObjectCreator.CreateObject(destinationType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool IMappingEngineRunner.ShouldMapSourceValueAsNull(ResolutionContext context)
        {
            if (context.DestinationType.IsValueType() && !context.DestinationType.IsNullableType())
                return false;

            var typeMap = context.GetContextTypeMap();
            if (typeMap != null)
                return _mapperContext.ConfigurationProvider.GetProfileConfiguration(typeMap.Profile).MapNullSourceValuesAsNull;

            return _mapperContext.ConfigurationProvider.MapNullSourceValuesAsNull;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool IMappingEngineRunner.ShouldMapSourceCollectionAsNull(ResolutionContext context)
        {
            var typeMap = context.GetContextTypeMap();
            if (typeMap != null)
                return _mapperContext.ConfigurationProvider.GetProfileConfiguration(typeMap.Profile).MapNullSourceCollectionsAsNull;

            return _mapperContext.ConfigurationProvider.MapNullSourceCollectionsAsNull;
        }

        private void ClearTypeMap(object sender, TypeMapCreatedEventArgs e)
        {
            IObjectMapper existing;

            _objectMapperCache.TryRemove(new TypePair(e.TypeMap.SourceType, e.TypeMap.DestinationType), out existing);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opts"></param>
        private void DefaultMappingOptions(IMappingOperationOptions opts)
        {
            opts.ConstructServicesUsing(_serviceCtor);
        }
    }
}