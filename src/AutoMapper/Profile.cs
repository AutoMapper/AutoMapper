using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper.Configuration;
using AutoMapper.Configuration.Conventions;
using AutoMapper.Internal;

namespace AutoMapper
{
    /// <summary>
    ///     Provides a named configuration for maps. Naming conventions become scoped per profile.
    /// </summary>
    public abstract class Profile : IProfileExpressionInternal, IProfileConfiguration
    {
        private readonly List<Action<PropertyMap, IMemberConfigurationExpression>> _allPropertyMapActions =
            new List<Action<PropertyMap, IMemberConfigurationExpression>>();
        private readonly List<Action<TypeMap, IMappingExpression>> _allTypeMapActions =
            new List<Action<TypeMap, IMappingExpression>>();
        private readonly List<string> _globalIgnore = new List<string>();
        private readonly List<IMemberConfiguration> _memberConfigurations = new List<IMemberConfiguration>();
        private readonly List<ITypeMapConfiguration> _openTypeMapConfigs = new List<ITypeMapConfiguration>();
        private readonly List<MethodInfo> _sourceExtensionMethods = new List<MethodInfo>();
        private readonly List<ITypeMapConfiguration> _typeMapConfigs = new List<ITypeMapConfiguration>();
        private readonly List<ValueTransformerConfiguration> _valueTransformerConfigs = new List<ValueTransformerConfiguration>();
        private readonly PrePostfixName _prePostfixName = new PrePostfixName();
        private bool? _constructorMappingEnabled;

        protected Profile(string profileName) : this() => ProfileName = profileName;

        protected Profile()
        {
            ProfileName = GetType().FullName;
            var memberConfiguration = new MemberConfiguration();
            memberConfiguration.MemberMappers.Add(new NameSplitMember());
            _prePostfixName.Prefixes.Add("Get");
            memberConfiguration.NameMapper.NamedMappers.Add(_prePostfixName);
            _memberConfigurations.Add(memberConfiguration);
        }
        protected Profile(string profileName, Action<IProfileExpression> configurationAction)
            : this(profileName)  => configurationAction(this);

        IMemberConfiguration DefaultMemberConfig => _memberConfigurations[0];
        IMemberConfiguration IProfileExpressionInternal.DefaultMemberConfig => DefaultMemberConfig;
        bool? IProfileConfiguration.ConstructorMappingEnabled => _constructorMappingEnabled;
        bool? IProfileExpressionInternal.MethodMappingEnabled { get; set; }
        bool? IProfileConfiguration.MethodMappingEnabled => this.Internal().MethodMappingEnabled;
        bool? IProfileExpressionInternal.FieldMappingEnabled { get; set; }
        bool? IProfileConfiguration.FieldMappingEnabled => this.Internal().FieldMappingEnabled;
        bool? IProfileConfiguration.EnableNullPropagationForQueryMapping => this.Internal().EnableNullPropagationForQueryMapping;
        IEnumerable<Action<PropertyMap, IMemberConfigurationExpression>> IProfileConfiguration.AllPropertyMapActions
            => _allPropertyMapActions;
        IEnumerable<Action<TypeMap, IMappingExpression>> IProfileConfiguration.AllTypeMapActions => _allTypeMapActions;
        IEnumerable<string> IProfileConfiguration.GlobalIgnores => _globalIgnore;
        IEnumerable<IMemberConfiguration> IProfileConfiguration.MemberConfigurations => _memberConfigurations;
        IEnumerable<MethodInfo> IProfileConfiguration.SourceExtensionMethods => _sourceExtensionMethods;
        IReadOnlyCollection<ITypeMapConfiguration> IProfileConfiguration.TypeMapConfigs => _typeMapConfigs;
        IReadOnlyCollection<ITypeMapConfiguration> IProfileConfiguration.OpenTypeMapConfigs => _openTypeMapConfigs;
        IEnumerable<ValueTransformerConfiguration> IProfileConfiguration.ValueTransformers => _valueTransformerConfigs;

