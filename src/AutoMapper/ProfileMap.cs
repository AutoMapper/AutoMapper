namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Configuration;
    using Configuration.Conventions;
    using Mappers;

#if NETSTANDARD1_1
    struct IConvertible{}
#endif

    [DebuggerDisplay("{Name}")]
    public class ProfileMap
    {
        private static readonly Type[] ExcludedTypes = new[]{ typeof(object), typeof(ValueType), typeof(Enum), typeof(IComparable), typeof(IFormattable), typeof(IConvertible) };
        private readonly TypeMapFactory _typeMapFactory = new TypeMapFactory();
        private readonly IEnumerable<ITypeMapConfiguration> _typeMapConfigs;
        private readonly IEnumerable<ITypeMapConfiguration> _openTypeMapConfigs;
        private readonly LockingConcurrentDictionary<Type, TypeDetails> _typeDetails;

        public ProfileMap(IProfileConfiguration profile)
            : this(profile, null)
        {
        }

        public ProfileMap(IProfileConfiguration profile, IConfiguration configuration)
        {
            _typeDetails = new LockingConcurrentDictionary<Type, TypeDetails>(TypeDetailsFactory);

            Name = profile.ProfileName;
            AllowNullCollections = profile.AllowNullCollections ?? configuration?.AllowNullCollections ?? false;
            AllowNullDestinationValues = profile.AllowNullDestinationValues ?? configuration?.AllowNullDestinationValues ?? true;
            EnableNullPropagationForQueryMapping = profile.EnableNullPropagationForQueryMapping ?? configuration?.EnableNullPropagationForQueryMapping ?? false;
            ConstructorMappingEnabled = profile.ConstructorMappingEnabled ?? configuration?.ConstructorMappingEnabled ?? true;
            ShouldMapField = profile.ShouldMapField ?? configuration?.ShouldMapField ?? (p => p.IsPublic());
            ShouldMapProperty = profile.ShouldMapProperty ?? configuration?.ShouldMapProperty ?? (p => p.IsPublic());
            CreateMissingTypeMaps = profile.CreateMissingTypeMaps ?? configuration?.CreateMissingTypeMaps ?? false;

            TypeConfigurations = profile.TypeConfigurations
                .Concat(configuration?.TypeConfigurations ?? Enumerable.Empty<IConditionalObjectMapper>())
                .Concat(CreateMissingTypeMaps
                    ? Enumerable.Repeat(new ConditionalObjectMapper { Conventions = { tp => !ExcludedTypes.Contains(tp.SourceType) && !ExcludedTypes.Contains(tp.DestinationType)} }, 1)
                    : Enumerable.Empty<IConditionalObjectMapper>())
                .ToArray();


            MemberConfigurations = profile.MemberConfigurations.ToArray();

            MemberConfigurations.FirstOrDefault()?.AddMember<NameSplitMember>(_ => _.SourceMemberNamingConvention = profile.SourceMemberNamingConvention);
            MemberConfigurations.FirstOrDefault()?.AddMember<NameSplitMember>(_ => _.DestinationMemberNamingConvention = profile.DestinationMemberNamingConvention);

            GlobalIgnores = profile.GlobalIgnores.Concat(configuration?.GlobalIgnores ?? Enumerable.Empty<string>()).ToArray();
            SourceExtensionMethods = profile.SourceExtensionMethods.Concat(configuration?.SourceExtensionMethods ?? Enumerable.Empty<MethodInfo>()).ToArray();
            AllPropertyMapActions = profile.AllPropertyMapActions.Concat(configuration?.AllPropertyMapActions ?? Enumerable.Empty<Action<PropertyMap, IMemberConfigurationExpression>>()).ToArray();
            AllTypeMapActions = profile.AllTypeMapActions.Concat(configuration?.AllTypeMapActions ?? Enumerable.Empty<Action<TypeMap, IMappingExpression>>()).ToArray();

            Prefixes =
                profile.MemberConfigurations
                    .Select(m => m.NameMapper)
                    .SelectMany(m => m.NamedMappers)
                    .OfType<PrePostfixName>()
                    .SelectMany(m => m.Prefixes)
                    .ToArray();

            Postfixes =
                profile.MemberConfigurations
                    .Select(m => m.NameMapper)
                    .SelectMany(m => m.NamedMappers)
                    .OfType<PrePostfixName>()
                    .SelectMany(m => m.Postfixes)
                    .ToArray();

            _typeMapConfigs = profile.TypeMapConfigs.ToArray();
            _openTypeMapConfigs = profile.OpenTypeMapConfigs.ToArray();
        }

        public bool AllowNullCollections { get; }
        public bool AllowNullDestinationValues { get; }
        public bool ConstructorMappingEnabled { get; }
        public bool CreateMissingTypeMaps { get; }
        public bool EnableNullPropagationForQueryMapping { get; }
        public string Name { get; }
        public Func<FieldInfo, bool> ShouldMapField { get; }
        public Func<PropertyInfo, bool> ShouldMapProperty { get; }

        public IEnumerable<Action<PropertyMap, IMemberConfigurationExpression>> AllPropertyMapActions { get; }
        public IEnumerable<Action<TypeMap, IMappingExpression>> AllTypeMapActions { get; }
        public IEnumerable<string> GlobalIgnores { get; }
        public IEnumerable<IMemberConfiguration> MemberConfigurations { get; }
        public IEnumerable<MethodInfo> SourceExtensionMethods { get; }
        public IEnumerable<IConditionalObjectMapper> TypeConfigurations { get; }
        public IEnumerable<string> Prefixes { get; }
        public IEnumerable<string> Postfixes { get; }

        public TypeDetails CreateTypeDetails(Type type)
        {
            return _typeDetails.GetOrAdd(type);
        }

        private TypeDetails TypeDetailsFactory(Type type)
        {
            return new TypeDetails(type, this);
        }

        public void Register(TypeMapRegistry typeMapRegistry)
        {
            foreach (var config in _typeMapConfigs.Where(c => !c.IsOpenGeneric))
            {
                BuildTypeMap(typeMapRegistry, config);

                if (config.ReverseTypeMap != null)
                {
                    BuildTypeMap(typeMapRegistry, config.ReverseTypeMap);
                }
            }
        }

        public void Configure(TypeMapRegistry typeMapRegistry)
        {
            foreach (var typeMapConfiguration in _typeMapConfigs.Where(c => !c.IsOpenGeneric))
            {
                Configure(typeMapRegistry, typeMapConfiguration);
                if (typeMapConfiguration.ReverseTypeMap != null)
                {
                    Configure(typeMapRegistry, typeMapConfiguration.ReverseTypeMap);
                }
            }
        }

        private void BuildTypeMap(TypeMapRegistry typeMapRegistry, ITypeMapConfiguration config)
        {
            var typeMap = _typeMapFactory.CreateTypeMap(config.SourceType, config.DestinationType, this, config.MemberList);

            config.Configure(typeMap);

            typeMapRegistry.RegisterTypeMap(typeMap);
        }

        private void Configure(TypeMapRegistry typeMapRegistry, ITypeMapConfiguration typeMapConfiguration)
        {
            var typeMap = typeMapRegistry.GetTypeMap(typeMapConfiguration.Types);
            Configure(typeMapRegistry, typeMap);
        }

        private void Configure(TypeMapRegistry typeMapRegistry, TypeMap typeMap)
        {
            foreach (var action in AllTypeMapActions)
            {
                var expression = new MappingExpression(typeMap.Types, typeMap.ConfiguredMemberList);

                action(typeMap, expression);

                expression.Configure(typeMap);
            }

            foreach (var action in AllPropertyMapActions)
            {
                foreach (var propertyMap in typeMap.GetPropertyMaps())
                {
                    var memberExpression = new MappingExpression.MemberConfigurationExpression(propertyMap.DestinationProperty, typeMap.SourceType);

                    action(propertyMap, memberExpression);

                    memberExpression.Configure(typeMap);
                }
            }

            ApplyBaseMaps(typeMapRegistry, typeMap, typeMap);
            ApplyDerivedMaps(typeMapRegistry, typeMap, typeMap);
        }

        public TypeMap ConfigureConventionTypeMap(TypeMapRegistry typeMapRegistry, TypePair types)
        {
            if (!TypeConfigurations.Any(c => c.IsMatch(types)))
                return null;

            var typeMap = _typeMapFactory.CreateTypeMap(types.SourceType, types.DestinationType, this, MemberList.Destination);

            var config = new MappingExpression(typeMap.Types, typeMap.ConfiguredMemberList);

            config.Configure(typeMap);

            Configure(typeMapRegistry, typeMap);

            return typeMap;
        }

        public TypeMap ConfigureClosedGenericTypeMap(TypeMapRegistry typeMapRegistry, TypePair closedTypes, TypePair requestedTypes)
        {
            var openMapConfig = _openTypeMapConfigs
                .SelectMany(tm => tm.ReverseTypeMap == null ? new[] { tm } : new[] { tm, tm.ReverseTypeMap })
                //.Where(tm => tm.IsOpenGeneric)
                .Where(tm =>
                    tm.Types.SourceType.GetGenericTypeDefinitionIfGeneric() == closedTypes.SourceType.GetGenericTypeDefinitionIfGeneric() &&
                    tm.Types.DestinationType.GetGenericTypeDefinitionIfGeneric() == closedTypes.DestinationType.GetGenericTypeDefinitionIfGeneric())
                .OrderByDescending(tm => tm.DestinationType == closedTypes.DestinationType) // Favor more specific destination matches,
                .ThenByDescending(tm => tm.SourceType == closedTypes.SourceType) // then more specific source matches
                .FirstOrDefault();

            if (openMapConfig == null)
                return null;

            var closedMap = _typeMapFactory.CreateTypeMap(requestedTypes.SourceType, requestedTypes.DestinationType, this, openMapConfig.MemberList);

            openMapConfig.Configure(closedMap);

            Configure(typeMapRegistry, closedMap);

            if (closedMap.TypeConverterType != null)
            {
                var typeParams =
                    (openMapConfig.SourceType.IsGenericTypeDefinition() ? closedTypes.SourceType.GetGenericArguments() : new Type[0])
                        .Concat
                    (openMapConfig.DestinationType.IsGenericTypeDefinition() ? closedTypes.DestinationType.GetGenericArguments() : new Type[0]);

                var neededParameters = closedMap.TypeConverterType.GetGenericParameters().Length;
                closedMap.TypeConverterType = closedMap.TypeConverterType.MakeGenericType(typeParams.Take(neededParameters).ToArray());
            }
            if (closedMap.DestinationTypeOverride?.IsGenericTypeDefinition() == true)
            {
                var neededParameters = closedMap.DestinationTypeOverride.GetGenericParameters().Length;
                closedMap.DestinationTypeOverride = closedMap.DestinationTypeOverride.MakeGenericType(closedTypes.DestinationType.GetGenericArguments().Take(neededParameters).ToArray());
            }
            return closedMap;
        }


        private static void ApplyBaseMaps(TypeMapRegistry typeMapRegistry, TypeMap derivedMap, TypeMap currentMap)
        {
            foreach (var baseMap in currentMap.IncludedBaseTypes.Select(typeMapRegistry.GetTypeMap).Where(baseMap => baseMap != null))
            {
                baseMap.IncludeDerivedTypes(currentMap.SourceType, currentMap.DestinationType);
                derivedMap.ApplyInheritedMap(baseMap);
                ApplyBaseMaps(typeMapRegistry, derivedMap, baseMap);
            }
        }

        private void ApplyDerivedMaps(TypeMapRegistry typeMapRegistry, TypeMap baseMap, TypeMap typeMap)
        {
            foreach (var inheritedTypeMap in typeMap.IncludedDerivedTypes.Select(typeMapRegistry.GetTypeMap).Where(map => map != null))
            {
                inheritedTypeMap.ApplyInheritedMap(baseMap);
                ApplyDerivedMaps(typeMapRegistry, baseMap, inheritedTypeMap);
            }
        }
    }
}