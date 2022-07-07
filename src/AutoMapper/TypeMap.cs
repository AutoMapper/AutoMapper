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
    using static Expression;
    using static Execution.ExpressionBuilder;
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
        private static readonly MethodInfo CreateProxyMethod = typeof(ObjectFactory).GetStaticMethod(nameof(ObjectFactory.CreateInterfaceProxy));
        private Dictionary<string, PropertyMap> _propertyMaps;
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
        private Type _destinationTypeOverride;
        public TypeMap(Type sourceType, Type destinationType, ProfileMap profile, ITypeMapConfiguration typeMapConfiguration = null)
        {
            Types = new(sourceType, destinationType);
            Profile = profile;
            if (typeMapConfiguration?.HasTypeConverter is true)
            {
                return;
            }
            SourceTypeDetails = profile.CreateTypeDetails(sourceType);
            DestinationTypeDetails = profile.CreateTypeDetails(destinationType);
            var sourceMembers = new List<MemberInfo>();
            foreach (var destinationProperty in DestinationTypeDetails.WriteAccessors)
            {
                sourceMembers.Clear();
                var propertyType = destinationProperty.GetMemberType();
                if (profile.MapDestinationPropertyToSource(SourceTypeDetails, destinationType, propertyType, destinationProperty.Name, sourceMembers,
                        typeMapConfiguration?.IsReverseMap is true))
                {
                    AddPropertyMap(destinationProperty, propertyType, sourceMembers);
                }
            }
        }
        public void CheckProjection()
        {
            if (Projection)
            {
                throw new AutoMapperConfigurationException("CreateProjection works with ProjectTo, not with Map.", QueryMapperHelper.MissingMapException(Types));
            }
        }
        public PathMap FindOrCreatePathMapFor(LambdaExpression destinationExpression, MemberPath path, TypeMap typeMap)
        {
            _pathMaps ??= new();
            var pathMap = _pathMaps.GetValueOrDefault(path);
            if (pathMap == null)
            {
                pathMap = new(destinationExpression, path, typeMap);
                AddPathMap(pathMap);
            }
            return pathMap;
        }
        private void AddPathMap(PathMap pathMap) => _pathMaps.Add(pathMap.MemberPath, pathMap);
        public Features<IRuntimeFeature> Features => _features ??= new();
        public LambdaExpression MapExpression { get; private set; }
        internal bool CanConstructorMap() => Profile.ConstructorMappingEnabled && !DestinationType.IsAbstract &&
            !CustomConstruction && !HasTypeConverter && DestinationConstructors.Length > 0;
        public TypePair Types;
        public ConstructorMap ConstructorMap { get; set; }
        public TypeDetails SourceTypeDetails { get; private set; }
        private TypeDetails DestinationTypeDetails { get; set; }
        public Type SourceType => Types.SourceType;
        public Type DestinationType => Types.DestinationType;
        public ProfileMap Profile { get; }
        public LambdaExpression CustomMapExpression { get; set; }
        public LambdaExpression CustomCtorFunction { get; set; }
        public LambdaExpression CustomCtorExpression { get; set; }
        public Type DestinationTypeOverride
        {
            get => _destinationTypeOverride;
            set
            {
                _destinationTypeOverride = value;
                _sealed = true;
            }
        }
        public bool IncludeAllDerivedTypes { get; set; }
        public MemberList ConfiguredMemberList { get; set; }
        public IReadOnlyCollection<TypePair> IncludedDerivedTypes => _includedDerivedTypes.NullCheck();
        public IReadOnlyCollection<TypePair> IncludedBaseTypes => _includedBaseTypes.NullCheck();
        public IReadOnlyCollection<LambdaExpression> BeforeMapActions => _beforeMapActions.NullCheck();
        public IReadOnlyCollection<LambdaExpression> AfterMapActions => _afterMapActions.NullCheck();
        public IReadOnlyCollection<ValueTransformerConfiguration> ValueTransformers => _valueTransformerConfigs.NullCheck();
        public bool PreserveReferences { get; set; }
        public int MaxDepth { get; set; }
        public bool DisableConstructorValidation { get; set; }
        public IReadOnlyCollection<PropertyMap> PropertyMaps => _orderedPropertyMaps ?? (_propertyMaps?.Values).NullCheck();
        public IReadOnlyCollection<PathMap> PathMaps => (_pathMaps?.Values).NullCheck();
        public IEnumerable<MemberMap> MemberMaps
        {
            get
            {
                IEnumerable<MemberMap> maps = PropertyMaps;
                if (_pathMaps != null)
                {
                    maps = maps.Concat(_pathMaps.Values);
                }
                if (ConstructorMapping)
                {
                    maps = maps.Concat(ConstructorMap.CtorParams);
                }
                return maps;
            }
        }
        public bool? IsValid { get; set; }
        public bool PassesCtorValidation =>
            DisableConstructorValidation
            || CustomConstruction
            || ConstructorMapping
            || DestinationType.IsAbstract
            || DestinationType.IsGenericTypeDefinition
            || DestinationType.IsValueType
            || TypeDetails.GetConstructors(DestinationType, Profile).Any(c => c.AllParametersOptional());
        public MemberInfo[] DestinationSetters => DestinationTypeDetails.WriteAccessors;
        public ConstructorParameters[] DestinationConstructors => DestinationTypeDetails.Constructors;
        public bool ConstructorMapping => ConstructorMap is { CanResolve: true };
        public bool CustomConstruction => (CustomCtorExpression ?? CustomCtorFunction) != null;
        public bool HasTypeConverter => TypeConverter != null;
        public TypeConverter TypeConverter { get; set; }
        public bool ShouldCheckForValid =>
            !HasTypeConverter
            && DestinationTypeOverride == null
            && ConfiguredMemberList != MemberList.None
            && !(IsValid ?? false);
        public LambdaExpression[] IncludedMembers { get; internal set; } = Array.Empty<LambdaExpression>();
        public string[] IncludedMembersNames { get; internal set; } = Array.Empty<string>();
        public IReadOnlyCollection<IncludedMember> IncludedMembersTypeMaps => _includedMembersTypeMaps.NullCheck();
        public Type MakeGenericType(Type type) => type.IsGenericTypeDefinition ?
            type.MakeGenericType(SourceType.GenericTypeArguments.Concat(DestinationType.GenericTypeArguments).Take(type.GenericParametersCount()).ToArray()) :
            type;
        public bool HasIncludedMembers => IncludedMembers.Length > 0 || IncludedMembersNames.Length > 0;
        public IEnumerable<LambdaExpression> GetAllIncludedMembers() => IncludedMembers.Concat(GetUntypedIncludedMembers());
        private IEnumerable<LambdaExpression> GetUntypedIncludedMembers() =>
            SourceType.IsGenericTypeDefinition ?
                Array.Empty<LambdaExpression>() :
                IncludedMembersNames.Select(name => ExpressionBuilder.MemberAccessLambda(SourceType, name));
        public bool ConstructorParameterMatches(string destinationPropertyName) => ConstructorMapping && ConstructorMap[destinationPropertyName] != null;
        public void AddPropertyMap(MemberInfo destProperty, Type destinationPropertyType, IEnumerable<MemberInfo> sourceMembers)
        {
            var propertyMap = new PropertyMap(destProperty, destinationPropertyType, this);
            propertyMap.MapByConvention(sourceMembers);
            AddPropertyMap(propertyMap);
        }
        private void AddPropertyMap(PropertyMap propertyMap)
        {
            _propertyMaps ??= new();
            _propertyMaps.Add(propertyMap.DestinationName, propertyMap);
        }
        public string[] GetUnmappedPropertyNames()
        {
            IEnumerable<string> properties;
            if (ConfiguredMemberList == MemberList.Destination)
            {
                properties = Profile.CreateTypeDetails(DestinationType).WriteAccessors
                    .Select(p => p.Name)
                    .Where(p => !ConstructorParameterMatches(p))
                    .Except(MappedMembers().Select(m => m.DestinationName))
                    .Except(PathMaps.Select(p => p.MemberPath.First.Name));
            }
            else
            {
                var ignoredSourceMembers = _sourceMemberConfigs?.Values
                    .Where(smc => smc.IsIgnored())
                    .Select(pm => pm.SourceMember.Name);
                properties = Profile.CreateTypeDetails(SourceType).ReadAccessors
                    .Select(p => p.Name)
                    .Except(MappedMembers().Select(m => m.GetSourceMemberName()))
                    .Except(IncludedMembersNames)
                    .Except(IncludedMembers.Select(m => m.GetMember()?.Name))
                    .Except(ignoredSourceMembers ?? Array.Empty<string>());
            }
            return properties.Where(memberName => !Profile.GlobalIgnores.Any(memberName.StartsWith)).ToArray();
            IEnumerable<MemberMap> MappedMembers() => MemberMaps.Where(pm => pm.IsMapped);
        }
        public PropertyMap FindOrCreatePropertyMapFor(MemberInfo destinationProperty, Type destinationPropertyType)
        {
            var propertyMap = GetPropertyMap(destinationProperty.Name);
            if (propertyMap != null) return propertyMap;

            propertyMap = new(destinationProperty, destinationPropertyType, this);

            AddPropertyMap(propertyMap);
            return propertyMap;
        }
        public TypePair AsPair() => new(SourceType, DestinationTypeOverride);
        public void IncludeDerivedTypes(TypePair derivedTypes)
        {
            CheckDifferent(derivedTypes);
            _includedDerivedTypes ??= new();
            _includedDerivedTypes.Add(derivedTypes);
        }
        private void CheckDifferent(TypePair types)
        {
            if (types == Types)
            {
                throw new InvalidOperationException($"You cannot include a type map into itself.{Environment.NewLine}Source type: {types.SourceType.FullName}{Environment.NewLine}Destination type: {types.DestinationType.FullName}");
            }
        }
        public void IncludeBaseTypes(TypePair baseTypes)
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
        public void Seal(IGlobalConfiguration configurationProvider, HashSet<TypeMap> typeMapsPath)
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
                    includedMemberTypeMap.TypeMap.Seal(configurationProvider, typeMapsPath);
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
                MapExpression = CreateMapperLambda(configurationProvider, typeMapsPath);
            }
            _features?.Seal(configurationProvider);
            SourceTypeDetails = null;
            DestinationTypeDetails = null;
            return;
            bool HasMappingOrder()
            {
                if (_propertyMaps == null)
                {
                    return false;
                }
                foreach (var propertyMap in _propertyMaps.Values)
                {
                    if (propertyMap.MappingOrder != null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public void ConstructUsingServiceLocator() => CustomCtorFunction = Lambda(ServiceLocator(DestinationType));
        internal LambdaExpression CreateMapperLambda(IGlobalConfiguration configurationProvider, HashSet<TypeMap> typeMapsPath) =>
            Types.IsGenericTypeDefinition ? null : new TypeMapPlanBuilder(configurationProvider, this).CreateMapperLambda(typeMapsPath);
        private PropertyMap GetPropertyMap(string name) => _propertyMaps?.GetValueOrDefault(name);
        private PropertyMap GetPropertyMap(PropertyMap propertyMap) => GetPropertyMap(propertyMap.DestinationName);
        public bool AddMemberMap(IncludedMember includedMember)
        {
            _includedMembersTypeMaps ??= new();
            return _includedMembersTypeMaps.Add(includedMember);
        }
        public SourceMemberConfig FindOrCreateSourceMemberConfigFor(MemberInfo sourceMember)
        {
            _sourceMemberConfigs ??= new();
            var config = _sourceMemberConfigs.GetValueOrDefault(sourceMember);

            if (config != null) return config;

            config = new(sourceMember);
            _sourceMemberConfigs.Add(config.SourceMember, config);
            return config;
        }
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
            var appliedConstructorMap = ConstructorMap?.ApplyIncludedMember(includedMember);
            if (includedMemberMaps.Length == 0 && notOverridenPathMaps.Length == 0 && appliedConstructorMap is not true)
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
                AddPathMap(new(notOverridenPathMap, this, includedMember) { CustomMapExpression = notOverridenPathMap.CustomMapExpression });
            }
            return;
            void ApplyIncludedMemberActions(IncludedMember includedMember, TypeMap typeMap)
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
        }
        private void ApplyInheritedTypeMap(TypeMap inheritedTypeMap)
        {
            if (inheritedTypeMap._propertyMaps != null)
            {
                ApplyInheritedPropertyMaps(inheritedTypeMap);
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
            return;
            void ApplyInheritedPropertyMaps(TypeMap inheritedTypeMap)
            {
                foreach (var inheritedMappedProperty in inheritedTypeMap._propertyMaps.Values)
                {
                    if (!inheritedMappedProperty.IsMapped)
                    {
                        continue;
                    }
                    var conventionPropertyMap = GetPropertyMap(inheritedMappedProperty);
                    if (conventionPropertyMap != null)
                    {
                        conventionPropertyMap.ApplyInheritedPropertyMap(inheritedMappedProperty);
                    }
                    else
                    {
                        AddPropertyMap(new(inheritedMappedProperty, this));
                    }
                }
            }
            void ApplyInheritedMapActions(TypeMap inheritedTypeMap)
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
            void ApplyInheritedSourceMembers(TypeMap inheritedTypeMap)
            {
                _sourceMemberConfigs ??= new();
                foreach (var inheritedSourceConfig in inheritedTypeMap._sourceMemberConfigs.Values)
                {
                    _sourceMemberConfigs.TryAdd(inheritedSourceConfig.SourceMember, inheritedSourceConfig);
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
        public void AsProxy() => CustomCtorFunction = Lambda(Call(CreateProxyMethod, Constant(DestinationType)));
        public void CloseGenerics(ITypeMapConfiguration openMapConfig, TypePair closedTypes)
        {
            TypeConverter?.CloseGenerics(openMapConfig, closedTypes);
            if (DestinationTypeOverride is { IsGenericTypeDefinition: true })
            {
                var neededParameters = DestinationTypeOverride.GenericParametersCount();
                DestinationTypeOverride = DestinationTypeOverride.MakeGenericType(closedTypes.DestinationType.GenericTypeArguments.Take(neededParameters).ToArray());
            }
        }
    }
}