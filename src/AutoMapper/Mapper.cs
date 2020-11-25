using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper
{
    using QueryableExtensions;
    using ObjectMappingOperationOptions = MappingOperationOptions<object, object>;
    using IObjectMappingOperationOptions = IMappingOperationOptions<object, object>;
    using Internal;

    public class Mapper : IMapper, IInternalRuntimeMapper
    {
        private readonly IGlobalConfiguration _configurationProvider;
        public Mapper(IConfigurationProvider configurationProvider) : this(configurationProvider, configurationProvider.Internal().ServiceCtor) {}
        public Mapper(IConfigurationProvider configurationProvider, Func<Type, object> serviceCtor)
        {
            _configurationProvider = (IGlobalConfiguration)configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            DefaultContext = new ResolutionContext(new ObjectMappingOperationOptions(serviceCtor ?? throw new NullReferenceException(nameof(serviceCtor))), this);
        }
        internal ResolutionContext DefaultContext { get; }
        ResolutionContext IInternalRuntimeMapper.DefaultContext => DefaultContext;
        public IConfigurationProvider ConfigurationProvider => _configurationProvider;

        public TDestination Map<TDestination>(object source) => Map(source, default(TDestination));

        public TDestination Map<TDestination>(object source, Action<IMappingOperationOptions<object, TDestination>> opts) => Map(source, default, opts);

        public TDestination Map<TSource, TDestination>(TSource source) => Map(source, default(TDestination));

        public TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions<TSource, TDestination>> opts) =>
            Map(source, default, opts);

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) =>
            MapCore(source, destination, DefaultContext);

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions<TSource, TDestination>> opts) =>
            MapWithOptions(source, destination, opts);

        public object Map(object source, Type sourceType, Type destinationType) => Map(source, null, sourceType, destinationType);

        public object Map(object source, Type sourceType, Type destinationType, Action<IObjectMappingOperationOptions> opts) =>
            Map(source, null, sourceType, destinationType, opts);

        public object Map(object source, object destination, Type sourceType, Type destinationType) =>
            MapCore(source, destination, DefaultContext, sourceType, destinationType);

        public object Map(object source, object destination, Type sourceType, Type destinationType, Action<IObjectMappingOperationOptions> opts) =>
            MapWithOptions(source, destination, opts, sourceType, destinationType);

        public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, object parameters, params Expression<Func<TDestination, object>>[] membersToExpand)
            => source.ProjectTo(ConfigurationProvider, parameters, membersToExpand);

        public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, IDictionary<string, object> parameters, params string[] membersToExpand)
            => source.ProjectTo<TDestination>(ConfigurationProvider, parameters, membersToExpand);

        public IQueryable ProjectTo(IQueryable source, Type destinationType, IDictionary<string, object> parameters, params string[] membersToExpand)
            => source.ProjectTo(destinationType, ConfigurationProvider, parameters, membersToExpand);

        TDestination IInternalRuntimeMapper.Map<TSource, TDestination>(TSource source, TDestination destination,
            ResolutionContext context, Type sourceType, Type destinationType, IMemberMap memberMap) =>
            MapCore(source, destination, context, sourceType, destinationType, memberMap);

        private TDestination MapWithOptions<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions<TSource, TDestination>> opts,
            Type sourceType = null, Type destinationType = null)
        {
            var typedOptions = new MappingOperationOptions<TSource, TDestination>(DefaultContext.Options.ServiceCtor);
            opts(typedOptions);
            typedOptions.BeforeMapAction?.Invoke(source, destination);
            destination = MapCore(source, destination, new ResolutionContext(typedOptions, this), sourceType, destinationType);
            typedOptions.AfterMapAction?.Invoke(source, destination);
            return destination;
        }

        private TDestination MapCore<TSource, TDestination>(
            TSource source, TDestination destination, ResolutionContext context, Type sourceType = null, Type destinationType = null, IMemberMap memberMap = null)
        {
            var runtimeTypes = new TypePair(source?.GetType() ?? sourceType ?? typeof(TSource), destination?.GetType() ?? destinationType ?? typeof(TDestination));
            var requestedTypes = new TypePair(typeof(TSource), typeof(TDestination));
            var mapRequest = new MapRequest(requestedTypes, runtimeTypes, memberMap);
            return _configurationProvider.GetExecutionPlan<TSource, TDestination>(mapRequest)(source, destination, context);
        }
    }
}