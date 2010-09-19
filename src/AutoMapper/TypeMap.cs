using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper
{
	public class TypeMap
	{
		private readonly IList<Action<object, object>> _afterMapActions = new List<Action<object, object>>();
		private readonly IList<Action<object, object>> _beforeMapActions = new List<Action<object, object>>();
		private readonly TypeInfo _destinationType;
		private readonly IDictionary<Type, Type> _includedDerivedTypes = new Dictionary<Type, Type>();
		private readonly IList<PropertyMap> _propertyMaps = new List<PropertyMap>();
		private PropertyMap[] _orderedPropertyMaps;
		private readonly TypeInfo _sourceType;
		private bool _sealed;

		public TypeMap(TypeInfo sourceType, TypeInfo destinationType)
		{
			_sourceType = sourceType;
			_destinationType = destinationType;
            Profile = ConfigurationStore.DefaultProfileName;
		}

		public Type SourceType
		{
			get { return _sourceType.Type; }
		}

		public Type DestinationType
		{
			get { return _destinationType.Type; }
		}

		public string Profile { get; set; }
		public Func<ResolutionContext, object> CustomMapper { get; private set; }

		public Action<object, object> BeforeMap
		{
			get
			{
				return (src, dest) =>
				       	{
				       		foreach (var action in _beforeMapActions)
				       			action(src, dest);
				       	};
			}
		}

		public Action<object, object> AfterMap
		{
			get
			{
				return (src, dest) =>
				       	{
				       		foreach (var action in _afterMapActions)
				       			action(src, dest);
				       	};
			}
		}

		public Func<object, object> DestinationCtor { get; set; }

	    public List<string> IgnorePropertiesStartingWith { get; set; }

	    public IEnumerable<PropertyMap> GetPropertyMaps()
		{
            if (_sealed)
                return _orderedPropertyMaps;

		    return _propertyMaps;
		}

		public void AddPropertyMap(PropertyMap propertyMap)
		{
			_propertyMaps.Add(propertyMap);
		}

		public void AddPropertyMap(IMemberAccessor destProperty, IEnumerable<IValueResolver> resolvers)
		{
			var propertyMap = new PropertyMap(destProperty);

			resolvers.Each(propertyMap.ChainResolver);

			AddPropertyMap(propertyMap);
		}

		public string[] GetUnmappedPropertyNames()
		{
			var autoMappedProperties = _propertyMaps.Where(pm => pm.IsMapped())
				.Select(pm => pm.DestinationProperty.Name);

		    var properties = _destinationType.GetPublicWriteAccessors()
		        .Select(p => p.Name)
		        .Except(autoMappedProperties);

		    return properties.Where(memberName => !IgnorePropertiesStartingWith.Any(memberName.StartsWith)).ToArray();
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
            _includedDerivedTypes[derivedSourceType] = derivedDestinationType;
		}

		public Type GetDerivedTypeFor(Type derivedSourceType)
		{
			if (!_includedDerivedTypes.ContainsKey(derivedSourceType))
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
			_propertyMaps.Clear();
		}

		public void AddBeforeMapAction(Action<object, object> beforeMap)
		{
			_beforeMapActions.Add(beforeMap);
		}

		public void AddAfterMapAction(Action<object, object> afterMap)
		{
			_afterMapActions.Add(afterMap);
		}

		public void Seal()
		{
			if (_sealed)
				return;

			_orderedPropertyMaps = _propertyMaps.OrderBy(map => map.GetMappingOrder()).ToArray();

			_orderedPropertyMaps.Each(pm => pm.Seal());

			_sealed = true;
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