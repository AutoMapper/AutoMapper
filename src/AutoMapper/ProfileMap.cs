using AutoMapper.Configuration.Conventions;
using System.Collections.Concurrent;
namespace AutoMapper;
[DebuggerDisplay("{Name}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class ProfileMap
{
    private static readonly HashSet<string> EmptyHashSet = new();
    private TypeMapConfiguration[] _typeMapConfigs;
    private Dictionary<TypePair, TypeMapConfiguration> _openTypeMapConfigs;
    private Dictionary<Type, TypeDetails> _typeDetails;
    private ConcurrentDictionary<Type, TypeDetails> _runtimeTypeDetails;
    public ProfileMap(IProfileConfiguration profile, IGlobalConfigurationExpression configuration = null)
    {
        var globalProfile = (IProfileConfiguration)configuration;
        Name = profile.ProfileName;
        AllowNullCollections = profile.AllowNullCollections ?? configuration?.AllowNullCollections ?? false;
        AllowNullDestinationValues = profile.AllowNullDestinationValues ?? configuration?.AllowNullDestinationValues ?? true;
        EnableNullPropagationForQueryMapping = profile.EnableNullPropagationForQueryMapping ?? configuration?.EnableNullPropagationForQueryMapping ?? false;
        ConstructorMappingEnabled = profile.ConstructorMappingEnabled ?? globalProfile?.ConstructorMappingEnabled ?? true;
        MethodMappingEnabled = profile.MethodMappingEnabled ?? globalProfile?.MethodMappingEnabled ?? true;
        FieldMappingEnabled = profile.FieldMappingEnabled ?? globalProfile?.FieldMappingEnabled ?? true;
        ShouldMapField = profile.ShouldMapField ?? configuration?.ShouldMapField ?? (p => p.IsPublic);
        ShouldMapProperty = profile.ShouldMapProperty ?? configuration?.ShouldMapProperty ?? (p => p.IsPublic());
        ShouldMapMethod = profile.ShouldMapMethod ?? configuration?.ShouldMapMethod ?? (p => !p.IsSpecialName);
        ShouldUseConstructor = profile.ShouldUseConstructor ?? configuration?.ShouldUseConstructor ?? (c => true);
        ValueTransformers = profile.ValueTransformers.Concat(configuration?.ValueTransformers).ToArray();
        var profileInternal = (IProfileExpressionInternal)profile;
        MemberConfiguration = profileInternal.MemberConfiguration;
        MemberConfiguration.Merge(configuration.Internal()?.MemberConfiguration);
        var globalIgnores = profile.GlobalIgnores.Concat(globalProfile?.GlobalIgnores);
        GlobalIgnores = globalIgnores == Array.Empty<string>() ? EmptyHashSet : new HashSet<string>(globalIgnores);
        SourceExtensionMethods = profile.SourceExtensionMethods.Concat(globalProfile?.SourceExtensionMethods).ToArray();
        AllPropertyMapActions = profile.AllPropertyMapActions.Concat(globalProfile?.AllPropertyMapActions).ToArray();
        AllTypeMapActions = profile.AllTypeMapActions.Concat(globalProfile?.AllTypeMapActions).ToArray();
        profileInternal.MemberConfiguration.Seal();
        Prefixes.TryAdd(profileInternal.Prefixes.Concat(configuration?.Prefixes));
        Postfixes.TryAdd(profileInternal.Postfixes.Concat(configuration?.Postfixes));
        TypeMapConfigs();
        OpenTypeMapConfigs();
        _typeDetails = new(2 * _typeMapConfigs.Length);
        return;
        void TypeMapConfigs()
        {
            _typeMapConfigs = new TypeMapConfiguration[profile.TypeMapConfigs.Count];
            var index = 0;
            var reverseMapsCount = 0;
            foreach (var typeMapConfig in profile.TypeMapConfigs)
            {
                _typeMapConfigs[index++] = typeMapConfig;
                if (typeMapConfig.ReverseTypeMap != null)
                {
                    reverseMapsCount++;
                }
            }
            TypeMapsCount = index + reverseMapsCount;
        }
        void OpenTypeMapConfigs()
        {
            _openTypeMapConfigs = new(profile.OpenTypeMapConfigs.Count);
            foreach (var openTypeMapConfig in profile.OpenTypeMapConfigs)
            {
                _openTypeMapConfigs.Add(openTypeMapConfig.Types, openTypeMapConfig);
                var reverseMap = openTypeMapConfig.ReverseTypeMap;
                if (reverseMap != null)
                {
                    _openTypeMapConfigs.Add(reverseMap.Types, reverseMap);
                }
            }
        }
    }
    public int OpenTypeMapsCount => _openTypeMapConfigs.Count;
    public int TypeMapsCount { get; private set; }
    internal void Clear()
    {
        _typeDetails = null;
        _typeMapConfigs = null;
    }
    public bool AllowNullCollections { get; }
    public bool AllowNullDestinationValues { get; }
    public bool ConstructorMappingEnabled { get; }
    public bool EnableNullPropagationForQueryMapping { get; }
    public bool MethodMappingEnabled { get; }
    public bool FieldMappingEnabled { get; }
    public string Name { get; }
    public Func<FieldInfo, bool> ShouldMapField { get; }
    public Func<PropertyInfo, bool> ShouldMapProperty { get; }
    public Func<MethodInfo, bool> ShouldMapMethod { get; }
    public Func<ConstructorInfo, bool> ShouldUseConstructor { get; }
    public IEnumerable<Action<PropertyMap, IMemberConfigurationExpression>> AllPropertyMapActions { get; }
    public IEnumerable<Action<TypeMap, IMappingExpression>> AllTypeMapActions { get; }
    public HashSet<string> GlobalIgnores { get; }
    public MemberConfiguration MemberConfiguration { get; }
    public IEnumerable<MethodInfo> SourceExtensionMethods { get; }
    public List<string> Prefixes { get; } = new();
    public List<string> Postfixes { get; } = new();
    public IReadOnlyCollection<ValueTransformerConfiguration> ValueTransformers { get; }
    public TypeDetails CreateTypeDetails(Type type)
    {
        if (_typeDetails == null)
        {
            return CreateRuntimeTypeDetails(type);
        }
        if (_typeDetails.TryGetValue(type, out var typeDetails))
        {
            return typeDetails;
        }
        typeDetails = new(type, this);
        _typeDetails.Add(type, typeDetails);
        return typeDetails;
        TypeDetails CreateRuntimeTypeDetails(Type type)
        {
            if (_runtimeTypeDetails == null)
            {
                Interlocked.CompareExchange(ref _runtimeTypeDetails, new(Environment.ProcessorCount, 2 * _openTypeMapConfigs.Count), null);
            }
            return _runtimeTypeDetails.GetOrAdd(type, (type, profile) => new(type, profile), this);
        }
    }
    public void Register(IGlobalConfiguration configuration)
    {
        foreach (var config in _typeMapConfigs)
        {
            if (config.DestinationTypeOverride == null)
            {
                BuildTypeMap(configuration, config);
                if (config.ReverseTypeMap != null)
                {
                    BuildTypeMap(configuration, config.ReverseTypeMap);
                }
            }
        }
    }
    private void BuildTypeMap(IGlobalConfiguration configuration, TypeMapConfiguration config)
    {
        var sourceMembers = configuration.SourceMembers;
        var typeMap = new TypeMap(config.SourceType, config.DestinationType, this, config, sourceMembers);
        config.Configure(typeMap, sourceMembers);
        configuration.RegisterTypeMap(typeMap);
    }
    public void Configure(IGlobalConfiguration configuration)
    {
        foreach (var typeMapConfiguration in _typeMapConfigs)
        {
            if (typeMapConfiguration.DestinationTypeOverride == null)
            {
                Configure(typeMapConfiguration, configuration);
                if (typeMapConfiguration.ReverseTypeMap != null)
                {
                    Configure(typeMapConfiguration.ReverseTypeMap, configuration);
                }
            }
            else
            {
                configuration.RegisterAsMap(typeMapConfiguration);
            }
        }
    }
    private void Configure(TypeMapConfiguration typeMapConfiguration, IGlobalConfiguration configuration)
    {
        var typeMap = typeMapConfiguration.TypeMap;
        if (typeMap.IncludeAllDerivedTypes)
        {
            IncludeAllDerived(configuration, typeMap);
        }
        Configure(typeMap, configuration);
    }
    private static void IncludeAllDerived(IGlobalConfiguration configuration, TypeMap typeMap)
    {
        foreach (var derivedMap in configuration.GetAllTypeMaps().Where(tm =>
                typeMap != tm &&
                typeMap.SourceType.IsAssignableFrom(tm.SourceType) &&
                typeMap.DestinationType.IsAssignableFrom(tm.DestinationType)))
        {
            typeMap.IncludeDerivedTypes(derivedMap.Types);
        }
    }
    private void Configure(TypeMap typeMap, IGlobalConfiguration configuration)
    {
        foreach (var action in AllTypeMapActions)
        {
            var expression = new MappingExpression(typeMap.Types, typeMap.ConfiguredMemberList);
            action(typeMap, expression);
            expression.Configure(typeMap, configuration.SourceMembers);
        }
        foreach (var action in AllPropertyMapActions)
        {
            foreach (var propertyMap in typeMap.PropertyMaps)
            {
                var memberExpression = new MappingExpression.MemberConfigurationExpression(propertyMap.DestinationMember, typeMap.SourceType);
                action(propertyMap, memberExpression);
                memberExpression.Configure(typeMap);
            }
        }
        ApplyBaseMaps(typeMap, typeMap, configuration);
        ApplyDerivedMaps(typeMap, typeMap, configuration);
        ApplyMemberMaps(typeMap, configuration);
    }
    public TypeMap CreateClosedGenericTypeMap(TypeMapConfiguration openMapConfig, TypePair closedTypes, IGlobalConfiguration configuration)
    {
        TypeMap closedMap;
        lock (configuration)
        {
            closedMap = new TypeMap(closedTypes.SourceType, closedTypes.DestinationType, this, openMapConfig);
        }
        openMapConfig.Configure(closedMap, configuration.SourceMembers);
        Configure(closedMap, configuration);
        closedMap.CloseGenerics(openMapConfig, closedTypes);
        return closedMap;
    }
    public TypeMapConfiguration GetGenericMap(TypePair genericPair) => _openTypeMapConfigs.GetValueOrDefault(genericPair);
    private void ApplyBaseMaps(TypeMap derivedMap, TypeMap currentMap, IGlobalConfiguration configuration)
    {
        foreach (var baseMap in configuration.GetIncludedTypeMaps(currentMap.IncludedBaseTypes))
        {
            baseMap.IncludeDerivedTypes(currentMap.Types);
            derivedMap.AddInheritedMap(baseMap);
            ApplyBaseMaps(derivedMap, baseMap, configuration);
        }
    }
    private void ApplyMemberMaps(TypeMap currentMap, IGlobalConfiguration configuration)
    {
        if (!currentMap.HasIncludedMembers)
        {
            return;
        }
        foreach (var includedMemberExpression in currentMap.GetAllIncludedMembers())
        {
            var includedMap = configuration.GetIncludedTypeMap(includedMemberExpression.Body.Type, currentMap.DestinationType);
            var includedMember = new IncludedMember(includedMap, includedMemberExpression);
            if (currentMap.AddMemberMap(includedMember))
            {
                ApplyMemberMaps(includedMap, configuration);
                foreach (var inheritedIncludedMember in includedMap.IncludedMembersTypeMaps)
                {
                    currentMap.AddMemberMap(includedMember.Chain(inheritedIncludedMember));
                }
            }
        }
    }
    private void ApplyDerivedMaps(TypeMap baseMap, TypeMap typeMap, IGlobalConfiguration configuration)
    {
        foreach (var derivedMap in configuration.GetIncludedTypeMaps(typeMap))
        {
            derivedMap.IncludeBaseTypes(typeMap.Types);
            derivedMap.AddInheritedMap(baseMap);
            ApplyDerivedMaps(baseMap, derivedMap, configuration);
        }
    }
    public bool MapDestinationPropertyToSource(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string destMemberName, List<MemberInfo> members, bool reverseNamingConventions) =>
        MemberConfiguration.IsMatch(this, sourceTypeDetails, destType, destMemberType, destMemberName, members, reverseNamingConventions);
    public bool AllowsNullDestinationValuesFor(MemberMap memberMap = null) => memberMap?.AllowNull ?? AllowNullDestinationValues;
    public bool AllowsNullCollectionsFor(MemberMap memberMap = null) => memberMap?.AllowNull ?? AllowNullCollections;
}
[EditorBrowsable(EditorBrowsableState.Never)]
[DebuggerDisplay("{MemberExpression}, {TypeMap}")]
public class IncludedMember : IEquatable<IncludedMember>
{
    public IncludedMember(TypeMap typeMap, LambdaExpression memberExpression) : this(typeMap, memberExpression,
        Variable(memberExpression.Body.Type, string.Join("", memberExpression.GetMembersChain().Select(m => m.Name))), memberExpression)
    {
    }
    private IncludedMember(TypeMap typeMap, LambdaExpression memberExpression, ParameterExpression variable, LambdaExpression projectToCustomSource)
    {
        TypeMap = typeMap;
        MemberExpression = memberExpression;
        Variable = variable;
        ProjectToCustomSource = projectToCustomSource;
    }
    public IncludedMember Chain(IncludedMember other)
    {
        if (other == null)
        {
            return this;
        }
        return new(other.TypeMap, Chain(other.MemberExpression), other.Variable, Chain(MemberExpression, other.MemberExpression));
    }
    public static LambdaExpression Chain(LambdaExpression customSource, LambdaExpression lambda) => 
        Lambda(lambda.ReplaceParameters(customSource.Body), customSource.Parameters);
    public TypeMap TypeMap { get; }
    public LambdaExpression MemberExpression { get; }
    public ParameterExpression Variable { get; }
    public LambdaExpression ProjectToCustomSource { get; }
    public LambdaExpression Chain(LambdaExpression lambda) => Lambda(lambda.ReplaceParameters(Variable), lambda.Parameters);
    public bool Equals(IncludedMember other) => TypeMap == other?.TypeMap;
    public override int GetHashCode() => TypeMap.GetHashCode();
}