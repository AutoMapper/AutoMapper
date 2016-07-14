using System.Linq;
using System.Runtime.CompilerServices;
using AutoMapper.Mappers;

namespace AutoMapper
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;
    using Configuration;
    using Configuration.Conventions;
    using IMemberConfiguration = Configuration.Conventions.IMemberConfiguration;

    /// <summary>
    /// Provides a named configuration for maps. Naming conventions become scoped per profile.
    /// </summary>
    public abstract class Profile : IProfileExpression, IProfileConfiguration
    {
        private readonly ConditionalObjectMapper _mapMissingTypes = new ConditionalObjectMapper {Conventions = {tp => true}};
        private readonly List<string> _globalIgnore = new List<string>();
        private readonly List<Action<TypeMap, IMappingExpression>> _allTypeMapActions = new List<Action<TypeMap, IMappingExpression>>();
        private readonly List<Action<PropertyMap, IMemberConfigurationExpression>> _allPropertyMapActions = new List<Action<PropertyMap, IMemberConfigurationExpression>>();
        private readonly List<ITypeMapConfiguration> _typeMapConfigs = new List<ITypeMapConfiguration>();
        private readonly List<ITypeMapConfiguration> _openTypeMapConfigs = new List<ITypeMapConfiguration>();
        private readonly TypeMapFactory _typeMapFactory = new TypeMapFactory();
        private readonly List<MethodInfo> _sourceExtensionMethods = new List<MethodInfo>();
        private readonly IList<IMemberConfiguration> _memberConfigurations = new List<IMemberConfiguration>();
        private readonly ConcurrentDictionary<Type, TypeDetails> _typeDetails = new ConcurrentDictionary<Type, TypeDetails>();

        protected Profile(string profileName)
            :this()
        {
            ProfileName = profileName;
        }

        protected Profile()
        {
            ProfileName = GetType().FullName;
            IncludeSourceExtensionMethods(typeof(Enumerable));
            _memberConfigurations.Add(new MemberConfiguration().AddMember<NameSplitMember>().AddName<PrePostfixName>(_ => _.AddStrings(p => p.Prefixes, "Get")));
        }

        [Obsolete("Create a constructor and configure inside of your profile's constructor instead. Will be removed in 6.0")]
        protected virtual void Configure() { }

#pragma warning disable 618 
        internal void Initialize() => Configure();
#pragma warning restore 618

        public virtual string ProfileName { get; }

        public void DisableConstructorMapping()
        {
            ConstructorMappingEnabled = false;
        }

        public bool AllowNullDestinationValues { get; set; } = true;

        public bool AllowNullCollections { get; set; }

        public IEnumerable<string> GlobalIgnores => _globalIgnore;

        public IEnumerable<string> Prefixes { get; private set; } = new string[0];

        public IEnumerable<string> Postfixes { get; private set; } = new string[0];

        public INamingConvention SourceMemberNamingConvention
        {
            get
        {
                INamingConvention convention = null;
                DefaultMemberConfig.AddMember<NameSplitMember>(_ => convention = _.SourceMemberNamingConvention);
                return convention;
        }
            set { DefaultMemberConfig.AddMember<NameSplitMember>(_ => _.SourceMemberNamingConvention = value); }
        }

        public INamingConvention DestinationMemberNamingConvention
        {
            get
        {
                INamingConvention convention = null;
                DefaultMemberConfig.AddMember<NameSplitMember>(_ => convention = _.DestinationMemberNamingConvention);
                return convention;
        }
            set { DefaultMemberConfig.AddMember<NameSplitMember>(_ => _.DestinationMemberNamingConvention = value); }
        }


        public bool CreateMissingTypeMaps
        {
            get
            {
                return _createMissingTypeMaps;
            }
            set
            {
                _createMissingTypeMaps = value;
                if (value)
                    _typeConfigurations.Add(_mapMissingTypes);
                else
                    _typeConfigurations.Remove(_mapMissingTypes);
            }
        }

        public IEnumerable<Action<TypeMap, IMappingExpression>> AllTypeMapActions => _allTypeMapActions;

        public IEnumerable<Action<PropertyMap, IMemberConfigurationExpression>> AllPropertyMapActions => _allPropertyMapActions;

        public void ForAllMaps(Action<TypeMap, IMappingExpression> configuration)
        {
            _allTypeMapActions.Add(configuration);
        }

        public void ForAllPropertyMaps(Func<PropertyMap, bool> condition, Action<PropertyMap, IMemberConfigurationExpression> configuration)
        {
            _allPropertyMapActions.Add((pm, cfg) =>
            {
                if (condition(pm)) configuration(pm, cfg);
            });
        }

        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            return CreateMap<TSource, TDestination>(MemberList.Destination);
        }

        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList)
        {
            return CreateMappingExpression<TSource, TDestination>(memberList);
        }

        public IMappingExpression CreateMap(Type sourceType, Type destinationType)
        {
            return CreateMap(sourceType, destinationType, MemberList.Destination);
        }

        public IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList)
        {
            var map = new MappingExpression(new TypePair(sourceType, destinationType), memberList);

            _typeMapConfigs.Add(map);

            if (sourceType.IsGenericTypeDefinition() || destinationType.IsGenericTypeDefinition())
                _openTypeMapConfigs.Add(map);

            return map;
        }

        private IMappingExpression<TSource, TDestination> CreateMappingExpression<TSource, TDestination>(MemberList memberList)
        {
            var mappingExp = new MappingExpression<TSource, TDestination>(memberList);

            _typeMapConfigs.Add(mappingExp);

            return mappingExp;
        }

        public void ClearPrefixes()
        {
            DefaultMemberConfig.AddName<PrePostfixName>(_ => _.Prefixes.Clear());
        }

        public void RecognizeAlias(string original, string alias)
        {
            DefaultMemberConfig.AddName<ReplaceName>(_ => _.AddReplace(original, alias));
        }

        public void ReplaceMemberName(string original, string newValue)
        {
            DefaultMemberConfig.AddName<ReplaceName>(_ => _.AddReplace(original, newValue));
        }

        public void RecognizePrefixes(params string[] prefixes)
        {
            DefaultMemberConfig.AddName<PrePostfixName>(_ => _.AddStrings(p => p.Prefixes, prefixes));
        }

        public void RecognizePostfixes(params string[] postfixes)
        {
            DefaultMemberConfig.AddName<PrePostfixName>(_ => _.AddStrings(p => p.Postfixes, postfixes));
        }

        public void RecognizeDestinationPrefixes(params string[] prefixes)
        {
            DefaultMemberConfig.AddName<PrePostfixName>(_ => _.AddStrings(p => p.DestinationPrefixes, prefixes));
        }

        public void RecognizeDestinationPostfixes(params string[] postfixes)
        {
            DefaultMemberConfig.AddName<PrePostfixName>(_ => _.AddStrings(p => p.DestinationPostfixes, postfixes));
        }

        public void AddGlobalIgnore(string propertyNameStartingWith)
        {
            _globalIgnore.Add(propertyNameStartingWith);
        }

        public IMemberConfiguration DefaultMemberConfig => _memberConfigurations.First();

        public IEnumerable<IMemberConfiguration> MemberConfigurations => _memberConfigurations;

        public IMemberConfiguration AddMemberConfiguration()
        {
            var condition = new MemberConfiguration();
            _memberConfigurations.Add(condition);
            return condition;
        }
        private readonly IList<ConditionalObjectMapper> _typeConfigurations = new List<ConditionalObjectMapper>();

        private bool _createMissingTypeMaps;

        public IEnumerable<IConditionalObjectMapper> TypeConfigurations => _typeConfigurations;

        public IConditionalObjectMapper AddConditionalObjectMapper()
        {
            var condition = new ConditionalObjectMapper();

            _typeConfigurations.Add(condition);

            return condition;
        }

        public bool ConstructorMappingEnabled { get; private set; } = true;

        public IEnumerable<MethodInfo> SourceExtensionMethods => _sourceExtensionMethods;

        public Func<PropertyInfo, bool> ShouldMapProperty { get; set; } = p => p.IsPublic();

        public Func<FieldInfo, bool> ShouldMapField { get; set; } = f => f.IsPublic();

        public void IncludeSourceExtensionMethods(Type type)
        {
            _sourceExtensionMethods.AddRange(type.GetDeclaredMethods().Where(m => m.IsStatic && m.IsDefined(typeof(ExtensionAttribute), false) && m.GetParameters().Length == 1));
        }

        void IProfileConfiguration.Register(TypeMapRegistry typeMapRegistry)
        {
            Prefixes =
                MemberConfigurations
                    .Select(m => m.NameMapper)
                    .SelectMany(m => m.NamedMappers)
                    .OfType<PrePostfixName>()
                    .SelectMany(m => m.Prefixes)
                    .ToArray();

            Postfixes =
                MemberConfigurations
                    .Select(m => m.NameMapper)
                    .SelectMany(m => m.NamedMappers)
                    .OfType<PrePostfixName>()
                    .SelectMany(m => m.Postfixes)
                    .ToArray();

            foreach (var config in _typeMapConfigs.Where(c => !c.IsOpenGeneric))
            {
                BuildTypeMap(typeMapRegistry, config);

                if (config.ReverseTypeMap != null)
                {
                    BuildTypeMap(typeMapRegistry, config.ReverseTypeMap);
                }
            }
        }

        void IProfileConfiguration.Configure(TypeMapRegistry typeMapRegistry)
        {
            foreach (var typeMap in _typeMapConfigs.Where(c => !c.IsOpenGeneric).Select(config => typeMapRegistry.GetTypeMap(config.Types)))
            {
                Configure(typeMapRegistry, typeMap);
            }
        }

        TypeMap IProfileConfiguration.ConfigureConventionTypeMap(TypeMapRegistry typeMapRegistry, TypePair types)
        {
            if (! TypeConfigurations.Any(c => c.IsMatch(types)))
                return null;

            var typeMap = _typeMapFactory.CreateTypeMap(types.SourceType, types.DestinationType, this, MemberList.Destination);

            var config = new MappingExpression(typeMap.Types, typeMap.ConfiguredMemberList);

            config.Configure(this, typeMap);

            Configure(typeMapRegistry, typeMap);

            return typeMap;
        }

        TypeDetails IProfileConfiguration.CreateTypeDetails(Type type)
        {
            return _typeDetails.GetOrAdd(type, t => new TypeDetails(t, this));
        }

        TypeMap IProfileConfiguration.ConfigureClosedGenericTypeMap(TypeMapRegistry typeMapRegistry, TypePair closedTypes, TypePair requestedTypes)
        {
            var openMapConfig = _openTypeMapConfigs
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

            openMapConfig.Configure(this, closedMap);

            Configure(typeMapRegistry, closedMap);

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

        private void Configure(TypeMapRegistry typeMapRegistry, TypeMap typeMap)
        {
            foreach(var action in _allTypeMapActions)
            {
                var expression = new MappingExpression(typeMap.Types, typeMap.ConfiguredMemberList);

                action(typeMap, expression);

                expression.Configure(this, typeMap);
            }

            foreach (var action in _allPropertyMapActions)
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

        private static void ApplyBaseMaps(TypeMapRegistry typeMapRegistry, TypeMap derivedMap, TypeMap currentMap)
        {
            foreach(var baseMap in currentMap.IncludedBaseTypes.Select(typeMapRegistry.GetTypeMap).Where(baseMap => baseMap != null))
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

        private void BuildTypeMap(TypeMapRegistry typeMapRegistry, ITypeMapConfiguration config)
        {
            var typeMap = _typeMapFactory.CreateTypeMap(config.SourceType, config.DestinationType, this, config.MemberList);

            config.Configure(this, typeMap);

            typeMapRegistry.RegisterTypeMap(typeMap);
        }
    }
}