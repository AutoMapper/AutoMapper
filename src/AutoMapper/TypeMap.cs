using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.ReflectionExtensions;

namespace AutoMapper
{
	public class TypeMap
	{
		private readonly IList<PropertyMap> _propertyMaps = new List<PropertyMap>();
		private readonly IDictionary<Type, Type> _includedDerivedTypes = new Dictionary<Type, Type>();
		private readonly Type _sourceType;
		private readonly Type _destinationType;

		public TypeMap(Type sourceType, Type destinationType)
		{
			_sourceType = sourceType;
			_destinationType = destinationType;
			Profile = Configuration.DefaultProfileName;
		}

		public Type SourceType { get { return _sourceType; } }
		public Type DestinationType { get { return _destinationType; } }
		public string Profile { get; set; }
		public Func<ResolutionContext, object> CustomMapper { get; private set; }

		public IEnumerable<PropertyMap> GetPropertyMaps()
		{
			return _propertyMaps.OrderBy(map => map.GetMappingOrder());
		}

		public void AddPropertyMap(PropertyMap propertyMap)
		{
			_propertyMaps.Add(propertyMap);
		}

		public string[] GetUnmappedPropertyNames()
		{
			var autoMappedProperties = _propertyMaps.Where(pm => pm.IsMapped())
				.Select(pm => pm.DestinationProperty.Name);

			return DestinationType.GetPublicReadAccessors()
							.Select(p => p.Name)
							.Except(autoMappedProperties)
							.ToArray();
		}

		public PropertyMap FindOrCreatePropertyMapFor(IMemberAccessor destinationProperty)
		{
			var propertyMap = _propertyMaps.FirstOrDefault(pm => pm.DestinationProperty.Name.Equals(destinationProperty.Name));
			if (propertyMap == null)
			{
				propertyMap = new PropertyMap(destinationProperty);
				AddPropertyMap(propertyMap);
			}

			return propertyMap;
		}

		public void IncludeDerivedTypes(Type derivedSourceType, Type derivedDestinationType)
		{
			_includedDerivedTypes.Add(derivedSourceType, derivedDestinationType);
		}

		public Type GetDerivedTypeFor(Type derivedSourceType)
		{
			if (! _includedDerivedTypes.ContainsKey(derivedSourceType))
			{
				return DestinationType;
			}

			return _includedDerivedTypes[derivedSourceType];
		}

        public bool HasDerivedTypesToInclude()
        {
            return _includedDerivedTypes.Any();
        }

		public void UseCustomMapper(Func<ResolutionContext, object> customMapper)
		{
			CustomMapper = customMapper;
		}

	    public bool Equals(TypeMap other)
	    {
	        if (ReferenceEquals(null, other)) return false;
	        if (ReferenceEquals(this, other)) return true;
	        return Equals(other._sourceType, _sourceType) && Equals(other._destinationType, _destinationType);
	    }

	    public override bool Equals(object obj)
	    {
	        if (ReferenceEquals(null, obj)) return false;
	        if (ReferenceEquals(this, obj)) return true;
	        if (obj.GetType() != typeof (TypeMap)) return false;
	        return Equals((TypeMap) obj);
	    }

	    public override int GetHashCode()
	    {
	        unchecked
	        {
	            return (_sourceType.GetHashCode()*397) ^ _destinationType.GetHashCode();
	        }
	    }
	}
}
