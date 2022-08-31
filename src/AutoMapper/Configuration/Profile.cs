using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper.Configuration;
using AutoMapper.Configuration.Conventions;
using AutoMapper.Internal;
namespace AutoMapper
{
    using static Execution.ExpressionBuilder;
    public interface IProfileConfiguration
    {
        bool? FieldMappingEnabled { get; }
        bool? MethodMappingEnabled { get; }
        bool? ConstructorMappingEnabled { get; }
        bool? AllowNullDestinationValues { get; }
        bool? AllowNullCollections { get; }
        bool? EnableNullPropagationForQueryMapping { get; }
        IReadOnlyCollection<Action<TypeMap, IMappingExpression>> AllTypeMapActions { get; }
        IReadOnlyCollection<Action<PropertyMap, IMemberConfigurationExpression>> AllPropertyMapActions { get; }

        /// <summary>
        /// Source extension methods included for search
        /// </summary>
        IReadOnlyCollection<MethodInfo> SourceExtensionMethods { get; }

        /// <summary>
        /// Specify which properties should be mapped.
        /// By default only public properties are mapped.
        /// </summary>
        Func<PropertyInfo, bool> ShouldMapProperty { get; }

        /// <summary>
        /// Specify which fields should be mapped.
        /// By default only public fields are mapped.
        /// </summary>
        Func<FieldInfo, bool> ShouldMapField { get; }

        /// <summary>
        /// Specify which methods, of those that are eligible (public, parameterless, and non-static or extension methods), should be mapped.
        /// By default all eligible methods are mapped.
        /// </summary>
        Func<MethodInfo, bool> ShouldMapMethod { get; }

        /// <summary>
        /// Specify which constructors should be considered for the destination objects.
        /// By default all constructors are considered.
        /// </summary>
        Func<ConstructorInfo, bool> ShouldUseConstructor { get; }

        string ProfileName { get; }
        IReadOnlyCollection<string> GlobalIgnores { get; }
        INamingConvention SourceMemberNamingConvention { get; }
        INamingConvention DestinationMemberNamingConvention { get; }
        IReadOnlyCollection<TypeMapConfiguration> TypeMapConfigs { get; }
        IReadOnlyCollection<TypeMapConfiguration> OpenTypeMapConfigs { get; }
        IReadOnlyCollection<ValueTransformerConfiguration> ValueTransformers { get; }
    }
    /// <summary>
    ///     Provides a named configuration for maps. Naming conventions become scoped per profile.
    /// </summary>
    public abstract class Profile : IProfileExpressionInternal, IProfileConfiguration
    {
        private readonly List<string> _prefixes = new() { "Get" };
        private readonly List<string> _postfixes = new();
        private readonly List<TypeMapConfiguration> _typeMapConfigs = new();
        private readonly PrePostfixName _prePostfixName = new();
        private ReplaceName _replaceName;
        private readonly MemberConfiguration _memberConfiguration;
        private List<Action<PropertyMap, IMemberConfigurationExpression>> _allPropertyMapActions;
        private List<Action<TypeMap, IMappingExpression>> _allTypeMapActions;
        private List<string> _globalIgnores;
        private List<TypeMapConfiguration> _openTypeMapConfigs;
        private List<MethodInfo> _sourceExtensionMethods;
        private List<ValueTransformerConfiguration> _valueTransformerConfigs;
        private bool? _constructorMappingEnabled;
        protected Profile(string profileName) : this() => ProfileName = profileName;
        protected Profile()
        {
            ProfileName = GetType().FullName;
            _memberConfiguration = new(){ NameToMemberMappers = { _prePostfixName } };
        }
        protected Profile(string profileName, Action<IProfileExpression> configurationAction) : this(profileName)  => configurationAction(this);
        MemberConfiguration IProfileExpressionInternal.MemberConfiguration => _memberConfiguration;
        bool? IProfileConfiguration.ConstructorMappingEnabled => _constructorMappingEnabled;
        bool? IProfileExpressionInternal.MethodMappingEnabled { get; set; }
        bool? IProfileConfiguration.MethodMappingEnabled => this.Internal().MethodMappingEnabled;
        bool? IProfileExpressionInternal.FieldMappingEnabled { get; set; }
        bool? IProfileConfiguration.FieldMappingEnabled => this.Internal().FieldMappingEnabled;
        bool? IProfileConfiguration.EnableNullPropagationForQueryMapping => this.Internal().EnableNullPropagationForQueryMapping;
        IReadOnlyCollection<Action<PropertyMap, IMemberConfigurationExpression>> IProfileConfiguration.AllPropertyMapActions
            => _allPropertyMapActions.NullCheck();
        IReadOnlyCollection<Action<TypeMap, IMappingExpression>> IProfileConfiguration.AllTypeMapActions => _allTypeMapActions.NullCheck();
        IReadOnlyCollection<string> IProfileConfiguration.GlobalIgnores => _globalIgnores.NullCheck();
        IReadOnlyCollection<MethodInfo> IProfileConfiguration.SourceExtensionMethods => _sourceExtensionMethods.NullCheck();
        IReadOnlyCollection<TypeMapConfiguration> IProfileConfiguration.TypeMapConfigs => _typeMapConfigs;
        IReadOnlyCollection<TypeMapConfiguration> IProfileConfiguration.OpenTypeMapConfigs => _openTypeMapConfigs.NullCheck();
        IReadOnlyCollection<ValueTransformerConfiguration> IProfileConfiguration.ValueTransformers => _valueTransformerConfigs.NullCheck();

