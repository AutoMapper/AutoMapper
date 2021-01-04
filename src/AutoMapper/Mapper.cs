using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
namespace AutoMapper
{
    using QueryableExtensions;
    using IObjectMappingOperationOptions = IMappingOperationOptions<object, object>;
    using Internal;
    public interface IMapperBase
    {
        /// <summary>
        /// Execute a mapping from the source object to a new destination object.
        /// The source type is inferred from the source object.
        /// </summary>
        /// <typeparam name="TDestination">Destination type to create</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <returns>Mapped destination object</returns>
        TDestination Map<TDestination>(object source);
        /// <summary>
        /// Execute a mapping from the source object to a new destination object.
        /// </summary>
        /// <typeparam name="TSource">Source type to use, regardless of the runtime type</typeparam>
        /// <typeparam name="TDestination">Destination type to create</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <returns>Mapped destination object</returns>
        TDestination Map<TSource, TDestination>(TSource source);
        /// <summary>
        /// Execute a mapping from the source object to the existing destination object.
        /// </summary>
        /// <typeparam name="TSource">Source type to use</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination">Destination object to map into</param>
        /// <returns>The mapped destination object, same instance as the <paramref name="destination"/> object</returns>
        TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
        /// <summary>
        /// Execute a mapping from the source object to a new destination object with explicit <see cref="System.Type"/> objects
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to create</param>
        /// <returns>Mapped destination object</returns>
        object Map(object source, Type sourceType, Type destinationType);
        /// <summary>
        /// Execute a mapping from the source object to existing destination object with explicit <see cref="System.Type"/> objects
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination">Destination object to map into</param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to use</param>
        /// <returns>Mapped destination object, same instance as the <paramref name="destination"/> object</returns>
        object Map(object source, object destination, Type sourceType, Type destinationType);
    }
    public interface IMapper : IMapperBase
    {
        /// <summary>
        /// Execute a mapping from the source object to a new destination object with supplied mapping options.
        /// </summary>
        /// <typeparam name="TDestination">Destination type to create</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>Mapped destination object</returns>
        TDestination Map<TDestination>(object source, Action<IMappingOperationOptions<object, TDestination>> opts);
        /// <summary>
        /// Execute a mapping from the source object to a new destination object with supplied mapping options.
        /// </summary>
        /// <typeparam name="TSource">Source type to use</typeparam>
        /// <typeparam name="TDestination">Destination type to create</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>Mapped destination object</returns>
        TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions<TSource, TDestination>> opts);
        /// <summary>
        /// Execute a mapping from the source object to the existing destination object with supplied mapping options.
        /// </summary>
        /// <typeparam name="TSource">Source type to use</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination">Destination object to map into</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>The mapped destination object, same instance as the <paramref name="destination"/> object</returns>
        TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions<TSource, TDestination>> opts);
        /// <summary>
        /// Execute a mapping from the source object to a new destination object with explicit <see cref="System.Type"/> objects and supplied mapping options.
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to create</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>Mapped destination object</returns>
        object Map(object source, Type sourceType, Type destinationType, Action<IObjectMappingOperationOptions> opts);
        /// <summary>
        /// Execute a mapping from the source object to existing destination object with supplied mapping options and explicit <see cref="System.Type"/> objects
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination">Destination object to map into</param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to use</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>Mapped destination object, same instance as the <paramref name="destination"/> object</returns>
        object Map(object source, object destination, Type sourceType, Type destinationType, Action<IObjectMappingOperationOptions> opts);
        /// <summary>
        /// Configuration provider for performing maps
        /// </summary>
        IConfigurationProvider ConfigurationProvider { get; }
        /// <summary>
        /// Project the input queryable.
        /// </summary>
        /// <remarks>Projections are only calculated once and cached</remarks>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="source">Queryable source</param>
        /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
        /// <param name="membersToExpand">Explicit members to expand</param>
        /// <returns>Queryable result, use queryable extension methods to project and execute result</returns>
        IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, object parameters = null, params Expression<Func<TDestination, object>>[] membersToExpand);
        /// <summary>
        /// Project the input queryable.
        /// </summary>
        /// <typeparam name="TDestination">Destination type to map to</typeparam>
        /// <param name="source">Queryable source</param>
        /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
        /// <param name="membersToExpand">Explicit members to expand</param>
        /// <returns>Queryable result, use queryable extension methods to project and execute result</returns>
        IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, IDictionary<string, object> parameters, params string[] membersToExpand);
        /// <summary>
        /// Project the input queryable.
        /// </summary>
        /// <param name="source">Queryable source</param>
        /// <param name="destinationType">Destination type to map to</param>
        /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
        /// <param name="membersToExpand">Explicit members to expand</param>
        /// <returns>Queryable result, use queryable extension methods to project and execute result</returns>
        IQueryable ProjectTo(IQueryable source, Type destinationType, IDictionary<string, object> parameters = null, params string[] membersToExpand);
    }
    public interface IRuntimeMapper : IMapperBase
    {
    }
    internal interface IInternalRuntimeMapper : IRuntimeMapper
    {
        TDestination Map<TSource, TDestination>(TSource source, TDestination destination, ResolutionContext context, Type sourceType = null, Type destinationType = null, MemberMap memberMap = null);
        ResolutionContext DefaultContext { get; }
    }
    public class Mapper : IMapper, IInternalRuntimeMapper
    {
        private readonly IGlobalConfiguration _configurationProvider;
        public Mapper(IConfigurationProvider configurationProvider) : this(configurationProvider, configurationProvider.Internal().ServiceCtor) { }
        public Mapper(IConfigurationProvider configurationProvider, Func<Type, object> serviceCtor)
        {
            _configurationProvider = (IGlobalConfiguration)configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            DefaultContext = new ResolutionContext(new MappingOperationOptions<object, object>(serviceCtor ?? throw new NullReferenceException(nameof(serviceCtor))), this);
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
            ResolutionContext context, Type sourceType, Type destinationType, MemberMap memberMap) =>
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
            TSource source, TDestination destination, ResolutionContext context, Type sourceType = null, Type destinationType = null, MemberMap memberMap = null)
        {
            var runtimeTypes = new TypePair(source?.GetType() ?? sourceType ?? typeof(TSource), destination?.GetType() ?? destinationType ?? typeof(TDestination));
            var requestedTypes = new TypePair(typeof(TSource), typeof(TDestination));
            var mapRequest = new MapRequest(requestedTypes, runtimeTypes, memberMap);
            return _configurationProvider.GetExecutionPlan<TSource, TDestination>(mapRequest)(source, destination, context);
        }
    }
}