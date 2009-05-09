using System;
using System.Collections.Generic;

namespace AutoMapper
{
	public class ResolutionContext
	{
		public TypeMap TypeMap { get; private set; }
		public PropertyMap PropertyMap { get; private set; }
		public Type SourceType { get; private set; }
		public Type DestinationType { get; private set; }
		public int? ArrayIndex { get; private set; }
		public object SourceValue { get; private set; }
		public object DestinationValue { get; private set; }
		public ResolutionContext Parent { get; private set; }
		public Dictionary<object, object> InstanceCache { get; private set; }

		private ResolutionContext()
		{
		}

		public ResolutionContext(TypeMap typeMap, object source, Type sourceType, Type destinationType)
			: this(typeMap, source, null, sourceType, destinationType)
		{
		}

		public ResolutionContext(TypeMap typeMap, object source, object destination, Type sourceType, Type destinationType)
		{
			TypeMap = typeMap;
			SourceValue = source;
			DestinationValue = destination;
			if (typeMap != null)
			{
				SourceType = typeMap.SourceType;
				DestinationType = typeMap.DestinationType;
			}
			else
			{
				SourceType = sourceType;
				DestinationType = destinationType;
			}
			InstanceCache = new Dictionary<object, object>();
		}

		public string MemberName
		{
			get
			{
				return PropertyMap == null
				       	? string.Empty
				       	: (ArrayIndex == null
				       	   	? PropertyMap.DestinationProperty.Name
				       	   	: PropertyMap.DestinationProperty.Name + ArrayIndex.Value);
			}
		}

		public bool IsSourceValueNull
		{
			get { return Equals(null, SourceValue); }
		}

		public ResolutionContext CreateValueContext(object sourceValue)
		{
			return new ResolutionContext
				{
					ArrayIndex = ArrayIndex,
					TypeMap = TypeMap,
					PropertyMap = PropertyMap,
					SourceType = SourceType,
					SourceValue = sourceValue,
					DestinationValue = DestinationValue,
					Parent = this,
					DestinationType = DestinationType,
					InstanceCache = InstanceCache
				};
		}

		public ResolutionContext CreateValueContext(object sourceValue, Type sourceType)
		{
			return new ResolutionContext
				{
					ArrayIndex = ArrayIndex,
					TypeMap = TypeMap,
					PropertyMap = PropertyMap,
					SourceType = sourceType,
					SourceValue = sourceValue,
					DestinationValue = DestinationValue,
					Parent = this,
					DestinationType = DestinationType,
					InstanceCache = InstanceCache
				};
		}

		public ResolutionContext CreateMemberContext(TypeMap memberTypeMap, object memberValue, Type sourceMemberType, PropertyMap propertyMap)
		{
			if (memberTypeMap != null)
				return new ResolutionContext
					{
						Parent = this,
						DestinationType = memberTypeMap.DestinationType,
						PropertyMap = propertyMap,
						SourceType = memberTypeMap.SourceType,
						SourceValue = memberValue,
						TypeMap = memberTypeMap,
						InstanceCache = InstanceCache
					};

			return new ResolutionContext
				{
					Parent = this,
					DestinationType = propertyMap.DestinationProperty.MemberType,
					PropertyMap = propertyMap,
					SourceType = sourceMemberType,
					SourceValue = memberValue,
					TypeMap = memberTypeMap,
					InstanceCache = InstanceCache
				};
		}

		public ResolutionContext CreateElementContext(TypeMap elementTypeMap, object item, Type sourceElementType, Type destinationElementType, int arrayIndex)
		{
			return new ResolutionContext
				{
					ArrayIndex = arrayIndex,
					Parent = this,
					DestinationType = destinationElementType,
					PropertyMap = PropertyMap,
					SourceType = sourceElementType,
					SourceValue = item,
					TypeMap = elementTypeMap,
					InstanceCache = InstanceCache
				};
		}

		public override string ToString()
		{
			return string.Format("Trying to map {0} to {1}.", SourceType.Name, DestinationType.Name);
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
	}
}