        public virtual string ProfileName { get; }
        public bool? AllowNullDestinationValues { get; set; }
        public bool? AllowNullCollections { get; set; }
        bool? IProfileExpressionInternal.EnableNullPropagationForQueryMapping { get; set; }
        public Func<PropertyInfo, bool> ShouldMapProperty { get; set; }
        public Func<FieldInfo, bool> ShouldMapField { get; set; }
        public Func<MethodInfo, bool> ShouldMapMethod { get; set; }
        public Func<ConstructorInfo, bool> ShouldUseConstructor { get; set; }
        public INamingConvention SourceMemberNamingConvention
        {
            get => _memberConfiguration.SourceNamingConvention;
            set => _memberConfiguration.SourceNamingConvention = value;
        }
        public INamingConvention DestinationMemberNamingConvention
        {
            get => _memberConfiguration.DestinationNamingConvention;
            set => _memberConfiguration.DestinationNamingConvention = value;
        }
        public List<ValueTransformerConfiguration> ValueTransformers => _valueTransformerConfigs ??= new();
        List<string> IProfileExpressionInternal.Prefixes => _prefixes;
        List<string> IProfileExpressionInternal.Postfixes => _postfixes;
        public void DisableConstructorMapping() => _constructorMappingEnabled = false;

        void IProfileExpressionInternal.ForAllMaps(Action<TypeMap, IMappingExpression> configuration)
        {
            _allTypeMapActions ??= new();
            _allTypeMapActions.Add(configuration);
        }

        void IProfileExpressionInternal.ForAllPropertyMaps(Func<PropertyMap, bool> condition, Action<PropertyMap, IMemberConfigurationExpression> configuration)
        {
            _allPropertyMapActions ??= new();
            _allPropertyMapActions.Add((pm, cfg) =>
            {
                if (condition(pm)) configuration(pm, cfg);
            });
        }
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
            if (types.ContainsGenericParameters)
            {
                _openTypeMapConfigs ??= new();
                _openTypeMapConfigs.Add(map);
            }
            return map;
        }
        public void ClearPrefixes() => _prefixes.Clear();
        public void ReplaceMemberName(string original, string newValue)
        {
            if (_replaceName == null)
            {
                _replaceName = new();
                _memberConfiguration.NameToMemberMappers.Add(_replaceName);
            }
            _replaceName.MemberNameReplacers.Add(new(original, newValue));
        }
        public void RecognizePrefixes(params string[] prefixes) => _prefixes.AddRange(prefixes);
        public void RecognizePostfixes(params string[] postfixes) => _postfixes.AddRange(postfixes);
        public void RecognizeDestinationPrefixes(params string[] prefixes) => _prePostfixName.DestinationPrefixes.UnionWith(prefixes);
        public void RecognizeDestinationPostfixes(params string[] postfixes) => _prePostfixName.DestinationPostfixes.UnionWith(postfixes);
        public void AddGlobalIgnore(string propertyNameStartingWith)
        {
            _globalIgnores ??= new();
            _globalIgnores.Add(propertyNameStartingWith);
        }
        public void IncludeSourceExtensionMethods(Type type)
        {
            _sourceExtensionMethods ??= new();
            _sourceExtensionMethods.AddRange(
                type.GetMethods(TypeExtensions.StaticFlags).Where(m => m.Has<ExtensionAttribute>() && m.GetParameters().Length == 1));
        }
    }
}