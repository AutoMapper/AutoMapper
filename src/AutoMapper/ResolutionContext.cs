namespace AutoMapper
{
    using System;
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
        /// Current property map
        /// </summary>
        public PropertyMap PropertyMap { get; }

        /// <summary>
        /// Current source type
        /// </summary>
        public Type SourceType { get; }

        /// <summary>
        /// Current attempted destination type
        /// </summary>
        public Type DestinationType { get; }

        /// <summary>
        /// Index of current collection mapping
        /// </summary>
        public int? ArrayIndex { get; }

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
        /// Current mapping engine
        /// </summary>
        public IMappingEngine Engine { get; }

        private ResolutionContext(ResolutionContext context, object sourceValue, object destinationValue)
        {
            Parent = context;
            ArrayIndex = context.ArrayIndex;
            PropertyMap = context.PropertyMap;
            DestinationType = context.DestinationType;
            InstanceCache = context.InstanceCache;
            Options = context.Options;
            Engine = context.Engine;
            SourceValue = sourceValue;
            DestinationValue = destinationValue;
        }

        public ResolutionContext(TypeMap typeMap, object source, Type sourceType, Type destinationType,
            MappingOperationOptions options, IMappingEngine engine)
            : this(typeMap, source, null, sourceType, destinationType, options, engine)
        {
        }

        public ResolutionContext(TypeMap typeMap, object source, object destination, Type sourceType,
            Type destinationType, MappingOperationOptions options, IMappingEngine engine)
        {
            TypeMap = typeMap;
            SourceValue = source;
            DestinationValue = destination;
            if(typeMap != null)
            {
                SourceType = typeMap.SourceType;
                DestinationType = typeMap.DestinationType;
            }
            else
            {
                SourceType = sourceType;
                DestinationType = destinationType;
            }
            InstanceCache = new Dictionary<ResolutionContext, object>();
            Options = options;
            Engine = engine;
        }

        private ResolutionContext(ResolutionContext context, object sourceValue, Type sourceType) : this(context, sourceValue, context.DestinationValue)
        {
            TypeMap = null;
            SourceType = sourceType;
        }

        private ResolutionContext(ResolutionContext context, TypeMap memberTypeMap, object sourceValue,
            object destinationValue, Type sourceType, Type destinationType, int? arrayIndex = null) : this(context, sourceValue, destinationValue)
        {
            ArrayIndex = arrayIndex;
            TypeMap = memberTypeMap;
            if(memberTypeMap != null)
            {
                SourceType = memberTypeMap.SourceType;
                DestinationType = memberTypeMap.DestinationType;
            }
            else
            {
                SourceType = sourceType;
                DestinationType = destinationType;
            }
        }

        private ResolutionContext(ResolutionContext context, object sourceValue, object destinationValue,
            TypeMap memberTypeMap, PropertyMap propertyMap) : this(context, sourceValue, destinationValue)
        {
            TypeMap = memberTypeMap;
            PropertyMap = propertyMap;
            SourceType = memberTypeMap.SourceType;
            DestinationType = memberTypeMap.DestinationType;
        }

        private ResolutionContext(ResolutionContext context, object sourceValue, object destinationValue,
            Type sourceType, PropertyMap propertyMap) : this(context, sourceValue, destinationValue)
        {
            PropertyMap = propertyMap;
            SourceType = sourceType;
            var destinationMemberType = propertyMap.DestinationProperty.MemberType;
            DestinationType = destinationMemberType == typeof(object) ? sourceType : destinationMemberType;
        }

        private ResolutionContext(ResolutionContext context, object sourceValue, TypeMap typeMap, Type sourceType,
            Type destinationType, int arrayIndex) : this(context, typeMap, sourceValue, null, sourceType, destinationType, arrayIndex)
        {
        }

        public string MemberName => PropertyMap == null
            ? string.Empty
            : (ArrayIndex == null
                ? PropertyMap.DestinationProperty.Name
                : PropertyMap.DestinationProperty.Name + ArrayIndex.Value);

        public bool IsSourceValueNull => Equals(null, SourceValue);


        public ResolutionContext CreateValueContext(object sourceValue, Type sourceType)
        {
            return new ResolutionContext(this, sourceValue, sourceType);
        }

        public ResolutionContext CreateTypeContext(TypeMap memberTypeMap, object sourceValue, object destinationValue,
            Type sourceType, Type destinationType)
        {
            return new ResolutionContext(this, memberTypeMap, sourceValue, destinationValue, sourceType, destinationType);
        }

        public ResolutionContext CreatePropertyMapContext(PropertyMap propertyMap)
        {
            return new ResolutionContext(this, SourceValue, DestinationValue, SourceType, propertyMap);
        }

        public ResolutionContext CreateMemberContext(TypeMap memberTypeMap, object memberValue, object destinationValue,
            Type sourceMemberType, PropertyMap propertyMap)
        {
            return memberTypeMap != null
                ? new ResolutionContext(this, memberValue, destinationValue, memberTypeMap, propertyMap)
                : new ResolutionContext(this, memberValue, destinationValue, sourceMemberType, propertyMap);
        }

        public ResolutionContext CreateElementContext(TypeMap elementTypeMap, object item, Type sourceElementType,
            Type destinationElementType, int arrayIndex)
        {
            return new ResolutionContext(this, item, elementTypeMap, sourceElementType, destinationElementType,
                arrayIndex);
        }

        public override string ToString()
        {
            return $"Trying to map {SourceType.Name} to {DestinationType.Name}.";
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

        public PropertyMap GetContextPropertyMap()
        {
            PropertyMap propertyMap = PropertyMap;
            ResolutionContext parent = Parent;
            while ((propertyMap == null) && (parent != null))
            {
                propertyMap = parent.PropertyMap;
                parent = parent.Parent;
            }
            return propertyMap;
        }

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
                int result = (TypeMap != null ? TypeMap.GetHashCode() : 0);
                result = (result*397) ^ (SourceType != null ? SourceType.GetHashCode() : 0);
                result = (result*397) ^ (DestinationType != null ? DestinationType.GetHashCode() : 0);
                result = (result*397) ^ (SourceValue != null ? SourceValue.GetHashCode() : 0);
                return result;
            }
        }

        public static ResolutionContext New<TSource>(TSource sourceValue)
        {
            return new ResolutionContext(null, sourceValue, typeof (TSource), null, new MappingOperationOptions(),
                Mapper.Engine);
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