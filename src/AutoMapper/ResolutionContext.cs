using System;
using System.Collections.Generic;

namespace AutoMapper
{
    /// <summary>
    /// Context information regarding resolution of a destination value
    /// </summary>
    public class ResolutionContext
    {
        private Dictionary<ContextCacheKey, object> _instanceCache;
        private Dictionary<TypePair, int> _typeDepth;

        /// <summary>
        /// Mapping operation options
        /// </summary>
        public IMappingOperationOptions Options { get; }

        internal object GetDestination(object source, Type destinationType)
        {
            InstanceCache.TryGetValue(new ContextCacheKey(source, destinationType), out object destination);
            return destination;
        }

        internal void CacheDestination(object source, Type destinationType, object destination)
        {
            InstanceCache[new ContextCacheKey(source, destinationType)] = destination;
        }

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

        private void CheckDefault()
        {
            if(IsDefault)
            {
                throw new InvalidOperationException();
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

        internal void IncrementTypeDepth(in TypePair types)
        {
            TypeDepth[types]++;
        }

        internal void DecrementTypeDepth(in TypePair types)
        {
            TypeDepth[types]--;
        }

        internal int GetTypeDepth(in TypePair types)
        {
            if (!TypeDepth.ContainsKey(types))
                TypeDepth[types] = 1;

            return TypeDepth[types];
        }

        /// <summary>
        /// Current mapper
        /// </summary>
        public IRuntimeMapper Mapper { get; }

        /// <summary>
        /// Current configuration
        /// </summary>
        public IConfigurationProvider ConfigurationProvider => Mapper.ConfigurationProvider;

        /// <summary>
        /// Context items from <see cref="Options"/>
        /// </summary>
        public IDictionary<string, object> Items => Options.Items;

        public ResolutionContext(IMappingOperationOptions options, IRuntimeMapper mapper)
        {
            Options = options;
            Mapper = mapper;
        }

        internal bool IsDefault => this == Mapper.DefaultContext;

        internal TDestination Map<TSource, TDestination>(TSource source, TDestination destination, PropertyMap propertyMap)
            => Mapper.Map(source, destination, this, propertyMap);

        internal object Map(object source, object destination, Type sourceType, Type destinationType, PropertyMap propertyMap) 
            => Mapper.Map(source, destination, sourceType, destinationType, this, propertyMap);

        internal void ValidateMap(TypeMap typeMap)
            => ConfigurationProvider.AssertConfigurationIsValid(typeMap);
    }

    public struct ContextCacheKey : IEquatable<ContextCacheKey>
    {
        public static bool operator ==(ContextCacheKey left, ContextCacheKey right) => left.Equals(right);
        public static bool operator !=(ContextCacheKey left, ContextCacheKey right) => !left.Equals(right);

        private readonly object _source;
        private readonly Type _destinationType;

        public ContextCacheKey(object source, Type destinationType)
        {
            _source = source;
            _destinationType = destinationType;
        }

        public override int GetHashCode() => HashCodeCombiner.Combine(_source, _destinationType);

        public bool Equals(ContextCacheKey other) =>
            _source == other._source && _destinationType == other._destinationType;

        public override bool Equals(object other) => 
            other is ContextCacheKey && Equals((ContextCacheKey)other);
    }
}