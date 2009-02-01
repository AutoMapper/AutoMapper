using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper.ReflectionExtensions;

namespace AutoMapper
{
	public class TypeMap
	{
		private readonly IList<PropertyMap> _propertyMaps = new List<PropertyMap>();
		private readonly IDictionary<Type, Type> _includedDerivedTypes = new Dictionary<Type, Type>(); // SourceType, DestinationType

		public TypeMap(Type sourceType, Type destinationType)
		{
			DestinationType = destinationType;
			SourceType = sourceType;
			Profile = Configuration.DefaultProfileName;
		}

		public Type SourceType { get; private set; }
		public Type DestinationType { get; private set; }
		public string Profile { get; set; }

		public PropertyMap[] GetPropertyMaps()
		{
			return _propertyMaps.ToArray();
		}

		public void AddPropertyMap(PropertyMap propertyMap)
		{
			_propertyMaps.Add(propertyMap);
		}

		public string[] GetUnmappedPropertyNames()
		{
			var autoMappedProperties = _propertyMaps.Where(pm => pm.IsMapped())
				.Select(pm => pm.DestinationProperty.Name);

			return DestinationType.GetPublicGetProperties()
							.Select(p => p.Name)
							.Except(autoMappedProperties)
							.ToArray();
		}

		public PropertyMap FindOrCreatePropertyMapFor(PropertyInfo destinationProperty)
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
	}
}