using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Execution;

namespace AutoMapper
{
    /// <summary>
    /// Context information regarding resolution of a destination value
    /// </summary>
    public class ResolutionContext : IRuntimeMapper
    {
        private Dictionary<ContextCacheKey, object> _instanceCache;
        private Dictionary<TypePair, int> _typeDepth;
        private readonly IRuntimeMapper _inner;

        public ResolutionContext(IMappingOperationOptions options, IRuntimeMapper mapper)
        {
            Options = options;
            _inner = mapper;
        }

        /// <summary>
        /// Mapping operation options
        /// </summary>
        public IMappingOperationOptions Options { get; }

        /// <summary>
        /// Context items from <see cref="Options"/>
        /// </summary>
        public IDictionary<string, object> Items
        {
            get
            {
                CheckDefault();
                return Options.Items;
            }
        }

        /// <summary>
        /// Current mapper
        /// </summary>
        public IRuntimeMapper Mapper => this;

        public IConfigurationProvider ConfigurationProvider => _inner.ConfigurationProvider;

        Func<Type, object> IMapper.ServiceCtor => _inner.ServiceCtor;

        ResolutionContext IRuntimeMapper.DefaultContext => _inner.DefaultContext;

        /// <summary>
        /// Instance cache for resolving circular references
        /// </summary>
        public Dictionary<ContextCacheKey, object> InstanceCache
        {
            get
            {
                CheckDefault();
                if(_instanceCache != null)
                {
                    return _instanceCache;
                }
                _instanceCache = new Dictionary<ContextCacheKey, object>();
                return _instanceCache;
            }
        }

        /// <summary>
        /// Instance cache for resolving keeping track of depth
        /// </summary>
        private Dictionary<TypePair, int> TypeDepth
        {
            get
            {
                CheckDefault();
                if(_typeDepth != null)
                {
                    return _typeDepth;
                }
                _typeDepth = new Dictionary<TypePair, int>();
                return _typeDepth;
            }
        }

        TDestination IMapper.Map<TDestination>(object source)
            => (TDestination)_inner.Map(source, null, source?.GetType() ?? typeof(object), typeof(TDestination), this);

        TDestination IRuntimeMapper.Map<TDestination>(object source, Action<IMappingOperationOptions> opts) =>
            ((IMapper)this).Map<TDestination>(source, opts);

        TDestination IMapper.Map<TDestination>(object source, Action<IMappingOperationOptions> opts)
        {
            opts(Options);

            return ((IMapper)this).Map<TDestination>(source);
        }

        TDestination IMapper.Map<TSource, TDestination>(TSource source)
            => _inner.Map(source, default(TDestination), this);

        TDestination IRuntimeMapper.Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions<TSource, TDestination>> opts) =>
            ((IMapper)this).Map<TSource, TDestination>(source, opts);

        TDestination IMapper.Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            var typedOptions = new MappingOperationOptions<TSource, TDestination>(_inner.ServiceCtor);

            opts(typedOptions);

            var destination = default(TDestination);

            typedOptions.BeforeMapAction(source, destination);

            destination = _inner.Map(source, destination, this);

            typedOptions.AfterMapAction(source, destination);

