using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;

namespace AutoMapper
{
    using Execution;
    using Configuration;
    using Features;
    using QueryableExtensions.Impl;
    using Internal;

    /// <summary>
    /// Main configuration object holding all mapping configuration for a source and destination type
    /// </summary>
    [DebuggerDisplay("{SourceType.Name} -> {DestinationType.Name}")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TypeMap
    {
        private readonly Dictionary<string, PropertyMap> _propertyMaps = new Dictionary<string, PropertyMap>();
        private Features<IRuntimeFeature> _features;
        private HashSet<LambdaExpression> _afterMapActions;
        private HashSet<LambdaExpression> _beforeMapActions;
        private HashSet<TypePair> _includedDerivedTypes;
        private HashSet<TypePair> _includedBaseTypes;
        private Dictionary<MemberPath, PathMap> _pathMaps;
        private Dictionary<MemberInfo, SourceMemberConfig> _sourceMemberConfigs;
        private PropertyMap[] _orderedPropertyMaps;
        private bool _sealed;
        private HashSet<TypeMap> _inheritedTypeMaps;
        private HashSet<IncludedMember> _includedMembersTypeMaps;
        private List<ValueTransformerConfiguration> _valueTransformerConfigs;

        public TypeMap(Type sourceType, Type destinationType, ProfileMap profile)
        {
            Types = new TypePair(sourceType, destinationType);
            Profile = profile;
        }

        public void CheckProjection()
        {
            if (MapExpression == null && !Types.ContainsGenericParameters)
            {
                throw new AutoMapperConfigurationException("CreateProjection works with ProjectTo, not with Map.", QueryMapperHelper.MissingMapException(Types));
            }
        }

        public PathMap FindOrCreatePathMapFor(LambdaExpression destinationExpression, MemberPath path, TypeMap typeMap)
        {
            _pathMaps ??= new();
            var pathMap = _pathMaps.GetOrDefault(path);
            if (pathMap == null)
            {
                pathMap = new PathMap(destinationExpression, path, typeMap);
                AddPathMap(pathMap);
            }
            return pathMap;
        }

        private void AddPathMap(PathMap pathMap) => _pathMaps.Add(pathMap.MemberPath, pathMap);

        public Features<IRuntimeFeature> Features => _features ??= new();
        public LambdaExpression MapExpression { get; private set; }

        public TypePair Types { get; }

        public ConstructorMap ConstructorMap { get; set; }

        public TypeDetails SourceTypeDetails => Profile.GetTypeDetails(SourceType);
        private TypeDetails DestinationTypeDetails => Profile.GetTypeDetails(DestinationType);

        public Type SourceType => Types.SourceType;
        public Type DestinationType => Types.DestinationType;

        public ProfileMap Profile { get; }

        public LambdaExpression CustomMapFunction { get; set; }
        public LambdaExpression CustomMapExpression { get; set; }
        public LambdaExpression CustomCtorFunction { get; set; }
        public LambdaExpression CustomCtorExpression { get; set; }

        public Type DestinationTypeOverride { get; set; }
        public Type DestinationTypeToUse => DestinationTypeOverride ?? DestinationType;

        public bool ConstructDestinationUsingServiceLocator { get; set; }

        public bool IncludeAllDerivedTypes { get; set; }

        public MemberList ConfiguredMemberList { get; set; }

        public IReadOnlyCollection<TypePair> IncludedDerivedTypes => _includedDerivedTypes ?? (IReadOnlyCollection<TypePair>)Array.Empty<TypePair>();
        public IReadOnlyCollection<TypePair> IncludedBaseTypes => _includedBaseTypes ?? (IReadOnlyCollection<TypePair>)Array.Empty<TypePair>();

        public IReadOnlyCollection<LambdaExpression> BeforeMapActions => _beforeMapActions ?? (IReadOnlyCollection<LambdaExpression>)Array.Empty<LambdaExpression>();
        public IReadOnlyCollection<LambdaExpression> AfterMapActions => _afterMapActions ?? (IReadOnlyCollection<LambdaExpression>)Array.Empty<LambdaExpression>();
        public IReadOnlyCollection<ValueTransformerConfiguration> ValueTransformers => _valueTransformerConfigs ?? (IReadOnlyCollection<ValueTransformerConfiguration>)Array.Empty<ValueTransformerConfiguration>();

        public bool PreserveReferences { get; set; }
        public LambdaExpression Condition { get; set; }

        public int MaxDepth { get; set; }

        public Type TypeConverterType { get; set; }
        public bool DisableConstructorValidation { get; set; }

        public IReadOnlyCollection<PropertyMap> PropertyMaps => _orderedPropertyMaps ?? (IReadOnlyCollection<PropertyMap>)_propertyMaps.Values;
        public IReadOnlyCollection<PathMap> PathMaps => _pathMaps?.Values ?? (IReadOnlyCollection<PathMap>)Array.Empty<PathMap>();
        public IEnumerable<IMemberMap> MemberMaps => PropertyMaps.Cast<IMemberMap>().Concat(PathMaps).Concat(GetConstructorMemberMaps());

        public bool? IsValid { get; set; }
        internal bool WasInlineChecked { get; set; }

        public bool PassesCtorValidation =>
            DisableConstructorValidation
            || CustomCtorExpression != null
            || CustomCtorFunction != null
            || ConstructDestinationUsingServiceLocator
            || ConstructorMap?.CanResolve == true
            || DestinationTypeToUse.IsInterface
            || DestinationTypeToUse.IsAbstract
            || DestinationTypeToUse.IsGenericTypeDefinition
            || DestinationTypeToUse.IsValueType
            || TypeDetails.GetConstructors(DestinationType, Profile).Any(c => c.AllParametersOptional());

        public MemberInfo[] DestinationSetters => DestinationTypeDetails.WriteAccessors;
        public ConstructorParameters[] DestinationConstructors => DestinationTypeDetails.Constructors;

        public bool IsConstructorMapping =>
            CustomCtorExpression == null
            && CustomCtorFunction == null
            && !ConstructDestinationUsingServiceLocator
            && (ConstructorMap?.CanResolve ?? false);

        public bool HasTypeConverter =>
            CustomMapFunction != null
            || CustomMapExpression != null
            || TypeConverterType != null;

        public bool ShouldCheckForValid =>
            !HasTypeConverter
            && DestinationTypeOverride == null
            && ConfiguredMemberList != MemberList.None
            && !(IsValid ?? false);

        public LambdaExpression[] IncludedMembers { get; internal set; } = Array.Empty<LambdaExpression>();
        public string[] IncludedMembersNames { get; internal set; } = Array.Empty<string>();

        public IReadOnlyCollection<IncludedMember> IncludedMembersTypeMaps => _includedMembersTypeMaps ?? (IReadOnlyCollection<IncludedMember>)Array.Empty<IncludedMember>();

        public Type MakeGenericType(Type type) => type.IsGenericTypeDefinition ?
            type.MakeGenericType(SourceType.GenericTypeArguments.Concat(DestinationType.GenericTypeArguments).Take(type.GetGenericParameters().Length).ToArray()) :
            type;

        public IEnumerable<LambdaExpression> GetAllIncludedMembers() => IncludedMembers.Concat(GetUntypedIncludedMembers());

        private IEnumerable<LambdaExpression> GetUntypedIncludedMembers() =>
            SourceType.IsGenericTypeDefinition ?
                Array.Empty<LambdaExpression>() :
                IncludedMembersNames.Select(name => ExpressionFactory.MemberAccessLambda(SourceType, name));

        public bool ConstructorParameterMatches(string destinationPropertyName) =>
            ConstructorMap.CtorParams.Any(c => string.Equals(c.Parameter.Name, destinationPropertyName, StringComparison.OrdinalIgnoreCase));

        public void AddPropertyMap(MemberInfo destProperty, IEnumerable<MemberInfo> resolvers)
        {
            var propertyMap = new PropertyMap(destProperty, this);

            propertyMap.ChainMembers(resolvers);

            AddPropertyMap(propertyMap);
        }

        private void AddPropertyMap(PropertyMap propertyMap) => _propertyMaps.Add(propertyMap.DestinationName, propertyMap);

        public string[] GetUnmappedPropertyNames()
        {
            var autoMappedProperties = GetPropertyNames(PropertyMaps);

            IEnumerable<string> properties;

            if (ConfiguredMemberList == MemberList.Destination)
            {
                properties = Profile.CreateTypeDetails(DestinationType).WriteAccessors
                    .Select(p => p.Name)
                    .Except(autoMappedProperties)
                    .Except(PathMaps.Select(p => p.MemberPath.First.Name));
                if (IsConstructorMapping)
                {
                    properties = properties.Where(p => !ConstructorParameterMatches(p));
                }
            }
            else
            {
                var redirectedSourceMembers = MemberMaps
                     .Where(pm => pm.IsMapped && pm.SourceMember != null && pm.SourceMember.Name != pm.DestinationName)
                     .Select(pm => pm.SourceMember.Name);

                var ignoredSourceMembers = _sourceMemberConfigs?.Values
                    .Where(smc => smc.IsIgnored())
                    .Select(pm => pm.SourceMember.Name);

                properties = Profile.CreateTypeDetails(SourceType).ReadAccessors
                    .Select(p => p.Name)
                    .Except(autoMappedProperties)
                    .Except(redirectedSourceMembers)
                    .Except(ignoredSourceMembers ?? Array.Empty<string>());
            }

            return properties.Where(memberName => !Profile.GlobalIgnores.Any(memberName.StartsWith)).ToArray();
            string GetPropertyName(PropertyMap pm) => ConfiguredMemberList == MemberList.Destination
                ? pm.DestinationName
                : pm.SourceMembers.Count > 1
                    ? pm.SourceMembers.First().Name
                    : pm.SourceMember?.Name ?? pm.DestinationName;
            string[] GetPropertyNames(IEnumerable<PropertyMap> propertyMaps) => propertyMaps.Where(pm => pm.IsMapped).Select(GetPropertyName).ToArray();
        }

        public PropertyMap FindOrCreatePropertyMapFor(MemberInfo destinationProperty)
        {
            var propertyMap = GetPropertyMap(destinationProperty.Name);

            if (propertyMap != null) return propertyMap;

            propertyMap = new PropertyMap(destinationProperty, this);

            AddPropertyMap(propertyMap);

            return propertyMap;
        }

        public void IncludeDerivedTypes(in TypePair derivedTypes)
        {
            CheckDifferent(derivedTypes);
            _includedDerivedTypes ??= new();
            _includedDerivedTypes.Add(derivedTypes);
        }

        private void CheckDifferent(in TypePair types)
        {
            if (types == Types)
            {
                throw new InvalidOperationException("You cannot include a type map into itself.");
            }
        }

        public void IncludeBaseTypes(in TypePair baseTypes)
        {
            CheckDifferent(baseTypes);
            _includedBaseTypes ??= new();
            _includedBaseTypes.Add(baseTypes);
        }

        internal void IgnorePaths(MemberInfo destinationMember)
        {
            foreach (var pathMap in PathMaps.Where(pm => pm.MemberPath.First == destinationMember))
            {
                pathMap.Ignored = true;
            }
        }

        public bool HasDerivedTypesToInclude => _includedDerivedTypes?.Count > 0 || DestinationTypeOverride != null;

        public bool Projection { get; set; }

        public void AddBeforeMapAction(LambdaExpression beforeMap)
        {
            _beforeMapActions ??= new();
            _beforeMapActions.Add(beforeMap);
        }

        public void AddAfterMapAction(LambdaExpression afterMap)
        {
            _afterMapActions ??= new();
            _afterMapActions.Add(afterMap);
        }

        public void AddValueTransformation(ValueTransformerConfiguration valueTransformerConfiguration)
        {
            _valueTransformerConfigs ??= new();
            _valueTransformerConfigs.Add(valueTransformerConfiguration);
        }

        public void Seal(IGlobalConfiguration configurationProvider)
        {
            if (_sealed)
            {
                return;
            }
            _sealed = true;
            if (_inheritedTypeMaps != null)
            {
                foreach (var inheritedTypeMap in _inheritedTypeMaps)
                {
                    if (inheritedTypeMap._includedMembersTypeMaps != null)
                    {
                        _includedMembersTypeMaps ??= new();
                        _includedMembersTypeMaps.UnionWith(inheritedTypeMap._includedMembersTypeMaps);
                    }
                }
            }
            if (_includedMembersTypeMaps != null)
            {
                foreach (var includedMemberTypeMap in _includedMembersTypeMaps)
                {
                    includedMemberTypeMap.TypeMap.Seal(configurationProvider);
                    ApplyIncludedMemberTypeMap(includedMemberTypeMap);
                }
            }
            if (_inheritedTypeMaps != null)
            {
                foreach (var inheritedTypeMap in _inheritedTypeMaps)
                {
                    ApplyInheritedTypeMap(inheritedTypeMap);
                }
            }
            if (!Projection)
            {
                if (HasMappingOrder())
                {
                    _orderedPropertyMaps = PropertyMaps.OrderBy(map => map.MappingOrder).ToArray();
                    _propertyMaps.Clear();
                }
                MapExpression = CreateMapperLambda(configurationProvider, null);
            }
            _features?.Seal(configurationProvider);
        }

        private bool HasMappingOrder()
        {
            foreach (var propertyMap in _propertyMaps.Values)
            {
                if (propertyMap.MappingOrder != null)
                {
                    return true;
                }
            }
            return false;
        }

        internal LambdaExpression CreateMapperLambda(IGlobalConfiguration configurationProvider, HashSet<TypeMap> typeMapsPath) =>
            Types.IsGenericTypeDefinition ? null : new TypeMapPlanBuilder(configurationProvider, this).CreateMapperLambda(typeMapsPath);

        private PropertyMap GetPropertyMap(string name) => _propertyMaps.GetOrDefault(name);

        private PropertyMap GetPropertyMap(PropertyMap propertyMap) => GetPropertyMap(propertyMap.DestinationName);

        public bool AddMemberMap(IncludedMember includedMember)
        {
            _includedMembersTypeMaps ??= new();
            return _includedMembersTypeMaps.Add(includedMember);
        }

        public SourceMemberConfig FindOrCreateSourceMemberConfigFor(MemberInfo sourceMember)
        {
            _sourceMemberConfigs ??= new();
            var config = _sourceMemberConfigs.GetOrDefault(sourceMember);

            if (config != null) return config;

            config = new SourceMemberConfig(sourceMember);
            AddSourceMemberConfig(config);
            return config;
        }

        private void AddSourceMemberConfig(SourceMemberConfig config) => _sourceMemberConfigs.Add(config.SourceMember, config);

        public bool AddInheritedMap(TypeMap inheritedTypeMap)
        {
            _inheritedTypeMaps ??= new();
            return _inheritedTypeMaps.Add(inheritedTypeMap);
        }

        private void ApplyIncludedMemberTypeMap(IncludedMember includedMember)
        {
            var typeMap = includedMember.TypeMap;
            var includedMemberMaps = typeMap.PropertyMaps.
                Where(m => m.CanResolveValue && GetPropertyMap(m) == null)
                .Select(p => new PropertyMap(p, this, includedMember))
                .ToArray();
            var notOverridenPathMaps = NotOverridenPathMaps(typeMap);
            if (includedMemberMaps.Length == 0 && notOverridenPathMaps.Length == 0)
            {
                return;
            }
            foreach (var includedMemberMap in includedMemberMaps)
            {
                AddPropertyMap(includedMemberMap);
                foreach (var transformer in typeMap.ValueTransformers)
                {
                    includedMemberMap.AddValueTransformation(transformer);
                }
            }
            ApplyIncludedMemberActions(includedMember, typeMap);
            foreach (var notOverridenPathMap in notOverridenPathMaps)
            {
                AddPathMap(new PathMap(notOverridenPathMap, this, includedMember) { CustomMapExpression = notOverridenPathMap.CustomMapExpression });
            }
        }

        private void ApplyIncludedMemberActions(IncludedMember includedMember, TypeMap typeMap)
        {
            if (typeMap._beforeMapActions != null)
            {
                _beforeMapActions ??= new();
                _beforeMapActions.UnionWith(typeMap._beforeMapActions.Select(includedMember.Chain));
            }
            if (typeMap._afterMapActions != null)
            {
                _afterMapActions ??= new();
                _afterMapActions.UnionWith(typeMap._afterMapActions.Select(includedMember.Chain));
            }
        }

        private void ApplyInheritedTypeMap(TypeMap inheritedTypeMap)
        {
            foreach (var inheritedMappedProperty in inheritedTypeMap.PropertyMaps.Where(m => m.IsMapped))
            {
                var conventionPropertyMap = GetPropertyMap(inheritedMappedProperty);

                if (conventionPropertyMap != null)
                {
                    conventionPropertyMap.ApplyInheritedPropertyMap(inheritedMappedProperty);
                }
                else
                {
                    AddPropertyMap(new PropertyMap(inheritedMappedProperty, this));
                }
            }
            ApplyInheritedMapActions(inheritedTypeMap);
            if (inheritedTypeMap._sourceMemberConfigs != null)
            {
                ApplyInheritedSourceMembers(inheritedTypeMap);
            }
            var notOverridenPathMaps = NotOverridenPathMaps(inheritedTypeMap);
            foreach (var notOverridenPathMap in notOverridenPathMaps)
            {
                AddPathMap(notOverridenPathMap);
            }
            if (inheritedTypeMap._valueTransformerConfigs != null)
            {
                _valueTransformerConfigs ??= new();
                _valueTransformerConfigs.InsertRange(0, inheritedTypeMap._valueTransformerConfigs);
            }
        }

        private void ApplyInheritedMapActions(TypeMap inheritedTypeMap)
        {
            if (inheritedTypeMap._beforeMapActions != null)
            {
                _beforeMapActions ??= new();
                _beforeMapActions.UnionWith(inheritedTypeMap._beforeMapActions);
            }
            if (inheritedTypeMap._afterMapActions != null)
            {
                _afterMapActions ??= new();
                _afterMapActions.UnionWith(inheritedTypeMap._afterMapActions);
            }
        }

        private void ApplyInheritedSourceMembers(TypeMap inheritedTypeMap)
        {
            _sourceMemberConfigs ??= new();
            foreach (var inheritedSourceConfig in inheritedTypeMap._sourceMemberConfigs.Values)
            {
                if (!_sourceMemberConfigs.ContainsKey(inheritedSourceConfig.SourceMember))
                {
                    AddSourceMemberConfig(inheritedSourceConfig);
                }
            }
        }

        private PathMap[] NotOverridenPathMaps(TypeMap inheritedTypeMap)
        {
            if (inheritedTypeMap.PathMaps.Count == 0)
            {
                return Array.Empty<PathMap>();
            }
            _pathMaps ??= new();
            return inheritedTypeMap.PathMaps.Where(baseConfig => !_pathMaps.ContainsKey(baseConfig.MemberPath)).ToArray();
        }

        internal void CopyInheritedMapsTo(TypeMap typeMap)
        {
            if (_inheritedTypeMaps == null)
            {
                return;
            }
            typeMap._inheritedTypeMaps ??= new();
            typeMap._inheritedTypeMaps.UnionWith(_inheritedTypeMaps);
        }

        private IEnumerable<IMemberMap> GetConstructorMemberMaps() => IsConstructorMapping ? ConstructorMap.CtorParams : Enumerable.Empty<IMemberMap>();
    }
}