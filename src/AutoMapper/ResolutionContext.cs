using System;
using System.Collections.Generic;

namespace AutoMapper
{
	public class ResolutionContext : IEquatable<ResolutionContext>
	{
		private readonly TypeMap _typeMap;
		private readonly PropertyMap _propertyMap;
		private readonly Type _sourceType;
		private readonly Type _destinationType;
		private readonly int? _arrayIndex;
		private readonly object _sourceValue;
		private readonly object _destinationValue;
		private readonly ResolutionContext _parent;
		private readonly Dictionary<ResolutionContext, object> _instanceCache;

		public TypeMap TypeMap { get { return _typeMap; } }
		public PropertyMap PropertyMap { get { return _propertyMap; } }
		public Type SourceType { get { return _sourceType; } }
		public Type DestinationType { get { return _destinationType; } }
		public int? ArrayIndex { get { return _arrayIndex; } }
		public object SourceValue { get { return _sourceValue; } }
		public object DestinationValue { get { return _destinationValue; } }
		public ResolutionContext Parent { get { return _parent; } }
		public Dictionary<ResolutionContext, object> InstanceCache { get { return _instanceCache; } }

		public ResolutionContext(TypeMap typeMap, object source, Type sourceType, Type destinationType)
			: this(typeMap, source, null, sourceType, destinationType)
		{
		}

		public ResolutionContext(TypeMap typeMap, object source, object destination, Type sourceType, Type destinationType)
		{
			_typeMap = typeMap;
			_sourceValue = source;
			_destinationValue = destination;
			if (typeMap != null)
			{
				_sourceType = typeMap.SourceType;
				_destinationType = typeMap.DestinationType;
			}
			else
			{
				_sourceType = sourceType;
				_destinationType = destinationType;
			}
			_instanceCache = new Dictionary<ResolutionContext, object>();
		}

		private ResolutionContext(ResolutionContext context, object sourceValue)
		{
			_arrayIndex = context._arrayIndex;
			_typeMap = context._typeMap;
			_propertyMap = context._propertyMap;
			_sourceType = context._sourceType;
			_sourceValue = sourceValue;
			_destinationValue = context._destinationValue;
			_parent = context;
			_destinationType = context._destinationType;
			_instanceCache = context._instanceCache;
		}

		private ResolutionContext(ResolutionContext context, object sourceValue, Type sourceType)
		{
			_arrayIndex = context._arrayIndex;
			_typeMap = context._typeMap;
			_propertyMap = context._propertyMap;
			_sourceType = sourceType;
			_sourceValue = sourceValue;
			_destinationValue = context._destinationValue;
			_parent = context;
			_destinationType = context._destinationType;
			_instanceCache = context._instanceCache;
		}

		private ResolutionContext(ResolutionContext context, object sourceValue, object destinationValue, TypeMap memberTypeMap, PropertyMap propertyMap)
		{
			_typeMap = memberTypeMap;
			_propertyMap = propertyMap;
			_sourceType = memberTypeMap.SourceType;
			_sourceValue = sourceValue;
            _destinationValue = destinationValue;
			_parent = context;
			_destinationType = memberTypeMap.DestinationType;
			_instanceCache = context._instanceCache;
		}

		private ResolutionContext(ResolutionContext context, object sourceValue, object destinationValue, Type sourceType, PropertyMap propertyMap)
		{
			_propertyMap = propertyMap;
			_sourceType = sourceType;
			_sourceValue = sourceValue;
            _destinationValue = destinationValue;
			_parent = context;
			_destinationType = propertyMap.DestinationProperty.MemberType;
			_instanceCache = context._instanceCache;
		}

		private ResolutionContext(ResolutionContext context, object sourceValue, TypeMap typeMap, Type sourceType, Type destinationType, int arrayIndex)
		{
			_arrayIndex = arrayIndex;
			_typeMap = typeMap;
			_propertyMap = context._propertyMap;
			_sourceType = sourceType;
			_sourceValue = sourceValue;
			_parent = context;
			_destinationType = destinationType;
			_instanceCache = context._instanceCache;
		}

		public string MemberName
		{
			get
			{
				return _propertyMap == null
				       	? string.Empty
				       	: (_arrayIndex == null
				       	   	? _propertyMap.DestinationProperty.Name
				       	   	: _propertyMap.DestinationProperty.Name + _arrayIndex.Value);
			}
		}

		public bool IsSourceValueNull
		{
			get { return Equals(null, _sourceValue); }
		}

		public ResolutionContext CreateValueContext(object sourceValue)
		{
			return new ResolutionContext(this, sourceValue);
		}

		public ResolutionContext CreateValueContext(object sourceValue, Type sourceType)
		{
			return new ResolutionContext(this, sourceValue, sourceType);
		}

		public ResolutionContext CreateMemberContext(TypeMap memberTypeMap, object memberValue, object destinationValue, Type sourceMemberType, PropertyMap propertyMap)
		{
			return memberTypeMap != null
			       	? new ResolutionContext(this, memberValue, destinationValue, memberTypeMap, propertyMap)
			       	: new ResolutionContext(this, memberValue, destinationValue, sourceMemberType, propertyMap);
		}

		public ResolutionContext CreateElementContext(TypeMap elementTypeMap, object item, Type sourceElementType, Type destinationElementType, int arrayIndex)
		{
			return new ResolutionContext(this, item, elementTypeMap, sourceElementType, destinationElementType, arrayIndex);
		}

		public override string ToString()
		{
			return string.Format("Trying to map {0} to {1}.", SourceType.Name, DestinationType.Name);
		}

		public TypeMap GetContextTypeMap()
		{
			TypeMap typeMap = _typeMap;
			ResolutionContext parent = _parent;
			while ((typeMap == null) && (parent != null))
			{
				typeMap = parent._typeMap;
				parent = parent._parent;
			}
			return typeMap;
		}

		public PropertyMap GetContextPropertyMap()
		{
			PropertyMap propertyMap = _propertyMap;
			ResolutionContext parent = _parent;
			while ((propertyMap == null) && (parent != null))
			{
				propertyMap = parent._propertyMap;
				parent = parent._parent;
			}
			return propertyMap;
		}

		public bool Equals(ResolutionContext other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other._typeMap, _typeMap) && Equals(other._sourceType, _sourceType) && Equals(other._destinationType, _destinationType) && other._arrayIndex.Equals(_arrayIndex) && Equals(other._sourceValue, _sourceValue);
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
				int result = (_typeMap != null ? _typeMap.GetHashCode() : 0);
				result = (result*397) ^ (_sourceType != null ? _sourceType.GetHashCode() : 0);
				result = (result*397) ^ (_destinationType != null ? _destinationType.GetHashCode() : 0);
				result = (result*397) ^ (_arrayIndex.HasValue ? _arrayIndex.Value : 0);
				result = (result*397) ^ (_sourceValue != null ? _sourceValue.GetHashCode() : 0);
				return result;
			}
		}
	}

}