        public virtual string ProfileName { get; }
        public bool? AllowNullDestinationValues { get; set; }
        public bool? AllowNullCollections { get; set; }
        bool? IProfileExpressionInternal.EnableNullPropagationForQueryMapping { get; set; }
        public Func<PropertyInfo, bool> ShouldMapProperty { get; set; }
        public Func<FieldInfo, bool> ShouldMapField { get; set; }
        public Func<MethodInfo, bool> ShouldMapMethod { get; set; }
        public Func<ConstructorInfo, bool> ShouldUseConstructor { get; set; }
        public INamingConvention SourceMemberNamingConvention { get; set; }
        public INamingConvention DestinationMemberNamingConvention { get; set; }
        public IList<ValueTransformerConfiguration> ValueTransformers => _valueTransformerConfigs;

        public void DisableConstructorMapping() => _constructorMappingEnabled = false;

        void IProfileExpressionInternal.ForAllMaps(Action<TypeMap, IMappingExpression> configuration) => _allTypeMapActions.Add(configuration);

        void IProfileExpressionInternal.ForAllPropertyMaps(Func<PropertyMap, bool> condition, Action<PropertyMap, IMemberConfigurationExpression> configuration) =>
            _allPropertyMapActions.Add((pm, cfg) =>
            {
                if (condition(pm)) configuration(pm, cfg);
            });

        public IProjectionExpression<TSource, TDestination> CreateProjection<TSource, TDestination>() =>
            CreateProjection<TSource, TDestination>(MemberList.Destination);
        public IProjectionExpression<TSource, TDestination> CreateProjection<TSource, TDestination>(MemberList memberList) =>
            (IProjectionExpression<TSource, TDestination>)CreateMapCore<TSource, TDestination>(memberList, projection: true);
        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>() =>
            CreateMapCore<TSource, TDestination>(MemberList.Destination);
        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList) =>
            CreateMapCore<TSource, TDestination>(memberList);
        private IMappingExpression<TSource, TDestination> CreateMapCore<TSource, TDestination>(MemberList memberList, bool projection = false)
        {
            var mappingExp = new MappingExpression<TSource, TDestination>(memberList, projection);
            _typeMapConfigs.Add(mappingExp);
            return mappingExp;
        }

        public IMappingExpression CreateMap(Type sourceType, Type destinationType) => 
            CreateMap(sourceType, destinationType, MemberList.Destination);

        public IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList)
        {
            var types = new TypePair(sourceType, destinationType);
            var map = new MappingExpression(types, memberList);
            _typeMapConfigs.Add(map);
            if (types.IsGenericTypeDefinition)
            {
                _openTypeMapConfigs.Add(map);
            }
            return map;
        }
        public void ClearPrefixes() => _prePostfixName.Prefixes.Clear();
        public void ReplaceMemberName(string original, string newValue) =>
            DefaultMemberConfig.AddName<ReplaceName>(_ => _.AddReplace(original, newValue));
        public void RecognizePrefixes(params string[] prefixes) => _prePostfixName.Prefixes.AddRange(prefixes);
        public void RecognizePostfixes(params string[] postfixes) => _prePostfixName.Postfixes.AddRange(postfixes);
        public void RecognizeDestinationPrefixes(params string[] prefixes) => _prePostfixName.DestinationPrefixes.AddRange(prefixes);
        public void RecognizeDestinationPostfixes(params string[] postfixes) => _prePostfixName.DestinationPostfixes.AddRange(postfixes);
        public void AddGlobalIgnore(string propertyNameStartingWith) => _globalIgnore.Add(propertyNameStartingWith);
        IMemberConfiguration IProfileExpressionInternal.AddMemberConfiguration()
        {
            var condition = new MemberConfiguration();
            _memberConfigurations.Add(condition);
            return condition;
        }
        public void IncludeSourceExtensionMethods(Type type) => _sourceExtensionMethods.AddRange(
            type.GetMethods(TypeExtensions.StaticFlags).Where(m => m.GetParameters().Length == 1 && m.IsExtensionMethod()));
    }
}