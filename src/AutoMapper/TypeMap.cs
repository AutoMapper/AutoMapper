using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper
{
	public class TypeMap
	{
		private readonly IList<Func<Action<object, object>>> _afterMapActions = new List<Func<Action<object, object>>>();
		private readonly IList<Func<Action<object, object>>> _beforeMapActions = new List<Func<Action<object, object>>>();
		private readonly TypeInfo _destinationType;
		private readonly IDictionary<Type, Type> _includedDerivedTypes = new Dictionary<Type, Type>();
		private readonly IList<PropertyMap> _propertyMaps = new List<PropertyMap>();
		private readonly TypeInfo _sourceType;

		public TypeMap(TypeInfo sourceType, TypeInfo destinationType)
		{
			_sourceType = sourceType;
			_destinationType = destinationType;
			Profile = Configuration.DefaultProfileName;
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
				       			action()(src, dest);
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
				       			action()(src, dest);
				       	};
			}
		}

		public Func<object, object> DestinationCtor { get; set; }

		public IEnumerable<PropertyMap> GetPropertyMaps()
		{
			return _propertyMaps.OrderBy(map => map.GetMappingOrder());
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

			return _destinationType.GetPublicWriteAccessors()
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

				propertyMap.ChainResolver(destinationProperty);

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

		public void AddBeforeMapAction(Func<Action<object, object>> beforeMap)
		{
			_beforeMapActions.Add(beforeMap);
		}

		public void AddAfterMapAction(Func<Action<object, object>> afterMap)
		{
			_afterMapActions.Add(afterMap);
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