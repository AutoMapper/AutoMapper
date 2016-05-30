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
        public object SourceValue { get; private set; }

        /// <summary>
        /// Destination value
        /// </summary>
        public object DestinationValue { get; private set; }

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
        /// Current type map
        /// </summary>
        public TypeMap TypeMap { get; set; }

        /// <summary>
        /// Source and destination type pair
        /// </summary>
        public TypePair Types { get; private set; }

        public bool IsSourceValueNull => Equals(null, SourceValue);

        /// <summary>
        /// Context items from <see cref="Options"/>
        /// </summary>
        public IDictionary<string, object> Items => Options.Items;

        public ResolutionContext(object source, object destination, TypePair types, ResolutionContext parent) : this(parent)
        {
            SourceValue = source;
            DestinationValue = destination;
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

        internal ResolutionContext(ResolutionContext parent)
        {
            Parent = parent;
            Options = parent.Options;
            Mapper = parent.Mapper;
            if(MustPreserveReferences())
            {
                _instanceCache = parent.InstanceCache;
            }
        }

        private bool MustPreserveReferences()
        {
            var context = this;
            do
            {
                if(context.TypeMap != null && context.TypeMap.PreserveReferences)
                {
                    return true;
                }
                context = context.Parent;
            }
            while(context != null);
            return false;
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

        internal object Map(object source, object destination, Type sourceType, Type destinationType)
        {
            Types = TypePair.Create(source, destination, destinationType, destinationType);
            var typeMap = ConfigurationProvider.ResolveTypeMap(Types);
            SourceValue = source;
            DestinationValue = destination;
            TypeMap = typeMap;
            return Mapper.Map(this);
        }
    }
}