namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Configuration;
    using Configuration.Conventions;
    using Mappers;

    /// <summary>
    ///     Provides a named configuration for maps. Naming conventions become scoped per profile.
    /// </summary>
    public abstract class Profile : IProfileExpression, IProfileConfiguration
    {
        private readonly List<Action<PropertyMap, IMemberConfigurationExpression>> _allPropertyMapActions =
            new List<Action<PropertyMap, IMemberConfigurationExpression>>();

        private readonly List<Action<TypeMap, IMappingExpression>> _allTypeMapActions =
            new List<Action<TypeMap, IMappingExpression>>();

        private readonly List<string> _globalIgnore = new List<string>();
        private readonly IList<IMemberConfiguration> _memberConfigurations = new List<IMemberConfiguration>();
        private readonly List<ITypeMapConfiguration> _openTypeMapConfigs = new List<ITypeMapConfiguration>();
        private readonly List<MethodInfo> _sourceExtensionMethods = new List<MethodInfo>();
        private readonly IList<ConditionalObjectMapper> _typeConfigurations = new List<ConditionalObjectMapper>();

        private readonly List<ITypeMapConfiguration> _typeMapConfigs = new List<ITypeMapConfiguration>();

        protected Profile(string profileName)
            : this()
        {
            ProfileName = profileName;
        }

        protected Profile()
        {
            ProfileName = GetType().FullName;

            AddMemberConfiguration()
                .AddMember<NameSplitMember>()
                .AddName<PrePostfixName>(_ => _.AddStrings(p => p.Prefixes, "Get"));

            SourceMemberNamingConvention = new PascalCaseNamingConvention();
            DestinationMemberNamingConvention = new PascalCaseNamingConvention();
        }

        protected Profile(string profileName, Action<IProfileExpression> configurationAction)
            : this(profileName)
        {
            configurationAction(this);
        }

        public IMemberConfiguration DefaultMemberConfig => _memberConfigurations.First();
        public bool? ConstructorMappingEnabled { get; private set; }
        public bool? CreateMissingTypeMaps { get; set; }

        IEnumerable<Action<PropertyMap, IMemberConfigurationExpression>> IProfileConfiguration.AllPropertyMapActions
            => _allPropertyMapActions;

        IEnumerable<Action<TypeMap, IMappingExpression>> IProfileConfiguration.AllTypeMapActions => _allTypeMapActions;
        IEnumerable<string> IProfileConfiguration.GlobalIgnores => _globalIgnore;
        IEnumerable<IMemberConfiguration> IProfileConfiguration.MemberConfigurations => _memberConfigurations;
        IEnumerable<MethodInfo> IProfileConfiguration.SourceExtensionMethods => _sourceExtensionMethods;
        IEnumerable<IConditionalObjectMapper> IProfileConfiguration.TypeConfigurations => _typeConfigurations;
        IEnumerable<ITypeMapConfiguration> IProfileConfiguration.TypeMapConfigs => _typeMapConfigs;
        IEnumerable<ITypeMapConfiguration> IProfileConfiguration.OpenTypeMapConfigs => _openTypeMapConfigs;

        public virtual string ProfileName { get; }

        public bool? AllowNullDestinationValues { get; set; }
        public bool? AllowNullCollections { get; set; }
        public bool? EnableNullPropagationForQueryMapping { get; set; }
        public Func<PropertyInfo, bool> ShouldMapProperty { get; set; }
        public Func<FieldInfo, bool> ShouldMapField { get; set; }

        public INamingConvention SourceMemberNamingConvention { get; set; }
        public INamingConvention DestinationMemberNamingConvention { get; set; }


        public void DisableConstructorMapping()
        {
            ConstructorMappingEnabled = false;
        }

        public void ForAllMaps(Action<TypeMap, IMappingExpression> configuration)
        {
            _allTypeMapActions.Add(configuration);
        }

        public void ForAllPropertyMaps(Func<PropertyMap, bool> condition,
            Action<PropertyMap, IMemberConfigurationExpression> configuration)
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

        public IMemberConfiguration AddMemberConfiguration()
        {
            var condition = new MemberConfiguration();
            _memberConfigurations.Add(condition);
            return condition;
        }

        public IConditionalObjectMapper AddConditionalObjectMapper()
        {
            var condition = new ConditionalObjectMapper();

            _typeConfigurations.Add(condition);

            return condition;
        }

        public void IncludeSourceExtensionMethods(Type type)
        {
            _sourceExtensionMethods.AddRange(
                type.GetDeclaredMethods()
                    .Where(
                        m =>
                            m.IsStatic && m.IsDefined(typeof(ExtensionAttribute), false) &&
                            (m.GetParameters().Length == 1)));
        }

        [Obsolete(
             "Create a constructor and configure inside of your profile's constructor instead. Will be removed in 6.0")]
        protected virtual void Configure()
        {
        }

#pragma warning disable 618 
        internal void Initialize() => Configure();
#pragma warning restore 618

        private IMappingExpression<TSource, TDestination> CreateMappingExpression<TSource, TDestination>(
            MemberList memberList)
        {
            var mappingExp = new MappingExpression<TSource, TDestination>(memberList);

            _typeMapConfigs.Add(mappingExp);

            return mappingExp;
        }
    }
}