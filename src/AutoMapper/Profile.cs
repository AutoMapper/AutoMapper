using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper
{
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

	    public virtual string ProfileName { get; private set; }

        public void DisableConstructorMapping()
        {
            GetProfile().ConstructorMappingEnabled = false;
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

		public IEnumerable<string> Prefixes
	    {
	        get { return GetProfile().Prefixes; }
	    }

	    public IEnumerable<string> Postfixes
	    {
	        get { return GetProfile().Postfixes; }
	    }

	    public IEnumerable<string> DestinationPrefixes
	    {
	        get { return GetProfile().DestinationPrefixes; }
	    }

	    public IEnumerable<string> DestinationPostfixes
	    {
	        get { return GetProfile().DestinationPostfixes; }
	    }

        public IEnumerable<MemberNameReplacer> MemberNameReplacers
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<AliasedMember> Aliases
	    {
	        get { throw new NotImplementedException(); }
	    }

	    public bool ConstructorMappingEnabled
	    {
	        get { return _configurator.ConstructorMappingEnabled; }
	    }

	    public bool DataReaderMapperYieldReturnEnabled
	    {
            get { return _configurator.DataReaderMapperYieldReturnEnabled; }
	    }

        public IEnumerable<MethodInfo> SourceExtensionMethods
        {
            get { return GetProfile().SourceExtensionMethods; }
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

		private FormatterExpression GetProfile()
		{
			return _configurator.GetProfile(ProfileName);
		}
	}
}