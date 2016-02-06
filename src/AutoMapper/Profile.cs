using System.Linq;
using System.Runtime.CompilerServices;
using AutoMapper.Mappers;

namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Internal;
    using Configuration.Conventions;

    /// <summary>
    /// Provides a named configuration for maps. Naming conventions become scoped per profile.
    /// </summary>
    public abstract class Profile : IProfileExpression, IProfileConfiguration
    {
        private readonly ConditionalObjectMapper _mapMissingTypes = new ConditionalObjectMapper {Conventions = {tp => true}};
        private readonly List<string> _globalIgnore = new List<string>();
        private readonly List<Action<TypeMap, IMappingExpression>> _allTypeMapActions = new List<Action<TypeMap, IMappingExpression>>();
        private readonly List<ITypeMapConfiguration> _typeMapConfigs = new List<ITypeMapConfiguration>();
        private readonly TypeMapFactory _typeMapFactory = new TypeMapFactory();

        protected Profile(string profileName)
            :this()
        {
            ProfileName = profileName;
        }

        protected Profile()
        {
            ProfileName = GetType().FullName;
            IncludeSourceExtensionMethods(typeof(Enumerable).Assembly());
            _memberConfigurations.Add(new MemberConfiguration().AddMember<NameSplitMember>().AddName<PrePostfixName>(_ => _.AddStrings(p => p.Prefixes, "Get")));
        }

        [Obsolete("Use the construtor instead. Will be removed in 6.0")]
        protected virtual void Configure() { }

        internal void Initialize() => Configure();

        public virtual string ProfileName { get; }

        public void DisableConstructorMapping()
        {
            ConstructorMappingEnabled = false;
        }

        public bool AllowNullDestinationValues { get; set; } = true;

        public bool AllowNullCollections { get; set; }

        public IEnumerable<string> GlobalIgnores => _globalIgnore; 

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

        public void ForAllMaps(Action<TypeMap, IMappingExpression> configuration)
        {
            _allTypeMapActions.Add(configuration);
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

        private readonly List<MethodInfo> _sourceExtensionMethods = new List<MethodInfo>();

        private readonly IList<IMemberConfiguration> _memberConfigurations = new List<IMemberConfiguration>();

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

        public void IncludeSourceExtensionMethods(Assembly assembly)
        {
            //http://stackoverflow.com/questions/299515/c-sharp-reflection-to-identify-extension-methods
            _sourceExtensionMethods.AddRange(assembly.ExportedTypes
                .Where(type => type.IsSealed() && !type.IsGenericType() && !type.IsNested)
                .SelectMany(type => type.GetDeclaredMethods().Where(mi => mi.IsStatic))
                .Where(method => method.IsDefined(typeof(ExtensionAttribute), false))
                .Where(method => method.GetParameters().Length == 1));
        }

        void IProfileConfiguration.Register(TypeMapRegistry typeMapRegistry)
        {
            foreach (var config in _typeMapConfigs)
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
            foreach (var typeMap in _typeMapConfigs.Select(config => typeMapRegistry.GetTypeMap(config.Types)))
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

        TypeMap IProfileConfiguration.ConfigureClosedGenericTypeMap(TypeMapRegistry typeMapRegistry, TypePair closedTypes, TypePair openTypes)
        {
            var openMapConfig = _typeMapConfigs.FirstOrDefault(tm => tm.Types == openTypes);

            if (openMapConfig == null)
                return null;

            var closedMap = _typeMapFactory.CreateTypeMap(closedTypes.SourceType, closedTypes.DestinationType, this, openMapConfig.MemberList);

            openMapConfig.Configure(this, closedMap);

            Configure(typeMapRegistry, closedMap);

            return closedMap;
        }

        private void Configure(TypeMapRegistry typeMapRegistry, TypeMap typeMap)
        {
            foreach (var action in _allTypeMapActions)
            {
                var expression = new MappingExpression(typeMap.Types, typeMap.ConfiguredMemberList);

                action(typeMap, expression);

                expression.Configure(this, typeMap);
            }

            foreach (var baseMap in typeMap.IncludedBaseTypes.Select(typeMapRegistry.GetTypeMap).Where(baseMap => baseMap != null))
            {
                var currentMap = baseMap;
                while (currentMap != null)
                {
                    currentMap.IncludeDerivedTypes(typeMap.SourceType, typeMap.DestinationType);
                    typeMap.ApplyInheritedMap(currentMap);

                    currentMap = typeMapRegistry.GetTypeMap(new TypePair(currentMap.SourceType.BaseType(), currentMap.DestinationType.BaseType()));
                }

            }
            ApplyInheritedMaps(typeMapRegistry, typeMap, typeMap);
        }

        private void ApplyInheritedMaps(TypeMapRegistry typeMapRegistry, TypeMap baseMap, TypeMap typeMap)
        {
            foreach (var inheritedTypeMap in typeMap.IncludedDerivedTypes.Select(typeMapRegistry.GetTypeMap).Where(map => map != null))
            {
                inheritedTypeMap.ApplyInheritedMap(baseMap);
                ApplyInheritedMaps(typeMapRegistry, baseMap, inheritedTypeMap);
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