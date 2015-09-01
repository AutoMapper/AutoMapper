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

        internal Profile(string profileName)
        {
            ProfileName = profileName;
        }

        protected Profile()
        {
            ProfileName = GetType().FullName;
        }

        public virtual string ProfileName { get; }

        public void DisableConstructorMapping()
        {
            GetProfile().ConstructorMappingEnabled = false;
        }

        public Func<PropertyInfo, bool> ShouldMapProperty
        {
            get { return GetProfile().ShouldMapProperty; }
            set { GetProfile().ShouldMapProperty = value; }
        }

        public Func<FieldInfo, bool> ShouldMapField
        {
            get { return GetProfile().ShouldMapField; }
            set { GetProfile().ShouldMapField = value; }
        }

        public bool AllowNullDestinationValues
        {
            get { return GetProfile().AllowNullDestinationValues; }
            set { GetProfile().AllowNullDestinationValues = value; }
        }

        public bool AllowNullCollections
        {
            get { return GetProfile().AllowNullCollections; }
            set { GetProfile().AllowNullCollections = value; }
        }

        public void IncludeSourceExtensionMethods(Assembly assembly)
        {
            GetProfile().IncludeSourceExtensionMethods(assembly);
        }

        public INamingConvention SourceMemberNamingConvention
        {
            get { return GetProfile().SourceMemberNamingConvention; }
            set { GetProfile().SourceMemberNamingConvention = value; }
        }

        public INamingConvention DestinationMemberNamingConvention
        {
            get { return GetProfile().DestinationMemberNamingConvention; }
            set { GetProfile().DestinationMemberNamingConvention = value; }
        }

        public IEnumerable<string> Prefixes => GetProfile().Prefixes;

        public IEnumerable<string> Postfixes => GetProfile().Postfixes;

        public IEnumerable<string> DestinationPrefixes => GetProfile().DestinationPrefixes;

        public IEnumerable<string> DestinationPostfixes => GetProfile().DestinationPostfixes;

        public IEnumerable<MemberNameReplacer> MemberNameReplacers
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<AliasedMember> Aliases
        {
            get { throw new NotImplementedException(); }
        }

        public bool ConstructorMappingEnabled => _configurator.ConstructorMappingEnabled;

        public bool DataReaderMapperYieldReturnEnabled => _configurator.DataReaderMapperYieldReturnEnabled;

        public IEnumerable<MethodInfo> SourceExtensionMethods => GetProfile().SourceExtensionMethods;

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
            GetProfile().ClearPrefixes();
        }

        public void RecognizeAlias(string original, string alias)
        {
            GetProfile().RecognizeAlias(original, alias);
        }

        public void ReplaceMemberName(string original, string newValue)
        {
            GetProfile().ReplaceMemberName(original, newValue);
        }

        public void RecognizePrefixes(params string[] prefixes)
        {
            GetProfile().RecognizePrefixes(prefixes);
        }

        public void RecognizePostfixes(params string[] postfixes)
        {
            GetProfile().RecognizePostfixes(postfixes);
        }

        public void RecognizeDestinationPrefixes(params string[] prefixes)
        {
            GetProfile().RecognizeDestinationPrefixes(prefixes);
        }

        public void RecognizeDestinationPostfixes(params string[] postfixes)
        {
            GetProfile().RecognizeDestinationPostfixes(postfixes);
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
        }

        private ProfileConfiguration GetProfile()
        {
            return _configurator.GetProfile(ProfileName);
        }

        public IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList, string profileName)
        {
            return _configurator.CreateMap(sourceType, destinationType, memberList, profileName);
        }
    }
}