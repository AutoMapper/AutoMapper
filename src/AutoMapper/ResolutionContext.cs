using AutoMapper.Internal;
using System;
using System.Collections.Generic;

namespace AutoMapper
{
    /// <summary>
    /// Context information regarding resolution of a destination value
    /// </summary>
    public class ResolutionContext : IInternalRuntimeMapper
    {
        private Dictionary<ContextCacheKey, object> _instanceCache;
        private Dictionary<TypePair, int> _typeDepth;
        private readonly IInternalRuntimeMapper _inner;

        internal ResolutionContext(IMappingOperationOptions options, IInternalRuntimeMapper mapper)
        {
            Options = options;
            _inner = mapper;
        }

        internal ResolutionContext(IInternalRuntimeMapper mapper) : this(mapper.DefaultContext.Options, mapper) { }

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

        ResolutionContext IInternalRuntimeMapper.DefaultContext => _inner.DefaultContext;

        /// <summary>
        /// Instance cache for resolving circular references
        /// </summary>
        public Dictionary<ContextCacheKey, object> InstanceCache
        {
            get
            {
                CheckDefault();
                return _instanceCache ??= new Dictionary<ContextCacheKey, object>(); ;
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
                return _typeDepth ??= new Dictionary<TypePair, int>();
            }
        }

        TDestination IMapperBase.Map<TDestination>(object source) => ((IMapperBase)this).Map(source, default(TDestination));
        TDestination IMapperBase.Map<TSource, TDestination>(TSource source)
            => _inner.Map(source, default(TDestination), this);
        TDestination IMapperBase.Map<TSource, TDestination>(TSource source, TDestination destination)
            => _inner.Map(source, destination, this);
        object IMapperBase.Map(object source, Type sourceType, Type destinationType)
            => _inner.Map(source, (object)null, this, sourceType, destinationType);
        object IMapperBase.Map(object source, object destination, Type sourceType, Type destinationType)
            => _inner.Map(source, destination, this, sourceType, destinationType);
        TDestination IInternalRuntimeMapper.Map<TSource, TDestination>(TSource source, TDestination destination, ResolutionContext context,
            Type sourceType, Type destinationType, IMemberMap memberMap)
            => _inner.Map(source, destination, context, sourceType, destinationType, memberMap);

        internal object GetDestination(object source, Type destinationType) => InstanceCache.GetOrDefault(new ContextCacheKey(source, destinationType));

        internal void CacheDestination(object source, Type destinationType, object destination) => 
            InstanceCache[new ContextCacheKey(source, destinationType)] = destination;

        internal void IncrementTypeDepth(TypePair types) => TypeDepth[types]++;

        internal void DecrementTypeDepth(TypePair types) => TypeDepth[types]--;

        internal int GetTypeDepth(TypePair types) => TypeDepth.TryGetValue(types, out int depth) ? depth : TypeDepth[types] = 1;

        internal bool IsDefault => this == _inner.DefaultContext;

        internal TDestination Map<TSource, TDestination>(TSource source, TDestination destination, IMemberMap memberMap)
            => _inner.Map(source, destination, this, memberMap: memberMap);

        internal object Map(object source, object destination, Type sourceType, Type destinationType, IMemberMap memberMap)
            => _inner.Map(source, destination, this, sourceType, destinationType, memberMap);

        private void CheckDefault()
        {
            if (IsDefault)
            {
                throw new InvalidOperationException("You must use a Map overload that takes Action<IMappingOperationOptions>!");
            }
        }
    }
    public readonly struct ContextCacheKey : IEquatable<ContextCacheKey>
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
        public bool Equals(ContextCacheKey other) => Source == other.Source && _destinationType == other._destinationType;
        public override bool Equals(object other) => other is ContextCacheKey otherKey && Equals(otherKey);
    }
}