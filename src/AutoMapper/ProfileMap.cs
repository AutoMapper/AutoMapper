using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AutoMapper.Configuration;
using AutoMapper.Configuration.Conventions;
using AutoMapper.Mappers;
using AutoMapper.QueryableExtensions.Impl;

namespace AutoMapper
{
    [DebuggerDisplay("{Name}")]
    public class ProfileMap
    {
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
            CreateMissingTypeMaps = profile.CreateMissingTypeMaps ?? configuration?.CreateMissingTypeMaps ?? true;
            ValidateInlineMaps = profile.ValidateInlineMaps ?? configuration?.ValidateInlineMaps ?? true;

            TypeConfigurations = profile.TypeConfigurations
                .Concat(configuration?.TypeConfigurations ?? Enumerable.Empty<IConditionalObjectMapper>())
                .ToArray();

            ValueTransformers = profile.ValueTransformers.Concat(configuration?.ValueTransformers ?? Enumerable.Empty<ValueTransformerConfiguration>()).ToArray();

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
        public bool ValidateInlineMaps { get; }
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
        public IEnumerable<ValueTransformerConfiguration> ValueTransformers { get; }

        public TypeDetails CreateTypeDetails(Type type) => _typeDetails.GetOrAdd(type);

        private TypeDetails TypeDetailsFactory(Type type) => new TypeDetails(type, this);

        public void Register(IConfigurationProvider configurationProvider)
        {
            foreach (var config in _typeMapConfigs.Where(c => !c.IsOpenGeneric))
            {
                BuildTypeMap(configurationProvider, config);

                if (config.ReverseTypeMap != null)
                {
                    BuildTypeMap(configurationProvider, config.ReverseTypeMap);
                }
            }
        }

        public void Configure(IConfigurationProvider configurationProvider)
        {
            foreach (var typeMapConfiguration in _typeMapConfigs.Where(c => !c.IsOpenGeneric))
            {
                Configure(typeMapConfiguration, configurationProvider);
                if (typeMapConfiguration.ReverseTypeMap != null)
                {
                    Configure(typeMapConfiguration.ReverseTypeMap, configurationProvider);
                }
            }
        }

        private void BuildTypeMap(IConfigurationProvider configurationProvider, ITypeMapConfiguration config)
        {
            var typeMap = _typeMapFactory.CreateTypeMap(config.SourceType, config.DestinationType, this);

            config.Configure(typeMap);

            configurationProvider.RegisterTypeMap(typeMap);
        }

        private void Configure(ITypeMapConfiguration typeMapConfiguration, IConfigurationProvider configurationProvider)
        {
            var typeMap = configurationProvider.FindTypeMapFor(typeMapConfiguration.Types);
            Configure(typeMap, configurationProvider);
        }

        private void Configure(TypeMap typeMap, IConfigurationProvider configurationProvider)
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

            ApplyBaseMaps(typeMap, typeMap, configurationProvider);
            ApplyDerivedMaps(typeMap, typeMap, configurationProvider);
        }

        public bool IsConventionMap(TypePair types) => TypeConfigurations.Any(c => c.IsMatch(types));

        public TypeMap CreateConventionTypeMap(TypePair types, IConfigurationProvider configurationProvider)
        {
            var typeMap = _typeMapFactory.CreateTypeMap(types.SourceType, types.DestinationType, this);

            typeMap.IsConventionMap = true;

            var config = new MappingExpression(typeMap.Types, typeMap.ConfiguredMemberList);

            config.Configure(typeMap);

            Configure(typeMap, configurationProvider);

            return typeMap;
        }

        public TypeMap CreateInlineMap(TypePair types, IConfigurationProvider configurationProvider)
        {
            var typeMap = _typeMapFactory.CreateTypeMap(types.SourceType, types.DestinationType, this);

            typeMap.IsConventionMap = true;

            Configure(typeMap, configurationProvider);

            return typeMap;
        }

