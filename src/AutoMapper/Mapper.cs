using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper
{
    using QueryableExtensions;
    using ObjectMappingOperationOptions = MappingOperationOptions<object, object>;

    public class Mapper : IMapper, IInternalRuntimeMapper
    {
        public Mapper(IConfigurationProvider configurationProvider)
            : this(configurationProvider, configurationProvider.ServiceCtor)
        {
        }

        public Mapper(IConfigurationProvider configurationProvider, Func<Type, object> serviceCtor)
        {
            ConfigurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            ServiceCtor = serviceCtor ?? throw new ArgumentNullException(nameof(serviceCtor));
            DefaultContext = new ResolutionContext(new ObjectMappingOperationOptions(serviceCtor), this);
        }

        internal ResolutionContext DefaultContext { get; }

        ResolutionContext IInternalRuntimeMapper.DefaultContext => DefaultContext; 

        public Func<Type, object> ServiceCtor { get; }

        public IConfigurationProvider ConfigurationProvider { get; }

        public TDestination Map<TDestination>(object source) => Map<object, TDestination>(source);

        public TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> opts) => Map(source, default(TDestination), opts);

        public TDestination Map<TSource, TDestination>(TSource source) => Map(source, default(TDestination));

        public TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions<TSource, TDestination>> opts) =>
            Map(source, default(TDestination), opts);

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) =>
            ((IInternalRuntimeMapper)this).Map(source, destination, DefaultContext);

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            var types = TypePair.Create(source, destination, typeof(TSource), typeof(TDestination));
            var key = new TypePair(typeof(TSource), typeof(TDestination));

            var typedOptions = new MappingOperationOptions<TSource, TDestination>(ServiceCtor);

            opts(typedOptions);

            var mapRequest = new MapRequest(key, types);

            var func = ConfigurationProvider.GetMapperFunc<TSource, TDestination>(mapRequest);

            typedOptions.BeforeMapAction(source, destination);

            var context = new ResolutionContext(typedOptions, this);

            destination = func(source, destination, context);

            typedOptions.AfterMapAction(source, destination);

            return destination;
        }

        public object Map(object source, Type sourceType, Type destinationType) => Map(source, null, sourceType, destinationType);

        public object Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts) =>
            Map(source, null, sourceType, destinationType, opts);

        public object Map(object source, object destination, Type sourceType, Type destinationType) =>
            ((IInternalRuntimeMapper)this).Map(source, destination, sourceType, destinationType, DefaultContext);

        public object Map(object source, object destination, Type sourceType, Type destinationType,
            Action<IMappingOperationOptions> opts)
        {
            var types = TypePair.Create(source, destination, sourceType, destinationType);

            var options = new ObjectMappingOperationOptions(ServiceCtor);

            opts(options);

            var func = ConfigurationProvider.GetUntypedMapperFunc(new MapRequest(new TypePair(sourceType, destinationType), types));

            options.BeforeMapAction(source, destination);

            var context = new ResolutionContext(options, this);

            destination = func(source, destination, context);

            options.AfterMapAction(source, destination);

            return destination;
        }

        object IInternalRuntimeMapper.Map(object source, object destination, Type sourceType, Type destinationType,
            ResolutionContext context, IMemberMap memberMap)
        {
            var types = TypePair.Create(source, destination, sourceType, destinationType);

            var func = ConfigurationProvider.GetUntypedMapperFunc(new MapRequest(new TypePair(sourceType, destinationType), types, memberMap));

            return func(source, destination, context);
        }

        TDestination IInternalRuntimeMapper.Map<TSource, TDestination>(TSource source, TDestination destination,
            ResolutionContext context, IMemberMap memberMap)
        {
            var types = TypePair.Create(source, destination, typeof(TSource), typeof(TDestination));

            var func = ConfigurationProvider.GetMapperFunc<TSource, TDestination>(types, memberMap);

            return func(source, destination, context);
        }

        public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, object parameters, params Expression<Func<TDestination, object>>[] membersToExpand)
            => source.ProjectTo(ConfigurationProvider, parameters, membersToExpand);

        public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, IDictionary<string, object> parameters, params string[] membersToExpand)
            => source.ProjectTo<TDestination>(ConfigurationProvider, parameters, membersToExpand);

        public IQueryable ProjectTo(IQueryable source, Type destinationType, IDictionary<string, object> parameters, params string[] membersToExpand)
            => source.ProjectTo(destinationType, ConfigurationProvider, parameters, membersToExpand);
    }
}