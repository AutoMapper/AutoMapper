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

        public Profile(string profileName)
        {
            ProfileName = profileName;
        }

        protected Profile()
        {
            ProfileName = GetType().FullName;
        }

        public string ProfileName { get; }

        public void DisableConstructorMapping()
        {
            GetProfile.ConstructorMappingEnabled = false;
        }

        public Func<PropertyInfo, bool> ShouldMapProperty
        {
            get { return GetProfile.ShouldMapProperty; }
            set { GetProfile.ShouldMapProperty = value; }
        }

        public Func<FieldInfo, bool> ShouldMapField
        {
            get { return GetProfile.ShouldMapField; }
            set { GetProfile.ShouldMapField = value; }
        }

        public bool AllowNullDestinationValues
        {
            get { return GetProfile.MapNullSourceValuesAsNull; }
            set { GetProfile.MapNullSourceValuesAsNull = value; }
        }

        public bool AllowNullCollections
        {
            get { return GetProfile.MapNullSourceCollectionsAsNull; }
            set { GetProfile.MapNullSourceCollectionsAsNull = value; }
        }

        public void IncludeSourceExtensionMethods(Assembly assembly)
        {
            GetProfile.IncludeSourceExtensionMethods(assembly);
        }

        public INamingConvention SourceMemberNamingConvention
        {
            get
            {
                INamingConvention convention = null;
                GetProfile.MemberConfigurations[0].AddMember<NameSplitMember>(_ => convention = _.SourceMemberNamingConvention);
                return convention;
            }
            set { GetProfile.MemberConfigurations[0].AddMember<NameSplitMember>(_ => _.SourceMemberNamingConvention = value); }
        }

        public INamingConvention DestinationMemberNamingConvention
        {
            get
            {
                INamingConvention convention = null;
                GetProfile.MemberConfigurations[0].AddMember<NameSplitMember>(_ => convention = _.DestinationMemberNamingConvention);
                return convention;
            }
            set { GetProfile.MemberConfigurations[0].AddMember<NameSplitMember>(_ => _.DestinationMemberNamingConvention = value); }
        }

        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            return CreateMap<TSource, TDestination>(MemberList.Destination);
        }

        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList)
        {
            var map = _configurator.CreateMap<TSource, TDestination>(ProfileName, memberList);

            return map;
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
            GetProfile.MemberConfigurations[0].AddName<PrePostfixName>(_ => _.Prefixes.Clear());
        }

        public void RecognizeAlias(string original, string alias)
        {
            GetProfile.MemberConfigurations[0].AddName<ReplaceName>(_ => _.AddReplace(original, alias));
        }

        public void ReplaceMemberName(string original, string newValue)
        {
            GetProfile.MemberConfigurations[0].AddName<ReplaceName>(_ => _.AddReplace(original, newValue));
        }

        public void RecognizePrefixes(params string[] prefixes)
        {
            GetProfile.MemberConfigurations[0].AddName<PrePostfixName>(_ => _.AddStrings(p => p.Prefixes, prefixes));
        }

        public void RecognizePostfixes(params string[] postfixes)
        {
            GetProfile.MemberConfigurations[0].AddName<PrePostfixName>(_ => _.AddStrings(p => p.Postfixes, postfixes));
        }

        public void RecognizeDestinationPrefixes(params string[] prefixes)
        {
            GetProfile.MemberConfigurations[0].AddName<PrePostfixName>(_ => _.AddStrings(p => p.DestinationPrefixes, prefixes));
        }

        public void RecognizeDestinationPostfixes(params string[] postfixes)
        {
            GetProfile.MemberConfigurations[0].AddName<PrePostfixName>(_ => _.AddStrings(p => p.DestinationPostfixes, postfixes));
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

        public IProfileConfiguration GetProfile => _configurator.GetProfile(ProfileName);
    }
}