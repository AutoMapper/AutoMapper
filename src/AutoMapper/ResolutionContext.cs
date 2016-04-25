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
        /// Current source type
        /// </summary>
        public Type SourceType => Types.SourceType;

        /// <summary>
        /// Current attempted destination type
        /// </summary>
        public Type DestinationType => Types.DestinationType;

        /// <summary>
        /// Source value
        /// </summary>
        public object SourceValue { get; }

        /// <summary>
        /// Destination value
        /// </summary>
        public object DestinationValue { get; }

        /// <summary>
        /// Parent resolution context
        /// </summary>
        public ResolutionContext Parent { get; }

        /// <summary>
        /// Instance cache for resolving circular references
        /// </summary>
        public Dictionary<object, object> InstanceCache
        {
            get
            {
                if (_instanceCache != null)
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
        /// Current type map
        /// </summary>
        public TypeMap TypeMap { get; set; }

        /// <summary>
        /// Source and destination type pair
        /// </summary>
        public TypePair Types { get; }

        public bool IsSourceValueNull => Equals(null, SourceValue);

        public ResolutionContext(object source, object destination, TypePair types, ResolutionContext parent)
        {
            SourceValue = source;
            DestinationValue = destination;
            Parent = parent;
            Options = parent.Options;
            Mapper = parent.Mapper;

            _instanceCache = parent.InstanceCache;

            Types = types;
        }

        public ResolutionContext(object source, object destination, TypePair types, MappingOperationOptions options, IRuntimeMapper mapper)
        {
            SourceValue = source;
            DestinationValue = destination;
            Options = options;
            Mapper = mapper;
            Types = types;
        }

        public override string ToString() => $"Trying to map {SourceType.Name} to {DestinationType.Name}.";

        public TypeMap GetContextTypeMap()
        {
            TypeMap typeMap = TypeMap;
            ResolutionContext parent = Parent;
            while ((typeMap == null) && (parent != null))
            {
                typeMap = parent.TypeMap;
                parent = parent.Parent;
            }
            return typeMap;
        }

        public ResolutionContext[] GetContexts()
        {
            return GetContextsCore().Reverse().Distinct().ToArray();
        }

        protected IEnumerable<ResolutionContext> GetContextsCore()
        {
            var context = this;
            while (context.Parent != null)
            {
                yield return context;
                context = context.Parent;
            }
            yield return context;
        }

        public void BeforeMap(object destination)
        {
            if (Parent == null)
            {
                Options.BeforeMapAction(SourceValue, destination);
            }
        }

        public void AfterMap(object destination)
        {
            if (Parent == null)
            {
                Options.AfterMapAction(SourceValue, destination);
            }
        }
    }
}