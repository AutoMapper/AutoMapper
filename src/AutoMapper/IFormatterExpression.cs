using System;

namespace AutoMapper
{
    using System.Reflection;

    [Obsolete("Formatters should not be used.")]
    public interface IFormatterExpression
	{
        [Obsolete("Formatters should not be used.")]
        IFormatterCtorExpression<TValueFormatter> AddFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter;
        [Obsolete("Formatters should not be used.")]
        IFormatterCtorExpression AddFormatter(Type valueFormatterType);
        [Obsolete("Formatters should not be used.")]
        void AddFormatter(IValueFormatter formatter);
        [Obsolete("Formatters should not be used.")]
        void AddFormatExpression(Func<ResolutionContext, string> formatExpression);
        [Obsolete("Formatters should not be used.")]
        void SkipFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter;
        [Obsolete("Formatters should not be used.")]
        IFormatterExpression ForSourceType<TSource>();
    }

    [Obsolete("Formatters should not be used.")]
	public interface IFormatterCtorExpression
	{
		void ConstructedBy(Func<IValueFormatter> constructor);
	}

    [Obsolete("Formatters should not be used.")]
    public interface IFormatterCtorExpression<TValueFormatter>
		where TValueFormatter : IValueFormatter
	{
		void ConstructedBy(Func<TValueFormatter> constructor);
	}

    /// <summary>
    /// Configuration for profile-specific maps
    /// </summary>
	public interface IProfileExpression : IFormatterExpression, IMappingOptions
	{
        /// <summary>
        /// Creates a mapping configuration from the <typeparamref name="TSource"/> type to the <typeparamref name="TDestination"/> type
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <returns>Mapping expression for more configuration options</returns>
        IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>();

        /// <summary>
        /// Creates a mapping configuration from the <typeparamref name="TSource"/> type to the <typeparamref name="TDestination"/> type.
        /// Specify the member list to validate against during configuration validation.
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="memberList">Member list to validate</param>
        /// <returns>Mapping expression for more configuration options</returns>
        IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList);

        /// <summary>
        /// Create a mapping configuration from the source type to the destination type.
        /// Use this method when the source and destination type are known at runtime and not compile time.
        /// </summary>
        /// <param name="sourceType">Source type</param>
        /// <param name="destinationType">Destination type</param>
        /// <returns>Mapping expression for more configuration options</returns>
        IMappingExpression CreateMap(Type sourceType, Type destinationType);

        /// <summary>
        /// Creates a mapping configuration from the source type to the destination type.
        /// Specify the member list to validate against during configuration validation.
        /// </summary>
        /// <param name="sourceType">Source type</param>
        /// <param name="destinationType">Destination type</param>
        /// <param name="memberList">Member list to validate</param>
        /// <returns>Mapping expression for more configuration options</returns>
        IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList);

        /// <summary>
        /// Recognize a list of prefixes to be removed from source member names when matching
        /// </summary>
        /// <param name="prefixes">List of prefixes</param>
		void RecognizePrefixes(params string[] prefixes);

        /// <summary>
        /// Recognize a list of postfixes to be removed from source member names when matching
        /// </summary>
        /// <param name="postfixes">List of postfixes</param>
		void RecognizePostfixes(params string[] postfixes);

        /// <summary>
        /// Provide an alias for a member name when matching source member names
        /// </summary>
        /// <param name="original">Original member name</param>
        /// <param name="alias">Alias to match against</param>
		void RecognizeAlias(string original, string alias);

        
        /// <summary>
        /// Provide a newvalue for a part of a members name
        /// </summary>
        /// <param name="original">Original member value</param>
        /// <param name="newValue">New member value</param>
		void ReplaceMemberName(string original, string newValue);

        /// <summary>
        /// Recognize a list of prefixes to be removed from destination member names when matching
        /// </summary>
        /// <param name="prefixes">List of prefixes</param>
        void RecognizeDestinationPrefixes(params string[] prefixes);

        /// <summary>
        /// Recognize a list of postfixes to be removed from destination member names when matching
        /// </summary>
        /// <param name="postfixes">List of postfixes</param>
        void RecognizeDestinationPostfixes(params string[] postfixes);

        /// <summary>
        /// Add a property name to globally ignore. Matches against the beginning of the property names.
        /// </summary>
        /// <param name="propertyNameStartingWith">Property name to match against</param>
        void AddGlobalIgnore(string propertyNameStartingWith);

        /// <summary>
        /// Allow null destination values. If false, destination objects will be created for deep object graphs. Default true.
        /// </summary>
        bool AllowNullDestinationValues { get; set; }

        /// <summary>
        /// Allow null destination collections. If true, null source collections result in null destination collections. Default false.
        /// </summary>
        bool AllowNullCollections { get; set; }

        /// <summary>
        /// Include an assembly to search for extension methods to match
        /// </summary>
        /// <param name="assembly">Assembly containing extension methods</param>
        void IncludeSourceExtensionMethods(Assembly assembly);
	}

	public interface IConfiguration : IProfileExpression
	{
        /// <summary>
        /// Create a named profile for grouped mapping configuration
        /// </summary>
        /// <param name="profileName">Profile name</param>
        /// <returns>Profile configuration options</returns>
        IProfileExpression CreateProfile(string profileName);

        /// <summary>
        /// Create a named profile for grouped mapping configuration, and configure the profile
        /// </summary>
        /// <param name="profileName">Profile name</param>
        /// <param name="profileConfiguration">Profile configuration callback</param>
        void CreateProfile(string profileName, Action<IProfileExpression> profileConfiguration);

        /// <summary>
        /// Add an existing profile
        /// </summary>
        /// <param name="profile">Profile to add</param>
        void AddProfile(Profile profile);

        /// <summary>
        /// Add an existing profile type. Profile will be instantiated and added to the configuration.
        /// </summary>
        /// <typeparam name="TProfile">Profile type</typeparam>
        void AddProfile<TProfile>() where TProfile : Profile, new();
		
        /// <summary>
        /// Supply a factory method callback for creating formatters, resolvers and type converters
        /// </summary>
        /// <param name="constructor">Factory method</param>
        void ConstructServicesUsing(Func<Type, object> constructor);

        /// <summary>
        /// Disable constructor mapping. Use this if you don't intend to have AutoMapper try to map to constructors
        /// </summary>
	    void DisableConstructorMapping();

        /// <summary>
        /// Seal the configuration and optimize maps
        /// </summary>
		void Seal();

        /// <summary>
        /// Mapping via a data reader will yield return each item, keeping a data reader open instead of eagerly evaluating
        /// </summary>
	    void EnableYieldReturnForDataReaderMapper();
	}
}