            return destination;
        }

        TDestination IMapper.Map<TSource, TDestination>(TSource source, TDestination destination)
            => _inner.Map(source, destination, this);

        TDestination IRuntimeMapper.Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions<TSource, TDestination>> opts) =>
            ((IMapper)this).Map<TSource, TDestination>(source, destination, opts);

        TDestination IMapper.Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions<TSource, TDestination>> opts)
        {
            var typedOptions = new MappingOperationOptions<TSource, TDestination>(_inner.ServiceCtor);

            opts(typedOptions);

            typedOptions.BeforeMapAction(source, destination);

            destination = _inner.Map(source, destination, this);

            typedOptions.AfterMapAction(source, destination);

            return destination;
        }

        object IMapper.Map(object source, Type sourceType, Type destinationType)
            => _inner.Map(source, null, sourceType, destinationType, this);

        object IRuntimeMapper.Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts) =>
            ((IMapper)this).Map(source, sourceType, destinationType, opts);

        object IMapper.Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            opts(Options);

            return ((IMapper)this).Map(source, sourceType, destinationType);
        }

        object IMapper.Map(object source, object destination, Type sourceType, Type destinationType)
            => _inner.Map(source, destination, sourceType, destinationType, this);

        object IRuntimeMapper.Map(object source, object destination, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts) =>
            ((IMapper)this).Map(source, destination, sourceType, destinationType, opts);

        object IMapper.Map(object source, object destination, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            opts(Options);

            return ((IMapper)this).Map(source, destination, sourceType, destinationType);
        }

        object IRuntimeMapper.Map(object source, object destination, Type sourceType, Type destinationType, ResolutionContext context,
            IMemberMap memberMap)
            => _inner.Map(source, destination, sourceType, destinationType, context, memberMap);

        TDestination IRuntimeMapper.Map<TSource, TDestination>(TSource source, TDestination destination, ResolutionContext context,
            IMemberMap memberMap)
            => _inner.Map(source, destination, context, memberMap);

        IQueryable<TDestination> IMapper.ProjectTo<TDestination>(IQueryable source, object parameters, params Expression<Func<TDestination, object>>[] membersToExpand)
            => _inner.ProjectTo(source, parameters, membersToExpand);

        IQueryable<TDestination> IMapper.ProjectTo<TDestination>(IQueryable source, IDictionary<string, object> parameters, params string[] membersToExpand)
            => _inner.ProjectTo<TDestination>(source, parameters, membersToExpand);

        IQueryable IMapper.ProjectTo(IQueryable source, Type destinationType, IDictionary<string, object> parameters, params string[] membersToExpand) 
            => _inner.ProjectTo(source, destinationType, parameters, membersToExpand);

        internal object GetDestination(object source, Type destinationType)
        {
            InstanceCache.TryGetValue(new ContextCacheKey(source, destinationType), out object destination);
            return destination;
        }

        internal void CacheDestination(object source, Type destinationType, object destination)
        {
            InstanceCache[new ContextCacheKey(source, destinationType)] = destination;
        }

        internal void IncrementTypeDepth(TypePair types)
        {
            TypeDepth[types]++;
        }

        internal void DecrementTypeDepth(TypePair types)
        {
            TypeDepth[types]--;
        }

        internal int GetTypeDepth(TypePair types)
        {
            if (!TypeDepth.ContainsKey(types))
                TypeDepth[types] = 1;

            return TypeDepth[types];
        }

        internal void ValidateMap(TypeMap typeMap)
            => ConfigurationProvider.AssertConfigurationIsValid(typeMap);

        internal bool IsDefault => this == _inner.DefaultContext;

        internal TDestination Map<TSource, TDestination>(TSource source, TDestination destination, IMemberMap memberMap)
            => _inner.Map(source, destination, this, memberMap);

        internal object Map(object source, object destination, Type sourceType, Type destinationType, IMemberMap memberMap)
            => _inner.Map(source, destination, sourceType, destinationType, this, memberMap);

        private void CheckDefault()
        {
            if (IsDefault)
            {
                throw new InvalidOperationException("You must use a Map overload that takes Action<IMappingOperationOptions>!");
            }
        }
    }

    public struct ContextCacheKey : IEquatable<ContextCacheKey>
    {
        public static bool operator ==(ContextCacheKey left, ContextCacheKey right) => left.Equals(right);
        public static bool operator !=(ContextCacheKey left, ContextCacheKey right) => !left.Equals(right);
        private readonly Type _destinationType;

        public ContextCacheKey(object source, Type destinationType)
        {
            Source = source;
            _destinationType = destinationType;
        }

        public object Source { get; }

        public override int GetHashCode() => HashCodeCombiner.Combine(Source, _destinationType);

        public bool Equals(ContextCacheKey other) =>
            Source == other.Source && _destinationType == other._destinationType;

        public override bool Equals(object other) => 
            other is ContextCacheKey && Equals((ContextCacheKey)other);
    }
}