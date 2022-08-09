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
    using Internal;
    /// <summary>
    /// Main configuration object holding all mapping configuration for a source and destination type
    /// </summary>
    [DebuggerDisplay("{SourceType.Name} -> {DestinationType.Name}")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TypeMap
    {
        private static readonly MethodInfo CreateProxyMethod = typeof(ObjectFactory).GetStaticMethod(nameof(ObjectFactory.CreateInterfaceProxy));
        private TypeMapDetails _details;
        private Dictionary<string, PropertyMap> _propertyMaps;
        private bool _sealed;
        public TypeMap(Type sourceType, Type destinationType, ProfileMap profile, ITypeMapConfiguration typeMapConfiguration = null, List<MemberInfo> sourceMembers = null)
        {
            Types = new(sourceType, destinationType);
            Profile = profile;
            if (typeMapConfiguration?.HasTypeConverter is true)
            {
                return;
            }
            SourceTypeDetails = profile.CreateTypeDetails(sourceType);
            DestinationTypeDetails = profile.CreateTypeDetails(destinationType);
            sourceMembers ??= new();
            foreach (var destinationProperty in DestinationTypeDetails.WriteAccessors)
            {
                var destinationName = destinationProperty.Name;
                var memberConfig = typeMapConfiguration?.GetDestinationMemberConfiguration(destinationProperty);
                if (memberConfig?.Ignored is true || profile.GlobalIgnores.Contains(destinationName))
                {
                    continue;
                }
                sourceMembers.Clear();
                var propertyType = destinationProperty.GetMemberType();
                if (profile.MapDestinationPropertyToSource(SourceTypeDetails, destinationType, propertyType, destinationName, sourceMembers,
                        typeMapConfiguration?.IsReverseMap is true))
                {
                    AddPropertyMap(destinationProperty, propertyType, sourceMembers);
                }
            }
        }
        public Features<IRuntimeFeature> Features => Details.Features;
        private TypeMapDetails Details => _details ??= new();
        public void CheckProjection()
        {
            if (Projection)
            {
                throw new AutoMapperConfigurationException("CreateProjection works with ProjectTo, not with Map.", MissingMapException(Types));
            }
        }
        public static Exception MissingMapException(TypePair types) => MissingMapException(types.SourceType, types.DestinationType);
        public static Exception MissingMapException(Type sourceType, Type destinationType)
            => new InvalidOperationException($"Missing map from {sourceType} to {destinationType}. Create using CreateMap<{sourceType.Name}, {destinationType.Name}>.");
        public bool Projection { get; set; }
        public LambdaExpression MapExpression { get; private set; }
        public Expression Invoke(Expression source, Expression destination) =>
            Expression.Invoke(MapExpression, ToType(source, SourceType), ToType(destination, DestinationType), ContextParameter);
        internal bool CanConstructorMap() => Profile.ConstructorMappingEnabled && !DestinationType.IsAbstract &&
            !CustomConstruction && !HasTypeConverter && DestinationConstructors.Length > 0;
        public TypePair Types;
        public ConstructorMap ConstructorMap { get; set; }
        public TypeDetails SourceTypeDetails { get; private set; }
        private TypeDetails DestinationTypeDetails { get; set; }
        public Type SourceType => Types.SourceType;
        public Type DestinationType => Types.DestinationType;
        public ProfileMap Profile { get; }
        public LambdaExpression CustomMapExpression => TypeConverter?.ProjectToExpression;
        public LambdaExpression CustomCtorFunction { get => _details?.CustomCtorFunction; set => Details.CustomCtorFunction = value; }
        public LambdaExpression CustomCtorExpression => CustomCtorFunction?.Parameters.Count == 1 ? CustomCtorFunction : null;
        public Type DestinationTypeOverride
        {
            get => _details?.DestinationTypeOverride;
            set
            {
                Details.DestinationTypeOverride = value;
                _sealed = true;
            }
        }
        public bool IncludeAllDerivedTypes { get => (_details?.IncludeAllDerivedTypes).GetValueOrDefault(); set => Details.IncludeAllDerivedTypes = value; }
        public MemberList ConfiguredMemberList
        {
            get => (_details?.ConfiguredMemberList).GetValueOrDefault();
            set
            {
                if (value == default)
                {
                    return;
                }
                Details.ConfiguredMemberList = value;
            }
        }
        public IReadOnlyCollection<TypePair> IncludedDerivedTypes => (_details?.IncludedDerivedTypes).NullCheck();
        public IReadOnlyCollection<TypePair> IncludedBaseTypes => (_details?.IncludedBaseTypes).NullCheck();
        public IReadOnlyCollection<LambdaExpression> BeforeMapActions => (_details?.BeforeMapActions).NullCheck();
        public IReadOnlyCollection<LambdaExpression> AfterMapActions => (_details?.AfterMapActions).NullCheck();
        public IReadOnlyCollection<ValueTransformerConfiguration> ValueTransformers => (_details?.ValueTransformerConfigs).NullCheck();
        public bool PreserveReferences { get => (_details?.PreserveReferences).GetValueOrDefault(); set => Details.PreserveReferences = value; }
        public int MaxDepth { get => (_details?.MaxDepth).GetValueOrDefault(); set => Details.MaxDepth = value; }
        public bool DisableConstructorValidation { get => (_details?.DisableConstructorValidation).GetValueOrDefault(); set => Details.DisableConstructorValidation = value; }
        public IReadOnlyCollection<PropertyMap> PropertyMaps => (_propertyMaps?.Values).NullCheck();
        public IReadOnlyCollection<PathMap> PathMaps => (_details?.PathMaps?.Values).NullCheck();
        public IEnumerable<MemberMap> MemberMaps
        {
            get
            {
                var maps = PropertyMaps.Concat((IReadOnlyCollection<MemberMap>)PathMaps);
                if (ConstructorMapping)
                {
                    maps = maps.Concat(ConstructorMap.CtorParams);
                }
                return maps;
            }
        }
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
        public bool CustomConstruction => CustomCtorFunction != null;
        public bool HasTypeConverter => TypeConverter != null;
        public TypeConverter TypeConverter { get; set; }
        public bool ShouldCheckForValid => !HasTypeConverter && DestinationTypeOverride == null && ConfiguredMemberList != MemberList.None;
        public LambdaExpression[] IncludedMembers { get => _details?.IncludedMembers ?? Array.Empty<LambdaExpression>(); set => Details.IncludedMembers = value; }
        public string[] IncludedMembersNames { get => _details?.IncludedMembersNames ?? Array.Empty<string>(); set => Details.IncludedMembersNames = value; }
        public IReadOnlyCollection<IncludedMember> IncludedMembersTypeMaps => (_details?.IncludedMembersTypeMaps).NullCheck();
        public Type MakeGenericType(Type type) => type.IsGenericTypeDefinition ?
            type.MakeGenericType(SourceType.GenericTypeArguments.Concat(DestinationType.GenericTypeArguments).Take(type.GenericParametersCount()).ToArray()) :
            type;
        public bool HasIncludedMembers => IncludedMembers.Length > 0 || IncludedMembersNames.Length > 0;
        public IEnumerable<LambdaExpression> GetAllIncludedMembers() => IncludedMembersNames.Length == 0 ||  SourceType.ContainsGenericParameters ?
            IncludedMembers : IncludedMembers.Concat(IncludedMembersNames.Select(name => MemberAccessLambda(SourceType, name)));
        public bool ConstructorParameterMatches(string destinationPropertyName) => ConstructorMapping && ConstructorMap[destinationPropertyName] != null;
        private void AddPropertyMap(MemberInfo destProperty, Type destinationPropertyType, List<MemberInfo> sourceMembers)
        {
            var propertyMap = new PropertyMap(destProperty, destinationPropertyType, this);
            propertyMap.MapByConvention(sourceMembers.ToArray());
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
                var ignoredSourceMembers = _details?.SourceMemberConfigs?.Values
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
        private void CheckDifferent(TypePair types)
        {
            if (types == Types)
            {
                throw new InvalidOperationException($"You cannot include a type map into itself.{Environment.NewLine}Source type: {types.SourceType.FullName}{Environment.NewLine}Destination type: {types.DestinationType.FullName}");
            }
        }
        internal void IgnorePaths(MemberInfo destinationMember)
        {
            foreach (var pathMap in PathMaps.Where(pm => pm.MemberPath.First == destinationMember))
            {
                pathMap.Ignored = true;
            }
        }
        public bool HasDerivedTypesToInclude => IncludedDerivedTypes.Count > 0;
        public void Seal(IGlobalConfiguration configurationProvider, HashSet<TypeMap> typeMapsPath)
        {
            if (_sealed)
            {
                return;
            }
            _sealed = true;
            _details?.Seal(configurationProvider, this, typeMapsPath);
            if (!Projection)
            {
                MapExpression = CreateMapperLambda(configurationProvider, typeMapsPath);
            }
            SourceTypeDetails = null;
            DestinationTypeDetails = null;
        }
        public IEnumerable<PropertyMap> OrderedPropertyMaps()
        {
            if (HasMappingOrder())
            {
                return PropertyMaps.OrderBy(map => map.MappingOrder);
            }
            else
            {
                return PropertyMaps;
            }
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
        public void IncludeDerivedTypes(TypePair derivedTypes)
        {
            CheckDifferent(derivedTypes);
            Details.IncludeDerivedTypes(derivedTypes);
        }
        public void IncludeBaseTypes(TypePair baseTypes)
        {
            CheckDifferent(baseTypes);
            Details.IncludeBaseTypes(baseTypes);
        }
        public void AddBeforeMapAction(LambdaExpression beforeMap) => Details.AddBeforeMapAction(beforeMap);
        public void AddAfterMapAction(LambdaExpression afterMap) => Details.AddAfterMapAction(afterMap);
        public void AddValueTransformation(ValueTransformerConfiguration config) => Details.AddValueTransformation(config);
        public void ConstructUsingServiceLocator() => CustomCtorFunction = Lambda(ServiceLocator(DestinationType));
        internal LambdaExpression CreateMapperLambda(IGlobalConfiguration configurationProvider, HashSet<TypeMap> typeMapsPath) =>
            Types.IsGenericTypeDefinition ? null : new TypeMapPlanBuilder(configurationProvider, this).CreateMapperLambda(typeMapsPath);
        private PropertyMap GetPropertyMap(string name) => _propertyMaps?.GetValueOrDefault(name);
        private PropertyMap GetPropertyMap(PropertyMap propertyMap) => GetPropertyMap(propertyMap.DestinationName);
        public void AsProxy() => CustomCtorFunction = Lambda(Call(CreateProxyMethod, Constant(DestinationType)));
        internal void CopyInheritedMapsTo(TypeMap typeMap)
        {
            if (_details?.InheritedTypeMaps == null)
            {
                return;
            }
            _details.CopyInheritedMapsTo(typeMap);
        }
        public void CloseGenerics(ITypeMapConfiguration openMapConfig, TypePair closedTypes)
        {
            TypeConverter?.CloseGenerics(openMapConfig, closedTypes);
            if (DestinationTypeOverride is { IsGenericTypeDefinition: true })
            {
                var neededParameters = DestinationTypeOverride.GenericParametersCount();
                DestinationTypeOverride = DestinationTypeOverride.MakeGenericType(closedTypes.DestinationType.GenericTypeArguments.Take(neededParameters).ToArray());
            }
        }
        public bool AddMemberMap(IncludedMember includedMember) => Details.AddMemberMap(includedMember);
        public PathMap FindOrCreatePathMapFor(LambdaExpression destinationExpression, MemberPath path, TypeMap typeMap) =>
            Details.FindOrCreatePathMapFor(destinationExpression, path, typeMap);
        public bool AddInheritedMap(TypeMap inheritedTypeMap) => Details.AddInheritedMap(inheritedTypeMap);
        public SourceMemberConfig FindOrCreateSourceMemberConfigFor(MemberInfo sourceMember) => Details.FindOrCreateSourceMemberConfigFor(sourceMember);
        class TypeMapDetails
        {
            Features<IRuntimeFeature> _features;
            public bool PreserveReferences;
            public LambdaExpression[] IncludedMembers;
            public string[] IncludedMembersNames;
            public bool DisableConstructorValidation;
            public int MaxDepth;
            public bool IncludeAllDerivedTypes;
            public LambdaExpression CustomCtorFunction;
            public MemberList ConfiguredMemberList;
            public Type DestinationTypeOverride;
            public HashSet<LambdaExpression> AfterMapActions { get; private set; }
            public HashSet<LambdaExpression> BeforeMapActions { get; private set; }
            public HashSet<TypePair> IncludedDerivedTypes { get; private set; }
            public HashSet<TypePair> IncludedBaseTypes { get; private set; }
            public Dictionary<MemberPath, PathMap> PathMaps { get; private set; }
            public Dictionary<MemberInfo, SourceMemberConfig> SourceMemberConfigs { get; private set; }
            public HashSet<TypeMap> InheritedTypeMaps { get; private set; }
            public HashSet<IncludedMember> IncludedMembersTypeMaps { get; private set; }
            public List<ValueTransformerConfiguration> ValueTransformerConfigs { get; private set; }
            public Features<IRuntimeFeature> Features => _features ??= new();
            public void Seal(IGlobalConfiguration configurationProvider, TypeMap thisMap, HashSet<TypeMap> typeMapsPath)
            {
                if (InheritedTypeMaps != null)
                {
                    foreach (var inheritedTypeMap in InheritedTypeMaps)
                    {
                        var includedMaps = inheritedTypeMap?._details?.IncludedMembersTypeMaps;
                        if (includedMaps != null)
                        {
                            IncludedMembersTypeMaps ??= new();
                            IncludedMembersTypeMaps.UnionWith(includedMaps);
                        }
                    }
                }
                if (IncludedMembersTypeMaps != null)
                {
                    foreach (var includedMemberTypeMap in IncludedMembersTypeMaps)
                    {
                        includedMemberTypeMap.TypeMap.Seal(configurationProvider, typeMapsPath);
                        ApplyIncludedMemberTypeMap(includedMemberTypeMap, thisMap);
                    }
                }
                if (InheritedTypeMaps != null)
                {
                    foreach (var inheritedTypeMap in InheritedTypeMaps)
                    {
                        ApplyInheritedTypeMap(inheritedTypeMap, thisMap);
                    }
                }
                _features?.Seal(configurationProvider);
            }
            public void IncludeDerivedTypes(TypePair derivedTypes)
            {
                IncludedDerivedTypes ??= new();
                IncludedDerivedTypes.Add(derivedTypes);
            }
            public void AddBeforeMapAction(LambdaExpression beforeMap)
            {
                BeforeMapActions ??= new();
                BeforeMapActions.Add(beforeMap);
            }
            public void AddAfterMapAction(LambdaExpression afterMap)
            {
                AfterMapActions ??= new();
                AfterMapActions.Add(afterMap);
            }
            public void AddValueTransformation(ValueTransformerConfiguration valueTransformerConfiguration)
            {
                ValueTransformerConfigs ??= new();
                ValueTransformerConfigs.Add(valueTransformerConfiguration);
            }
            public PathMap FindOrCreatePathMapFor(LambdaExpression destinationExpression, MemberPath path, TypeMap typeMap)
            {
                PathMaps ??= new();
                var pathMap = PathMaps.GetValueOrDefault(path);
                if (pathMap == null)
                {
                    pathMap = new(destinationExpression, path, typeMap);
                    AddPathMap(pathMap);
                }
                return pathMap;
            }
            private void AddPathMap(PathMap pathMap) => PathMaps.Add(pathMap.MemberPath, pathMap);
            public void IncludeBaseTypes(TypePair baseTypes)
            {
                IncludedBaseTypes ??= new();
                IncludedBaseTypes.Add(baseTypes);
            }
            internal void CopyInheritedMapsTo(TypeMap typeMap)
            {
                typeMap.Details.InheritedTypeMaps ??= new();
                typeMap._details.InheritedTypeMaps.UnionWith(InheritedTypeMaps);
            }
            public bool AddMemberMap(IncludedMember includedMember)
            {
                IncludedMembersTypeMaps ??= new();
                return IncludedMembersTypeMaps.Add(includedMember);
            }
            public SourceMemberConfig FindOrCreateSourceMemberConfigFor(MemberInfo sourceMember)
            {
                SourceMemberConfigs ??= new();
                var config = SourceMemberConfigs.GetValueOrDefault(sourceMember);

                if (config != null) return config;

                config = new(sourceMember);
                SourceMemberConfigs.Add(config.SourceMember, config);
                return config;
            }
            public bool AddInheritedMap(TypeMap inheritedTypeMap)
            {
                InheritedTypeMaps ??= new();
                return InheritedTypeMaps.Add(inheritedTypeMap);
            }
            private void ApplyIncludedMemberTypeMap(IncludedMember includedMember, TypeMap thisMap)
            {
                var typeMap = includedMember.TypeMap;
                var includedMemberMaps = typeMap.PropertyMaps.
                    Where(m => m.CanResolveValue && thisMap.GetPropertyMap(m) == null)
                    .Select(p => new PropertyMap(p, thisMap, includedMember))
                    .ToArray();
                var notOverridenPathMaps = NotOverridenPathMaps(typeMap);
                var appliedConstructorMap = thisMap.ConstructorMap?.ApplyIncludedMember(includedMember);
                if (includedMemberMaps.Length == 0 && notOverridenPathMaps.Length == 0 && appliedConstructorMap is not true)
                {
                    return;
                }
                foreach (var includedMemberMap in includedMemberMaps)
                {
                    thisMap.AddPropertyMap(includedMemberMap);
                    foreach (var transformer in typeMap.ValueTransformers)
                    {
                        includedMemberMap.AddValueTransformation(transformer);
                    }
                }
                var details = typeMap._details;
                if (details != null)
                {
                    ApplyInheritedMapActions(details.BeforeMapActions?.Select(includedMember.Chain), details.AfterMapActions?.Select(includedMember.Chain));
                }
                foreach (var notOverridenPathMap in notOverridenPathMaps)
                {
                    AddPathMap(new(notOverridenPathMap, thisMap, includedMember));
                }
            }
            private void ApplyInheritedTypeMap(TypeMap inheritedTypeMap, TypeMap thisMap)
            {
                if (inheritedTypeMap._propertyMaps != null)
                {
                    ApplyInheritedPropertyMaps(inheritedTypeMap, thisMap);
                }
                var inheritedDetails = inheritedTypeMap._details;
                if (inheritedDetails == null)
                {
                    return;
                }
                ApplyInheritedMapActions(inheritedDetails.BeforeMapActions, inheritedDetails.AfterMapActions);
                if (inheritedDetails.SourceMemberConfigs != null)
                {
                    ApplyInheritedSourceMembers(inheritedTypeMap._details);
                }
                var notOverridenPathMaps = NotOverridenPathMaps(inheritedTypeMap);
                foreach (var notOverridenPathMap in notOverridenPathMaps)
                {
                    AddPathMap(notOverridenPathMap);
                }
                if (inheritedDetails.ValueTransformerConfigs != null)
                {
                    ValueTransformerConfigs ??= new();
                    ValueTransformerConfigs.InsertRange(0, inheritedDetails.ValueTransformerConfigs);
                }
                return;
                void ApplyInheritedPropertyMaps(TypeMap inheritedTypeMap, TypeMap thisMap)
                {
                    foreach (var inheritedMappedProperty in inheritedTypeMap._propertyMaps.Values)
                    {
                        if (!inheritedMappedProperty.IsMapped)
                        {
                            continue;
                        }
                        var conventionPropertyMap = thisMap.GetPropertyMap(inheritedMappedProperty);
                        if (conventionPropertyMap != null)
                        {
                            conventionPropertyMap.ApplyInheritedPropertyMap(inheritedMappedProperty);
                        }
                        else
                        {
                            thisMap.AddPropertyMap(new(inheritedMappedProperty, thisMap));
                        }
                    }
                }
                void ApplyInheritedSourceMembers(TypeMapDetails inheritedTypeMap)
                {
                    SourceMemberConfigs ??= new();
                    foreach (var inheritedSourceConfig in inheritedTypeMap.SourceMemberConfigs.Values)
                    {
                        SourceMemberConfigs.TryAdd(inheritedSourceConfig.SourceMember, inheritedSourceConfig);
                    }
                }
            }
            void ApplyInheritedMapActions(IEnumerable<LambdaExpression> beforeMap, IEnumerable<LambdaExpression> afterMap)
            {
                if (beforeMap != null)
                {
                    BeforeMapActions ??= new();
                    BeforeMapActions.UnionWith(beforeMap);
                }
                if (afterMap != null)
                {
                    AfterMapActions ??= new();
                    AfterMapActions.UnionWith(afterMap);
                }
            }
            private PathMap[] NotOverridenPathMaps(TypeMap inheritedTypeMap)
            {
                if (inheritedTypeMap.PathMaps.Count == 0)
                {
                    return Array.Empty<PathMap>();
                }
                PathMaps ??= new();
                return inheritedTypeMap.PathMaps.Where(baseConfig => !PathMaps.ContainsKey(baseConfig.MemberPath)).ToArray();
            }
        }
    }
}