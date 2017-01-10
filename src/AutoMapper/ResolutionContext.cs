namespace AutoMapper
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Configuration;

    /// <summary>
    /// Context information regarding resolution of a destination value
    /// </summary>
    public class ResolutionContext
    {
        private Dictionary<SourceDestinationType, object> _instanceCache;
        private Dictionary<TypePair, int> _typeDepth;

        /// <summary>
        /// Mapping operation options
        /// </summary>
        public IMappingOperationOptions Options { get; }

        internal object GetDestination(object source, Type destinationType)
        {
            object destination;
            if(InstanceCache.TryGetValue(new SourceDestinationType(source, destinationType), out destination))
            {
                return destination;
            }
            return null;
        }

        internal void CacheDestination(object source, Type destinationType, object destination)
        {
            InstanceCache.Add(new SourceDestinationType(source, destinationType), destination);
        }

        /// <summary>
        /// Instance cache for resolving circular references
        /// </summary>
        public Dictionary<SourceDestinationType, object> InstanceCache
        {
            get
            {
                CheckDefault();
                if(_instanceCache != null)
                {
                    return _instanceCache;
                }
                _instanceCache = new Dictionary<SourceDestinationType, object>();
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

        internal TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            var types = TypePair.Create(source, destination, typeof(TSource), typeof(TDestination));
            var mapperFunc = Mapper.ConfigurationProvider.GetMapperFunc<TSource, TDestination>(types);
            return mapperFunc(source, destination, this);
        }

        internal object Map(object source, object destination, Type sourceType, Type destinationType)
        {
            return Mapper.Map(source, destination, sourceType, destinationType, this);
        }
    }

    public struct SourceDestinationType : IEquatable<SourceDestinationType>
    {
        public static bool operator ==(SourceDestinationType left, SourceDestinationType right) => left.Equals(right);
        public static bool operator !=(SourceDestinationType left, SourceDestinationType right) => !left.Equals(right);

        private static int CombineHashCodes(object obj1, object obj2)
        {
            return CombineHashCodes(obj1.GetHashCode(), obj2.GetHashCode());
        }

        private static int CombineHashCodes(int h1, int h2)
        {
            return (h1 << 5) + h1 ^ h2;
        }

        private readonly int _hashCode;
        private readonly object _source;
        private readonly Type _destinationType;

        public SourceDestinationType(object source, Type destinationType)
        {
            _source = source;
            _destinationType = destinationType;
            _hashCode = CombineHashCodes(_source, _destinationType);
        }

        public override int GetHashCode() => _hashCode;

        public bool Equals(SourceDestinationType other) =>
            _source == other._source && _destinationType == other._destinationType;

        public override bool Equals(object other) => 
            other is SourceDestinationType && Equals((SourceDestinationType)other);
    }
}