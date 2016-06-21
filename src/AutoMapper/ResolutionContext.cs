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

        /// <summary>
        /// Mapping operation options
        /// </summary>
        public MappingOperationOptions Options { get; }

        /// <summary>
        /// Instance cache for resolving circular references
        /// </summary>
        public Dictionary<object, object> InstanceCache
        {
            get
            {
                if(_instanceCache != null)
                {
                    return _instanceCache;
                }
                _instanceCache = new Dictionary<object, object>();
                return _instanceCache;
            }
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

        public ResolutionContext(MappingOperationOptions options, IRuntimeMapper mapper)
        {
            Options = options;
            Mapper = mapper;
        }


        internal TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            var types = TypePair.Create(source, destination, typeof(TSource), typeof(TDestination));
            var a = Mapper.ConfigurationProvider.GetMapperFunc<TSource, TDestination>(types);
            return a(source, destination, this);
        }

        internal object Map(object source, object destination, Type sourceType, Type destinationType)
        {
            return Mapper.Map(source, destination, sourceType, destinationType, this);
        }
    }
}