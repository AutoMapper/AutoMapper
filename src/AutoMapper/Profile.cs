using System.Linq;
using System.Runtime.CompilerServices;
using AutoMapper.Mappers;

namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Internal;
    
    /// <summary>
    /// Provides a named configuration for maps. Naming conventions become scoped per profile.
    /// </summary>
    public abstract class Profile : IProfileExpression, IProfileConfiguration
    {
        private IConfiguration _configurator;
        private readonly IConditionalObjectMapper _mapMissingTypes;
        private readonly List<string> _globalIgnore;

        protected Profile(string profileName)
            :this()
        {
            ProfileName = profileName;
        }

        protected Profile()
        {
            ProfileName = GetType().FullName;
            AllowNullDestinationValues = true;
            ConstructorMappingEnabled = true;
            IncludeSourceExtensionMethods(typeof(Enumerable).Assembly());
            ShouldMapProperty = p => p.IsPublic();
            ShouldMapField = f => f.IsPublic;
            _mapMissingTypes = new ConditionalObjectMapper(ProfileName) {Conventions = {tp => true}};
            _globalIgnore = new List<string>();
            _memberConfigurations.Add(new MemberConfiguration().AddMember<NameSplitMember>().AddName<PrePostfixName>(_ => _.AddStrings(p => p.Prefixes, "Get")));
        }

        public virtual string ProfileName { get; }

        [Obsolete("Use interface calls")]
        public void DisableConstructorMapping() => ((IProfileExpression) this).DisableConstructorMapping();
        void IProfileExpression.DisableConstructorMapping()
        {
            ConstructorMappingEnabled = false;
        }

        [Obsolete("Use interface calls")]
        public bool AllowNullDestinationValues
        {
            get { return ((IProfileExpression)this).AllowNullDestinationValues; }
            set { ((IProfileExpression)this).AllowNullDestinationValues = value; }
        }
        bool IProfileConfiguration.AllowNullDestinationValues => ((IProfileExpression)this).AllowNullDestinationValues;
        bool IProfileExpression.AllowNullDestinationValues { get; set; }

        [Obsolete("Use interface calls")]
        public bool AllowNullCollections
        {
            get { return ((IProfileExpression)this).AllowNullCollections; }
            set { ((IProfileExpression)this).AllowNullCollections = value; }
        }
        bool IProfileConfiguration.AllowNullCollections => ((IProfileExpression)this).AllowNullCollections;
        bool IProfileExpression.AllowNullCollections { get; set; }

        public IEnumerable<string> GlobalIgnores => _globalIgnore;

        [Obsolete("Use interface calls")]
        public INamingConvention SourceMemberNamingConvention
        {
            get { return ((IProfileExpression)this).SourceMemberNamingConvention; }
            set { ((IProfileExpression)this).SourceMemberNamingConvention = value; }
        }
        INamingConvention IProfileConfiguration.SourceMemberNamingConvention => ((IProfileExpression)this).SourceMemberNamingConvention;
        INamingConvention IProfileExpression.SourceMemberNamingConvention
        {
            get
            {
                INamingConvention convention = null;
                DefaultMemberConfig.AddMember<NameSplitMember>(_ => convention = _.SourceMemberNamingConvention);
                return convention;
            }
            set { DefaultMemberConfig.AddMember<NameSplitMember>(_ => _.SourceMemberNamingConvention = value); }
        }

        [Obsolete("Use interface calls")]
        public INamingConvention DestinationMemberNamingConvention
        {
            get { return ((IProfileExpression)this).DestinationMemberNamingConvention; }
            set { ((IProfileExpression)this).DestinationMemberNamingConvention = value; }
        }
        INamingConvention IProfileConfiguration.DestinationMemberNamingConvention => ((IProfileExpression)this).DestinationMemberNamingConvention;
        INamingConvention IProfileExpression.DestinationMemberNamingConvention
        {
            get
            {
                INamingConvention convention = null;
                DefaultMemberConfig.AddMember<NameSplitMember>(_ => convention = _.DestinationMemberNamingConvention);
                return convention;
            }
            set { DefaultMemberConfig.AddMember<NameSplitMember>(_ => _.DestinationMemberNamingConvention = value); }
        }

        [Obsolete("Use interface calls")]
        public bool CreateMissingTypeMaps
        {
            get { return ((IProfileExpression)this).CreateMissingTypeMaps; }
            set { ((IProfileExpression)this).CreateMissingTypeMaps = value; }
        }
        bool IProfileConfiguration.CreateMissingTypeMaps => ((IProfileExpression)this).CreateMissingTypeMaps;
        bool IProfileExpression.CreateMissingTypeMaps
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

        [Obsolete("Use interface calls")]
        public void ForAllMaps(Action<TypeMap, IMappingExpression> configuration) => ((IProfileExpression)this).ForAllMaps(configuration);
        void IProfileExpression.ForAllMaps(Action<TypeMap, IMappingExpression> configuration) => _configurator.ForAllMaps(ProfileName, configuration);

        [Obsolete("Use interface calls")]
        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>() => ((IProfileExpression)this).CreateMap<TSource,TDestination>();
        IMappingExpression <TSource, TDestination> IProfileExpression.CreateMap<TSource, TDestination>()
        {
            return CreateMap<TSource, TDestination>(MemberList.Destination);
        }

        [Obsolete("Use interface calls")]
        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList) => ((IProfileExpression)this).CreateMap<TSource, TDestination>(memberList);
        IMappingExpression<TSource, TDestination> IProfileExpression.CreateMap<TSource, TDestination>(MemberList memberList)
        {
            return _configurator.CreateMap<TSource, TDestination>(ProfileName, memberList);
        }

        [Obsolete("Use interface calls")]
        public IMappingExpression CreateMap(Type sourceType, Type destinationType) => ((IProfileExpression)this).CreateMap(sourceType, destinationType);
        IMappingExpression IProfileExpression.CreateMap(Type sourceType, Type destinationType)
        {
            return CreateMap(sourceType, destinationType, MemberList.Destination);
        }

        [Obsolete("Use interface calls")]
        public IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList) => ((IProfileExpression)this).CreateMap(sourceType, destinationType, memberList);
        IMappingExpression IProfileExpression.CreateMap(Type sourceType, Type destinationType, MemberList memberList)
        {
            var map = _configurator.CreateMap(sourceType, destinationType, memberList, ProfileName);

            return map;
        }

        [Obsolete("Use interface calls")]
        public void ClearPrefixes() => ((IProfileExpression)this).ClearPrefixes();
        void IProfileExpression.ClearPrefixes()
        {
            DefaultMemberConfig.AddName<PrePostfixName>(_ => _.Prefixes.Clear());
        }

        [Obsolete("Use interface calls")]
        public void RecognizeAlias(string original, string alias) => ((IProfileExpression)this).RecognizeAlias(original, alias);
        void IProfileExpression.RecognizeAlias(string original, string alias)
        {
            DefaultMemberConfig.AddName<ReplaceName>(_ => _.AddReplace(original, alias));
        }

        [Obsolete("Use interface calls")]
        public void ReplaceMemberName(string original, string newValue) => ((IProfileExpression)this).ReplaceMemberName(original, newValue);
        void IProfileExpression.ReplaceMemberName(string original, string newValue)
        {
            DefaultMemberConfig.AddName<ReplaceName>(_ => _.AddReplace(original, newValue));
        }

        [Obsolete("Use interface calls")]
        public void RecognizePrefixes(params string[] prefixes) => ((IProfileExpression)this).RecognizePrefixes(prefixes);
        void IProfileExpression.RecognizePrefixes(params string[] prefixes)
        {
            DefaultMemberConfig.AddName<PrePostfixName>(_ => _.AddStrings(p => p.Prefixes, prefixes));
        }

        [Obsolete("Use interface calls")]
        public void RecognizePostfixes(params string[] postfixes) => ((IProfileExpression)this).RecognizePostfixes(postfixes);
        void IProfileExpression.RecognizePostfixes(params string[] postfixes)
        {
            DefaultMemberConfig.AddName<PrePostfixName>(_ => _.AddStrings(p => p.Postfixes, postfixes));
        }

        [Obsolete("Use interface calls")]
        public void RecognizeDestinationPrefixes(params string[] prefixes) => ((IProfileExpression)this).RecognizeDestinationPrefixes(prefixes);
        void IProfileExpression.RecognizeDestinationPrefixes(params string[] prefixes)
        {
            DefaultMemberConfig.AddName<PrePostfixName>(_ => _.AddStrings(p => p.DestinationPrefixes, prefixes));
        }

        [Obsolete("Use interface calls")]
        public void RecognizeDestinationPostfixes(params string[] postfixes) => ((IProfileExpression)this).RecognizeDestinationPostfixes(postfixes);
        void IProfileExpression.RecognizeDestinationPostfixes(params string[] postfixes)
        {
            DefaultMemberConfig.AddName<PrePostfixName>(_ => _.AddStrings(p => p.DestinationPostfixes, postfixes));
        }

        [Obsolete("Use interface calls")]
        public void AddGlobalIgnore(string propertyNameStartingWith) => ((IProfileExpression)this).AddGlobalIgnore(propertyNameStartingWith);
        void IProfileExpression.AddGlobalIgnore(string propertyNameStartingWith)
        {
            _globalIgnore.Add(propertyNameStartingWith);
        }

        /// <summary>
        /// Override this method in a derived class and call the CreateMap method to associate that map with this profile.
        /// Avoid calling the <see cref="Mapper"/> class from this method.
        /// </summary>
        [Obsolete("Set ConfigurationAction instead")]
        protected virtual void Configure()
        {
            ConfigurationAction(this);
        }

        public Action<IProfileExpression> ConfigurationAction { get; protected set; } = expression => { };

        internal void Initialize(IConfiguration configurator)
        {
            _configurator = configurator;

            Configure();
        }

        
        private readonly List<MethodInfo> _sourceExtensionMethods = new List<MethodInfo>();

        private readonly IList<IMemberConfiguration> _memberConfigurations = new List<IMemberConfiguration>();

        private IMemberConfiguration DefaultMemberConfig => ((IProfileConfiguration)this).DefaultMemberConfig;
        IMemberConfiguration IProfileConfiguration.DefaultMemberConfig => _memberConfigurations.First();

        [Obsolete("Use interface calls")]
        public IEnumerable<IMemberConfiguration> MemberConfigurations => ((IProfileConfiguration)this).MemberConfigurations;
        IEnumerable<IMemberConfiguration> IProfileConfiguration.MemberConfigurations => _memberConfigurations;

        [Obsolete("Use interface calls")]
        public IMemberConfiguration AddMemberConfiguration() => ((IProfileExpression)this).AddMemberConfiguration();
        IMemberConfiguration IProfileExpression.AddMemberConfiguration()
        {
            var condition = new MemberConfiguration();
            _memberConfigurations.Add(condition);
            return condition;
        }
        private readonly IList<IConditionalObjectMapper> _typeConfigurations = new List<IConditionalObjectMapper>();

        private bool _createMissingTypeMaps;

        [Obsolete("Use interface calls")]
        public IEnumerable<IConditionalObjectMapper> TypeConfigurations => ((IProfileConfiguration)this).TypeConfigurations;
        IEnumerable<IConditionalObjectMapper> IProfileConfiguration.TypeConfigurations => _typeConfigurations;

        [Obsolete("Use interface calls")]
        public IConditionalObjectMapper AddConditionalObjectMapper() => ((IProfileExpression)this).AddConditionalObjectMapper();
        IConditionalObjectMapper IProfileExpression.AddConditionalObjectMapper()
        {
            var condition = new ConditionalObjectMapper(ProfileName);

            _typeConfigurations.Add(condition);

            return condition;
        }

        public bool ConstructorMappingEnabled { get; private set; }

        public IEnumerable<MethodInfo> SourceExtensionMethods => _sourceExtensionMethods;
        
        [Obsolete("Use interface calls")]
        public Func<PropertyInfo, bool> ShouldMapProperty
        {
            get { return ((IProfileExpression)this).ShouldMapProperty; }
            set { ((IProfileExpression)this).ShouldMapProperty = value; }
        }
        Func<PropertyInfo, bool> IProfileConfiguration.ShouldMapProperty => ((IProfileExpression)this).ShouldMapProperty;
        Func<PropertyInfo, bool> IProfileExpression.ShouldMapProperty { get; set; }

        [Obsolete("Use interface calls")]
        public Func<FieldInfo, bool> ShouldMapField
        {
            get { return ((IProfileExpression)this).ShouldMapField; }
            set { ((IProfileExpression)this).ShouldMapField = value; }
        }
        Func<FieldInfo, bool> IProfileConfiguration.ShouldMapField => ((IProfileExpression)this).ShouldMapField;
        Func<FieldInfo, bool> IProfileExpression.ShouldMapField { get; set; }

        [Obsolete("Use interface calls")]
        public void IncludeSourceExtensionMethods(Assembly assembly) => ((IProfileExpression)this).IncludeSourceExtensionMethods(assembly);
        void IProfileExpression.IncludeSourceExtensionMethods(Assembly assembly)
        {
            //http://stackoverflow.com/questions/299515/c-sharp-reflection-to-identify-extension-methods
            _sourceExtensionMethods.AddRange(assembly.ExportedTypes
                .Where(type => type.IsSealed() && !type.IsGenericType() && !type.IsNested)
                .SelectMany(type => type.GetDeclaredMethods().Where(mi => mi.IsStatic))
                .Where(method => method.IsDefined(typeof(ExtensionAttribute), false))
                .Where(method => method.GetParameters().Length == 1));
        }
    }
}