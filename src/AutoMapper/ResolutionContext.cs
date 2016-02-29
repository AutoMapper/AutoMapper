namespace AutoMapper
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    /// <summary>
    /// Context information regarding resolution of a destination value
    /// </summary>
    public class ResolutionContext : IEquatable<ResolutionContext>
    {
        /// <summary>
        /// Mapping operation options
        /// </summary>
        public MappingOperationOptions Options { get; }

        /// <summary>
        /// Current type map
        /// </summary>
        public TypeMap TypeMap { get; }

        /// <summary>
        /// Current source type
        /// </summary>
        public Type SourceType { get; }

        /// <summary>
        /// Current attempted destination type
        /// </summary>
        public Type DestinationType { get; }

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
        public Dictionary<ResolutionContext, object> InstanceCache { get; }

        /// <summary>
        /// Current mapper
        /// </summary>
        public IRuntimeMapper Mapper { get; }

        /// <summary>
        /// Current configuration
        /// </summary>
        public IConfigurationProvider ConfigurationProvider => Mapper.ConfigurationProvider;

        /// <summary>
        /// Source and destination type pair
        /// </summary>
        public TypePair Types { get; }

        public bool IsSourceValueNull => Equals(null, SourceValue);

        public ResolutionContext(object source, object destination, Type sourceType, Type destinationType, TypeMap typeMap, ResolutionContext parent)
            : this(source, destination, typeMap)
        {
            Parent = parent;
            Options = parent.Options;
            Mapper = parent.Mapper;

            InstanceCache = parent.InstanceCache;

            SourceType = sourceType ?? typeMap?.SourceType ?? parent.SourceType;
            DestinationType = destinationType ?? typeMap?.DestinationType ?? parent.DestinationType;

            Types = new TypePair(SourceType, DestinationType);
        }

        public ResolutionContext(object source, object destination, Type sourceType, Type destinationType, TypeMap typeMap, MappingOperationOptions options, IRuntimeMapper mapper)
            : this(source, destination, typeMap)
        {
            Options = options;
            Mapper = mapper;

            InstanceCache = new Dictionary<ResolutionContext, object>();

            SourceType = source?.GetType() ?? sourceType ?? typeMap?.SourceType;
            DestinationType = destination?.GetType() ?? destinationType ?? typeMap?.DestinationType;

            Types = new TypePair(SourceType, DestinationType);
        }

        private ResolutionContext(object source, object destination, TypeMap typeMap)
        {
            SourceValue = source;
            DestinationValue = destination;

            TypeMap = typeMap;
        }

        public override string ToString() => $"Trying to map {SourceType.Name} to {DestinationType.Name}.";

        public bool Equals(ResolutionContext other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.TypeMap, TypeMap) && Equals(other.SourceType, SourceType) &&
                   Equals(other.DestinationType, DestinationType) && Equals(other.SourceValue, SourceValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ResolutionContext)) return false;
            return Equals((ResolutionContext) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = TypeMap?.GetHashCode() ?? 0;
                result = (result*397) ^ (SourceType?.GetHashCode() ?? 0);
                result = (result*397) ^ (DestinationType?.GetHashCode() ?? 0);
                result = (result*397) ^ (SourceValue?.GetHashCode() ?? 0);
                return result;
            }
        }

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
            while(context.Parent != null)
            {
                yield return context;
                context = context.Parent;
            }
            yield return context;
        }

        internal void BeforeMap(object destination)
        {
            if(Parent == null)
            {
                Options.BeforeMapAction(SourceValue, destination);
            }
        }

        internal void AfterMap(object destination)
        {
            if(Parent == null)
            {
                Options.AfterMapAction(SourceValue, destination);
            }
        }
    }
}