        public TypeMap CreateClosedGenericTypeMap(ITypeMapConfiguration openMapConfig, TypePair closedTypes, IConfigurationProvider configurationProvider)
        {
            var closedMap = _typeMapFactory.CreateTypeMap(closedTypes.SourceType, closedTypes.DestinationType, this);

            openMapConfig.Configure(closedMap);

            Configure(closedMap, configurationProvider);

            if(closedMap.TypeConverterType != null)
            {
                var typeParams =
                    (openMapConfig.SourceType.IsGenericTypeDefinition() ? closedTypes.SourceType.GetGenericArguments() : new Type[0])
                        .Concat
                    (openMapConfig.DestinationType.IsGenericTypeDefinition() ? closedTypes.DestinationType.GetGenericArguments() : new Type[0]);

                var neededParameters = closedMap.TypeConverterType.GetGenericParameters().Length;
                closedMap.TypeConverterType = closedMap.TypeConverterType.MakeGenericType(typeParams.Take(neededParameters).ToArray());
            }
            if(closedMap.DestinationTypeOverride?.IsGenericTypeDefinition() == true)
            {
                var neededParameters = closedMap.DestinationTypeOverride.GetGenericParameters().Length;
                closedMap.DestinationTypeOverride = closedMap.DestinationTypeOverride.MakeGenericType(closedTypes.DestinationType.GetGenericArguments().Take(neededParameters).ToArray());
            }
            return closedMap;
        }

        public ITypeMapConfiguration GetGenericMap(TypePair closedTypes)
        {
            return _openTypeMapConfigs
                .SelectMany(tm => tm.ReverseTypeMap == null ? new[] { tm } : new[] { tm, tm.ReverseTypeMap })
                .Where(tm =>
                    tm.Types.SourceType.GetGenericTypeDefinitionIfGeneric() == closedTypes.SourceType.GetGenericTypeDefinitionIfGeneric() &&
                    tm.Types.DestinationType.GetGenericTypeDefinitionIfGeneric() == closedTypes.DestinationType.GetGenericTypeDefinitionIfGeneric())
                .OrderByDescending(tm => tm.DestinationType == closedTypes.DestinationType) // Favor more specific destination matches,
                .ThenByDescending(tm => tm.SourceType == closedTypes.SourceType) // then more specific source matches
                .FirstOrDefault();
        }

        private void ApplyBaseMaps(TypeMap derivedMap, TypeMap currentMap, IConfigurationProvider configurationProvider)
        {
            foreach (var baseMap in GetIncludedTypeMaps(currentMap.IncludedBaseTypes, configurationProvider))
            {
                baseMap.IncludeDerivedTypes(currentMap.SourceType, currentMap.DestinationType);
                derivedMap.AddInheritedMap(baseMap);
                ApplyBaseMaps(derivedMap, baseMap, configurationProvider);
            }
        }

        private void ApplyDerivedMaps(TypeMap baseMap, TypeMap typeMap, IConfigurationProvider configurationProvider)
        {
            foreach (var inheritedTypeMap in GetIncludedTypeMaps(typeMap.IncludedDerivedTypes, configurationProvider))
            {
                inheritedTypeMap.IncludeBaseTypes(typeMap.SourceType, typeMap.DestinationType);
                inheritedTypeMap.AddInheritedMap(baseMap);
                ApplyDerivedMaps(baseMap, inheritedTypeMap, configurationProvider);
            }
        }

        private static IEnumerable<TypeMap> GetIncludedTypeMaps(IEnumerable<TypePair> includedTypes, IConfigurationProvider configurationProvider)
        {
            foreach(var pair in includedTypes)
            {
                var typeMap = configurationProvider.FindTypeMapFor(pair);
                if(typeMap != null)
                {
                    yield return typeMap;
                }
                typeMap = configurationProvider.ResolveTypeMap(pair);
                // we want the exact map the user included, but we could instantiate an open generic
                if(typeMap == null || typeMap.Types != pair || typeMap.IsConventionMap)
                {
                    throw QueryMapperHelper.MissingMapException(pair);
                }
                yield return typeMap;
            }
        }
    }
}