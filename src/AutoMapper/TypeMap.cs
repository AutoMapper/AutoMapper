using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper
{
    public class TypeMap
    {
        private readonly IList<Action<object, object>> _afterMapActions = new List<Action<object, object>>();
        private readonly IList<Action<object, object>> _beforeMapActions = new List<Action<object, object>>();
        private readonly TypeInfo _destinationType;
        private readonly IDictionary<Type, Type> _includedDerivedTypes = new Dictionary<Type, Type>();
		private readonly ThreadSafeList<PropertyMap> _propertyMaps = new ThreadSafeList<PropertyMap>();
        private readonly ThreadSafeList<SourceMemberConfig> _sourceMemberConfigs = new ThreadSafeList<SourceMemberConfig>();
        private readonly IList<PropertyMap> _inheritedMaps = new List<PropertyMap>();
        private PropertyMap[] _orderedPropertyMaps;
        private readonly TypeInfo _sourceType;
        private bool _sealed;
        private Func<ResolutionContext, bool> _condition;
        private ConstructorMap _constructorMap;

        public TypeMap(TypeInfo sourceType, TypeInfo destinationType, MemberList memberList)
        {
            _sourceType = sourceType;
            _destinationType = destinationType;
            Profile = ConfigurationStore.DefaultProfileName;
            ConfiguredMemberList = memberList;
        }

        public ConstructorMap ConstructorMap
        {
            get { return _constructorMap; }
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

        public Func<ResolutionContext, object> DestinationCtor { get; set; }

        public List<string> IgnorePropertiesStartingWith { get; set; }

        public Type DestinationTypeOverride { get; set; }

        public bool ConstructDestinationUsingServiceLocator { get; set; }

        public MemberList ConfiguredMemberList { get; private set; }

        public IEnumerable<PropertyMap> GetPropertyMaps()
        {
            if (_sealed)
                return _orderedPropertyMaps;

            return _propertyMaps.Concat(_inheritedMaps);
        }

        public IEnumerable<PropertyMap> GetCustomPropertyMaps()
        {
            return _propertyMaps;
        }

        public void AddPropertyMap(PropertyMap propertyMap)
        {
            _propertyMaps.Add(propertyMap);
        }

        protected void AddInheritedMap(PropertyMap propertyMap)
        {
            _inheritedMaps.Add(propertyMap);
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
            var inheritedProperties = _inheritedMaps.Where(pm => pm.IsMapped())
                .Select(pm => pm.DestinationProperty.Name);

            IEnumerable<string> properties;

            if (ConfiguredMemberList == MemberList.Destination)
                properties = _destinationType.GetPublicWriteAccessors()
                    .Select(p => p.Name)
                    .Except(autoMappedProperties)
                    .Except(inheritedProperties);
            else
            {
                var redirectedSourceMembers = _propertyMaps
                    .Where(pm => pm.IsMapped())
                    .Where(pm => pm.CustomExpression != null)
                    .Where(pm => pm.SourceMember != null)
                    .Select(pm => pm.SourceMember.Name);

                var ignoredSourceMembers = _sourceMemberConfigs
                    .Where(smc => smc.IsIgnored())
                    .Select(pm => pm.SourceMember.Name);

                properties = _sourceType.GetPublicReadAccessors()
                    .Select(p => p.Name)
                    .Except(autoMappedProperties)
                    .Except(inheritedProperties)
                    .Except(redirectedSourceMembers)
                    .Except(ignoredSourceMembers)
                    ;
            }

            return properties.Where(memberName => !IgnorePropertiesStartingWith.Any(memberName.StartsWith)).ToArray();
        }

        public PropertyMap FindOrCreatePropertyMapFor(IMemberAccessor destinationProperty)
        {
            var propertyMap = GetExistingPropertyMapFor(destinationProperty);
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

        public bool TypeHasBeenIncluded(Type derivedSourceType, Type derivedDestinationType)
        {
            if (_includedDerivedTypes.ContainsKey(derivedSourceType))
                return _includedDerivedTypes[derivedSourceType].IsAssignableFrom(derivedDestinationType);
            return false;
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

            _orderedPropertyMaps =
                _propertyMaps
                .Union(_inheritedMaps)
                .OrderBy(map => map.GetMappingOrder()).ToArray();

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

        public PropertyMap GetExistingPropertyMapFor(IMemberAccessor destinationProperty)
        {
            return _propertyMaps.FirstOrDefault(pm => pm.DestinationProperty.Name.Equals(destinationProperty.Name))
                   ??
                   _inheritedMaps.FirstOrDefault(pm => pm.DestinationProperty.Name.Equals(destinationProperty.Name));
        }

        public void AddInheritedPropertyMap(PropertyMap mappedProperty)
        {
            _inheritedMaps.Add(mappedProperty);
        }

        public void InheritTypes(TypeMap inheritedTypeMap)
        {
            foreach (var includedDerivedType in inheritedTypeMap._includedDerivedTypes
                .Where(includedDerivedType => !_includedDerivedTypes.Contains(includedDerivedType)))
            {
                _includedDerivedTypes.Add(includedDerivedType);
            }
        }

        public void SetCondition(Func<ResolutionContext, bool> condition)
        {
            _condition = condition;
        }

        public bool ShouldAssignValue(ResolutionContext resolutionContext)
        {
            return _condition == null || _condition(resolutionContext);
        }

        public void AddConstructorMap(ConstructorInfo constructorInfo, IEnumerable<ConstructorParameterMap> parameters)
        {
            var ctorMap = new ConstructorMap(constructorInfo, parameters);
            _constructorMap = ctorMap;
        }

        public SourceMemberConfig FindOrCreateSourceMemberConfigFor(MemberInfo sourceMember)
        {
            var config = _sourceMemberConfigs.FirstOrDefault(smc => smc.SourceMember == sourceMember);
            if (config == null)
            {
                config = new SourceMemberConfig(sourceMember);
                _sourceMemberConfigs.Add(config);
            }

            return config;
        }
    }
}