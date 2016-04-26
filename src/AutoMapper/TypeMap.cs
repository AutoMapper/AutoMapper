
namespace AutoMapper
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Configuration;
    using Execution;
    using static System.Linq.Expressions.Expression;

    /// <summary>
    /// Main configuration object holding all mapping configuration for a source and destination type
    /// </summary>
    [DebuggerDisplay("{SourceType.Name} -> {DestinationType.Name}")]
    public class TypeMap
    {
        private readonly List<LambdaExpression> _afterMapActions = new List<LambdaExpression>();
        private readonly List<LambdaExpression> _beforeMapActions = new List<LambdaExpression>();
        private readonly HashSet<TypePair> _includedDerivedTypes = new HashSet<TypePair>();
        private readonly HashSet<TypePair> _includedBaseTypes = new HashSet<TypePair>();
        private readonly ConcurrentBag<PropertyMap> _propertyMaps = new ConcurrentBag<PropertyMap>();
        private readonly ConcurrentBag<SourceMemberConfig> _sourceMemberConfigs = new ConcurrentBag<SourceMemberConfig>();

        private readonly IList<PropertyMap> _inheritedMaps = new List<PropertyMap>();
        private PropertyMap[] _orderedPropertyMaps;
        private bool _sealed;
        private readonly IList<TypeMap> _inheritedTypeMaps = new List<TypeMap>();

        public TypeMap(TypeDetails sourceType, TypeDetails destinationType, MemberList memberList, IProfileConfiguration profile)
        {
            SourceTypeDetails = sourceType;
            DestinationTypeDetails = destinationType;
            Types = new TypePair(sourceType.Type, destinationType.Type);
            Profile = profile;
            ConfiguredMemberList = memberList;
            IgnorePropertiesStartingWith = profile.GlobalIgnores;
        }

        public LambdaExpression MapExpression { get; private set; }

        public TypePair Types { get; }

        public ConstructorMap ConstructorMap { get; set; }

        public TypeDetails SourceTypeDetails { get; }
        public TypeDetails DestinationTypeDetails { get; }

        public Type SourceType => SourceTypeDetails.Type;
        public Type DestinationType => DestinationTypeDetails.Type;

        public IProfileConfiguration Profile { get; }

        public LambdaExpression CustomMapper { get; set; }
        public LambdaExpression CustomProjection { get; set; }
        public LambdaExpression DestinationCtor { get; set; }

        public IEnumerable<string> IgnorePropertiesStartingWith { get; set; }

        public Type DestinationTypeOverride { get; set; }
        public Type DestinationTypeToUse => DestinationTypeOverride ?? DestinationType;

        public bool ConstructDestinationUsingServiceLocator { get; set; }

        public MemberList ConfiguredMemberList { get; }

        public IEnumerable<TypePair> IncludedDerivedTypes => _includedDerivedTypes;
        public IEnumerable<TypePair> IncludedBaseTypes => _includedBaseTypes;

        public IEnumerable<LambdaExpression> BeforeMapActions => _beforeMapActions;
        public IEnumerable<LambdaExpression> AfterMapActions => _afterMapActions; 

        public bool PreserveReferences { get; set; }
        public LambdaExpression Condition { get; set; }

        public int MaxDepth { get; set; }

        public LambdaExpression Substitution { get; set; }
        public LambdaExpression ConstructExpression { get; set; }
        public Type TypeConverterType { get; set; }

        public PropertyMap[] GetPropertyMaps()
        {
            return _orderedPropertyMaps ?? _propertyMaps.Concat(_inheritedMaps).ToArray();
        }

        public void AddPropertyMap(IMemberAccessor destProperty, IEnumerable<IMemberGetter> resolvers)
        {
            var propertyMap = new PropertyMap(destProperty, this);

            propertyMap.ChainMembers(resolvers);

            _propertyMaps.Add(propertyMap);
        }

        public string[] GetUnmappedPropertyNames()
        {
            Func<PropertyMap, string> getFunc =
                pm =>
                    ConfiguredMemberList == MemberList.Destination
                        ? pm.DestinationProperty.Name
                        : pm.CustomExpression == null && pm.SourceMember != null
                            ? pm.SourceMember.Name
                            : pm.DestinationProperty.Name;
            var autoMappedProperties = _propertyMaps.Where(pm => pm.IsMapped())
                .Select(getFunc).ToList();
            var inheritedProperties = _inheritedMaps.Where(pm => pm.IsMapped())
                .Select(getFunc).ToList();

            IEnumerable<string> properties;

            if (ConfiguredMemberList == MemberList.Destination)
            {
                properties = DestinationTypeDetails.PublicWriteAccessors
                    .Select(p => p.Name)
                    .Except(autoMappedProperties)
                    .Except(inheritedProperties);
            }
            else
            {
                var redirectedSourceMembers = _propertyMaps
                    .Where(pm => pm.IsMapped() && pm.SourceMember != null && pm.SourceMember.Name != pm.DestinationProperty.Name)
                    .Select(pm => pm.SourceMember.Name);

                var ignoredSourceMembers = _sourceMemberConfigs
                    .Where(smc => smc.IsIgnored())
                    .Select(pm => pm.SourceMember.Name).ToList();

                properties = SourceTypeDetails.PublicReadAccessors
                    .Select(p => p.Name)
                    .Except(autoMappedProperties)
                    .Except(inheritedProperties)
                    .Except(redirectedSourceMembers)
                    .Except(ignoredSourceMembers);
            }

            return properties.Where(memberName => !IgnorePropertiesStartingWith.Any(memberName.StartsWith)).ToArray();
        }

        public PropertyMap FindOrCreatePropertyMapFor(IMemberAccessor destinationProperty)
        {
            var propertyMap = GetExistingPropertyMapFor(destinationProperty);

            if (propertyMap != null) return propertyMap;

            propertyMap = new PropertyMap(destinationProperty, this);

            _propertyMaps.Add(propertyMap);

            return propertyMap;
        }

        public void IncludeDerivedTypes(Type derivedSourceType, Type derivedDestinationType)
        {
            var derivedTypes = new TypePair(derivedSourceType, derivedDestinationType);
            if (derivedTypes.Equals(Types))
            {
                throw new InvalidOperationException("You cannot include a type map into itself.");
            }
            _includedDerivedTypes.Add(derivedTypes);
        }

        public void IncludeBaseTypes(Type baseSourceType, Type baseDestinationType)
        {
            var baseTypes = new TypePair(baseSourceType, baseDestinationType);
            if (baseTypes.Equals(Types))
            {
                throw new InvalidOperationException("You cannot include a type map into itself.");
            }
            _includedBaseTypes.Add(baseTypes);
        }

        public Type GetDerivedTypeFor(Type derivedSourceType)
        {
            if (DestinationTypeOverride != null)
            {
                return DestinationTypeOverride;
            }
            // This might need to be fixed for multiple derived source types to different dest types
            var match = _includedDerivedTypes.FirstOrDefault(tp => tp.SourceType == derivedSourceType);

            return match.DestinationType ?? DestinationType;
        }

        public bool TypeHasBeenIncluded(TypePair derivedTypes)
        {
            return _includedDerivedTypes.Contains(derivedTypes);
        }

        public bool HasDerivedTypesToInclude()
        {
            return _includedDerivedTypes.Any() || DestinationTypeOverride != null;
        }

        public void AddBeforeMapAction(LambdaExpression beforeMap)
        {
            _beforeMapActions.Add(beforeMap);
        }

        public void AddAfterMapAction(LambdaExpression afterMap)
        {
            _afterMapActions.Add(afterMap);
        }

        public void Seal(TypeMapRegistry typeMapRegistry)
        {
            if (_sealed)
                return;

            foreach (var inheritedTypeMap in _inheritedTypeMaps)
            {
                ApplyInheritedTypeMap(inheritedTypeMap);
            }

            _orderedPropertyMaps =
                _propertyMaps
                    .Union(_inheritedMaps)
                    .OrderBy(map => map.MappingOrder).ToArray();

            MapExpression = TypeMapPlanBuilder.BuildMapperFunc(this, typeMapRegistry);

            _sealed = true;
        }

        public bool Equals(TypeMap other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.SourceTypeDetails, SourceTypeDetails) && Equals(other.DestinationTypeDetails, DestinationTypeDetails);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(TypeMap)) return false;
            return Equals((TypeMap)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (SourceTypeDetails.GetHashCode() * 397) ^ DestinationTypeDetails.GetHashCode();
            }
        }

        public PropertyMap GetExistingPropertyMapFor(IMemberAccessor destinationProperty)
        {
            var propertyMap =
                _propertyMaps.FirstOrDefault(pm => pm.DestinationProperty.Name.Equals(destinationProperty.Name));

            if (propertyMap != null)
                return propertyMap;

            propertyMap =
                _inheritedMaps.FirstOrDefault(pm => pm.DestinationProperty.Name.Equals(destinationProperty.Name));

            if (propertyMap == null)
                return null;

            var propertyInfo = propertyMap.DestinationProperty.MemberInfo as PropertyInfo;

            if (propertyInfo == null)
                return propertyMap;

            var baseAccessor = propertyInfo.GetMethod;

            if (baseAccessor.IsAbstract || baseAccessor.IsVirtual)
                return propertyMap;

            var accessor = ((PropertyInfo)destinationProperty.MemberInfo).GetMethod;

            if (baseAccessor.DeclaringType == accessor.DeclaringType)
                return propertyMap;

            return null;
        }

        public void InheritTypes(TypeMap inheritedTypeMap)
        {
            foreach (var includedDerivedType in inheritedTypeMap._includedDerivedTypes
                .Where(includedDerivedType => !_includedDerivedTypes.Contains(includedDerivedType)))
            {
                _includedDerivedTypes.Add(includedDerivedType);
            }
        }

        public SourceMemberConfig FindOrCreateSourceMemberConfigFor(MemberInfo sourceMember)
        {
            var config = _sourceMemberConfigs.FirstOrDefault(smc => Equals(smc.SourceMember, sourceMember));

            if (config != null) return config;

            config = new SourceMemberConfig(sourceMember);
            _sourceMemberConfigs.Add(config);

            return config;
        }

        public void ApplyInheritedMap(TypeMap inheritedTypeMap)
        {
            _inheritedTypeMaps.Add(inheritedTypeMap);
        }

        public bool ShouldCheckForValid()
        {
            return CustomMapper == null
                && CustomProjection == null
                && TypeConverterType == null
                && DestinationTypeOverride == null;
        }

        private void ApplyInheritedTypeMap(TypeMap inheritedTypeMap)
        {
            foreach (var inheritedMappedProperty in inheritedTypeMap.GetPropertyMaps().Where(m => m.IsMapped()))
            {
                var conventionPropertyMap = GetPropertyMaps()
                    .SingleOrDefault(m =>
                        m.DestinationProperty.Name == inheritedMappedProperty.DestinationProperty.Name);

                if (conventionPropertyMap != null)
                {
                    conventionPropertyMap.ApplyInheritedPropertyMap(inheritedMappedProperty);
                }
                else
                {
                    var propertyMap = new PropertyMap(inheritedMappedProperty, this);

                    _inheritedMaps.Add(propertyMap);
                }
            }

            //Include BeforeMap
            foreach (var beforeMapAction in inheritedTypeMap._beforeMapActions)
            {
                AddBeforeMapAction(beforeMapAction);
            }
            //Include AfterMap
            foreach (var afterMapAction in inheritedTypeMap._afterMapActions)
            {
                AddAfterMapAction(afterMapAction);
            }
        }
    }
}
