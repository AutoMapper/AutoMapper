using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.Execution
{
    internal class RuntimeMapper : IRuntimeMapper
    {
        private readonly ResolutionContext _resolutionContext;
        private readonly IRuntimeMapper _inner;

        public RuntimeMapper(ResolutionContext resolutionContext, IRuntimeMapper inner)
        {
            _resolutionContext = resolutionContext;
            _inner = inner;
        }

        public IConfigurationProvider ConfigurationProvider => _inner.ConfigurationProvider;
        public Func<Type, object> ServiceCtor => _inner.ServiceCtor;
        public ResolutionContext DefaultContext => _inner.DefaultContext;

        public TDestination Map<TDestination>(object source)
            => (TDestination)_inner.Map(source, null, source.GetType(), typeof(TDestination), _resolutionContext);

        public TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> opts)
        {
            opts(_resolutionContext.Options);

            return Map<TDestination>(source);
        }

        public TDestination Map<TSource, TDestination>(TSource source)
            => _inner.Map(source, default(TDestination), _resolutionContext);

        public TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            var typedOptions = new MappingOperationOptions<TSource, TDestination>(_inner.ServiceCtor);

            opts(typedOptions);

            var destination = default(TDestination);

            typedOptions.BeforeMapAction(source, destination);

            destination = _inner.Map(source, destination, _resolutionContext);

            typedOptions.AfterMapAction(source, destination);

            return destination;
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
            => _inner.Map(source, destination, _resolutionContext);

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            var typedOptions = new MappingOperationOptions<TSource, TDestination>(_inner.ServiceCtor);

            opts(typedOptions);

            typedOptions.BeforeMapAction(source, destination);

            destination = _inner.Map(source, destination, _resolutionContext);

            typedOptions.AfterMapAction(source, destination);

            return destination;
        }

        public object Map(object source, Type sourceType, Type destinationType)
            => _inner.Map(source, null, sourceType, destinationType, _resolutionContext);

        public object Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            opts(_resolutionContext.Options);

            return Map(source, sourceType, destinationType);
        }

        public object Map(object source, object destination, Type sourceType, Type destinationType)
            => _inner.Map(source, destination, sourceType, destinationType, _resolutionContext);

        public object Map(object source, object destination, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            opts(_resolutionContext.Options);

            return Map(source, destination, sourceType, destinationType);
        }

        public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, object parameters = null, params Expression<Func<TDestination, object>>[] membersToExpand)
            => _inner.ProjectTo(source, parameters, membersToExpand);

        public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, IDictionary<string, object> parameters, params string[] membersToExpand)
            => _inner.ProjectTo<TDestination>(source, parameters, membersToExpand);

        public object Map(object source, object destination, Type sourceType, Type destinationType, ResolutionContext context, IMemberMap memberMap = null)
            => _inner.Map(source, destination, sourceType, destinationType, context, memberMap);

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination, ResolutionContext context, IMemberMap memberMap = null)
            => _inner.Map(source, destination, context, memberMap);
    }
}