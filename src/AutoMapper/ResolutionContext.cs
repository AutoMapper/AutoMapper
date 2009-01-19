using System;

namespace AutoMapper
{
	public class ResolutionContext
	{
		public TypeMap SourceValueTypeMap { get; private set; }
		public TypeMap ContextTypeMap { get; private set; }
		public PropertyMap PropertyMap { get; private set; }
		public Type SourceType { get; private set; }
		public Type DestinationType { get; private set; }
		public int? ArrayIndex { get; private set; }
		public object SourceValue { get; private set; }

		private ResolutionContext()
		{
		}

		public ResolutionContext(TypeMap typeMap, object source, Type sourceType, Type destinationType)
		{
			SourceValueTypeMap = typeMap;
			ContextTypeMap = typeMap;
			SourceValue = source;
			SourceType = sourceType;
			DestinationType = destinationType;
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
					SourceValueTypeMap = SourceValueTypeMap,
					PropertyMap = PropertyMap,
					SourceType = SourceType,
					SourceValue = sourceValue,
					ContextTypeMap = ContextTypeMap,
					DestinationType = DestinationType
				};
		}

		public ResolutionContext CreateMemberContext(TypeMap memberTypeMap, object memberValue, Type sourceMemberType, PropertyMap propertyMap)
		{
			return new ResolutionContext
				{
					ContextTypeMap = memberTypeMap ?? ContextTypeMap,
					DestinationType = propertyMap.DestinationProperty.PropertyType,
					PropertyMap = propertyMap,
					SourceType = sourceMemberType,
					SourceValue = memberValue,
					SourceValueTypeMap = memberTypeMap
				};
		}

		public ResolutionContext CreateElementContext(TypeMap elementTypeMap, object item, Type sourceElementType, Type destinationElementType, int arrayIndex)
		{
			return new ResolutionContext
				{
					ArrayIndex = arrayIndex,
					ContextTypeMap = elementTypeMap ?? ContextTypeMap,
					DestinationType = destinationElementType,
					PropertyMap = PropertyMap,
					SourceType = sourceElementType,
					SourceValue = item,
					SourceValueTypeMap = elementTypeMap
				};
		}
	}
}