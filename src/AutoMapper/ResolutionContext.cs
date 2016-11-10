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
        private Dictionary<object, object> _instanceCache;
        private Dictionary<TypePair, int> _typeDepth;

        /// <summary>
        /// Mapping operation options
        /// </summary>
        public IMappingOperationOptions Options { get; }

        /// <summary>
        /// Instance cache for resolving circular references
        /// </summary>
        public Dictionary<object, object> InstanceCache
        {
            get
            {
                CheckDefault();
                if(_instanceCache != null)
                {
                    return _instanceCache;
                }
                _instanceCache = new Dictionary<object, object>();
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
}