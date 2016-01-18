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
    public class Profile : IProfileExpression
    {
        private ConfigurationStore _configurator;
        private readonly IConditionalObjectMapper _mapMissingTypes;

        public Profile(string profileName)
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
        }

        public string ProfileName { get; }

        public void DisableConstructorMapping()
        {
            ConstructorMappingEnabled = false;
        }

        public bool AllowNullDestinationValues { get; set; }

        public bool AllowNullCollections { get; set; }

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
            set
            {
                if (value)
                    _typeConfigurations.Add(_mapMissingTypes);
                else
                    _typeConfigurations.Remove(_mapMissingTypes);
            }
        }

        public void ForAllMaps(Action<TypeMap, IMappingExpression> configuration)
        {
            _configurator.ForAllMaps(ProfileName, configuration);
        }

        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            return CreateMap<TSource, TDestination>(MemberList.Destination);
        }

        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList)
        {
            return _configurator.CreateMap<TSource, TDestination>(ProfileName, memberList);
        }

        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(string profileName, MemberList memberList)
        {
            return _configurator.CreateMap<TSource, TDestination>(profileName, memberList);
        }

        public IMappingExpression CreateMap(Type sourceType, Type destinationType)
        {
            return CreateMap(sourceType, destinationType, MemberList.Destination);
        }

        public IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList)
        {
            var map = _configurator.CreateMap(sourceType, destinationType, memberList, ProfileName);

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
            _configurator.AddGlobalIgnore(propertyNameStartingWith);
        }

        /// <summary>
        /// Override this method in a derived class and call the CreateMap method to associate that map with this profile.
        /// Avoid calling the <see cref="Mapper"/> class from this method.
        /// </summary>
        protected internal virtual void Configure()
        {
            // override in a derived class for custom configuration behavior
        }

        public void Initialize(ConfigurationStore configurator)
        {
            _configurator = configurator;
            _configurator._formatterProfiles.AddOrUpdate(ProfileName, this, (s, configuration) => this);
        }

        
        private readonly List<MethodInfo> _sourceExtensionMethods = new List<MethodInfo>();

        private readonly IList<IMemberConfiguration> _memberConfigurations = new List<IMemberConfiguration>();

        public IMemberConfiguration DefaultMemberConfig
        {
            get
            {
                if(!_memberConfigurations.Any())
                    _memberConfigurations.Add(new MemberConfiguration().AddMember<NameSplitMember>().AddName<PrePostfixName>(_ => _.AddStrings(p => p.Prefixes, "Get")));
                return _memberConfigurations.First();
            }
        }

        public IEnumerable<IMemberConfiguration> MemberConfigurations
        {
            get
            {
                var temp = DefaultMemberConfig;
                return _memberConfigurations;
            }
        }
        public IMemberConfiguration AddMemberConfiguration()
        {
            var condition = new MemberConfiguration();
            _memberConfigurations.Add(condition);
            return condition;
        }
        private IList<IConditionalObjectMapper> _typeConfigurations = new List<IConditionalObjectMapper>();
        public IEnumerable<IConditionalObjectMapper> TypeConfigurations => _typeConfigurations;
        public IConditionalObjectMapper AddConditionalObjectMapper()
        {
            var condition = new ConditionalObjectMapper(ProfileName);
            _typeConfigurations.Add(condition);
            return condition;
        }

        public bool ConstructorMappingEnabled { get; private set; }

        public IEnumerable<MethodInfo> SourceExtensionMethods => _sourceExtensionMethods;

        public Func<PropertyInfo, bool> ShouldMapProperty { get; set; }

        public Func<FieldInfo, bool> ShouldMapField { get; set; }

        public void IncludeSourceExtensionMethods(Assembly assembly)
        {
            //http://stackoverflow.com/questions/299515/c-sharp-reflection-to-identify-extension-methods
            _sourceExtensionMethods.AddRange(assembly.ExportedTypes
                .Where(type => type.IsSealed() && !type.IsGenericType() && !type.IsNested)
                .SelectMany(type => type.GetDeclaredMethods().Where(mi => mi.IsStatic))
                .Where(method => method.IsDefined(typeof(ExtensionAttribute), false))
                .Where(method => method.GetParameters().Length == 1));
        }

        public IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList, string profileName)
        {
            return _configurator.CreateMap(sourceType, destinationType, memberList, profileName);
        }
    }
}