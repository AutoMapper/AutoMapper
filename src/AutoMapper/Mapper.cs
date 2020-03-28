using System;

namespace AutoMapper
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using ObjectMappingOperationOptions = MappingOperationOptions<object, object>;
    using QueryableExtensions;

    public class Mapper : IRuntimeMapper
    {
        internal const string NoContextMapperOptions = "Set options in the outer Map call instead.";

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

        public ResolutionContext DefaultContext { get; }

        public Func<Type, object> ServiceCtor { get; }

        public IConfigurationProvider ConfigurationProvider { get; }

        public TDestination Map<TDestination>(object source)
        {
            var types = new TypePair(source?.GetType() ?? typeof(object), typeof(TDestination));

            var func = ConfigurationProvider.GetUntypedMapperFunc(new MapRequest(types, types));

            return (TDestination) func(source, null, DefaultContext);
        }

        public TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> opts)
        {
            var sourceType = source?.GetType() ?? typeof(object);
            var destinationType = typeof(TDestination);

            var mappedObject = (TDestination)((IMapper)this).Map(source, sourceType, destinationType, opts);
            return mappedObject;
        }

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            var types = TypePair.Create(source, typeof(TSource), typeof (TDestination));

            var func = ConfigurationProvider.GetMapperFunc<TSource, TDestination>(types);

            var destination = default(TDestination);

            return func(source, destination, DefaultContext);
        }

        public TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            var types = TypePair.Create(source, typeof(TSource), typeof(TDestination));

            var key = new TypePair(typeof(TSource), typeof(TDestination));

            var typedOptions = new MappingOperationOptions<TSource, TDestination>(ServiceCtor);

            opts(typedOptions);

            var mapRequest = new MapRequest(key, types);

            var func = ConfigurationProvider.GetMapperFunc<TSource, TDestination>(mapRequest);

            var destination = default(TDestination);

            typedOptions.BeforeMapAction(source, destination);

            var context = new ResolutionContext(typedOptions, this);

            destination = func(source, destination, context);

            typedOptions.AfterMapAction(source, destination);

            return destination;
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            var types = TypePair.Create(source, destination, typeof(TSource), typeof(TDestination));

            var func = ConfigurationProvider.GetMapperFunc<TSource, TDestination>(types);

            return func(source, destination, DefaultContext);
        }

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

        public object Map(object source, Type sourceType, Type destinationType)
        {
            var types = TypePair.Create(source, sourceType, destinationType);

            var func = ConfigurationProvider.GetUntypedMapperFunc(new MapRequest(new TypePair(sourceType, destinationType), types));

            return func(source, null, DefaultContext);
        }

        public object Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            var types = TypePair.Create(source, sourceType, destinationType);

            var options = new ObjectMappingOperationOptions(ServiceCtor);

            opts(options);

            var func = ConfigurationProvider.GetUntypedMapperFunc(new MapRequest(new TypePair(sourceType, destinationType), types));

            options.BeforeMapAction(source, null);

            var context = new ResolutionContext(options, this);

            var destination = func(source, null, context);

            options.AfterMapAction(source, destination);

            return destination;
        }

        public object Map(object source, object destination, Type sourceType, Type destinationType)
        {
            var types = TypePair.Create(source, destination, sourceType, destinationType);

            var func = ConfigurationProvider.GetUntypedMapperFunc(new MapRequest(new TypePair(sourceType, destinationType), types));

            return func(source, destination, DefaultContext);
        }

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

        public object Map(object source, object destination, Type sourceType, Type destinationType,
            ResolutionContext context, IMemberMap memberMap)
        {
            var types = TypePair.Create(source, destination, sourceType, destinationType);

            var func = ConfigurationProvider.GetUntypedMapperFunc(new MapRequest(new TypePair(sourceType, destinationType), types, memberMap));

            return func(source, destination, context);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination,
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