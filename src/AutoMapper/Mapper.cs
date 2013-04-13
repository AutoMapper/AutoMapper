using System;
using AutoMapper.Mappers;

namespace AutoMapper
{
    using Internal;

    /// <summary>
    /// Main entry point for AutoMapper, for both creating maps and performing maps.
    /// </summary>
    public static class Mapper
	{
	    private static readonly Func<ConfigurationStore> _configurationInit =
	        () => new ConfigurationStore(new TypeMapFactory(), PlatformAdapter.Resolve<IMapperRegistry>().GetMappers());
        
        private static ILazy<ConfigurationStore> _configuration = LazyFactory.Create(_configurationInit);

        private static readonly Func<IMappingEngine> _mappingEngineInit = 
            () => new MappingEngine(_configuration.Value);

        private static ILazy<IMappingEngine> _mappingEngine = LazyFactory.Create(_mappingEngineInit);

        /// <summary>
        /// When set, destination can have null values. Defaults to true.
        /// This does not affect simple types, only complex ones.
        /// </summary>
		public static bool AllowNullDestinationValues
		{
			get { return Configuration.AllowNullDestinationValues; }
			set { Configuration.AllowNullDestinationValues = value; }
		}

        /// <summary>
        /// Execute a mapping from the source object to a new destination object.
        /// The source type is inferred from the source object.
        /// </summary>
        /// <typeparam name="TDestination">Destination type to create</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <returns>Mapped destination object</returns>
        public static TDestination Map<TDestination>(object source)
        {
            return Engine.Map<TDestination>(source);
        }

        /// <summary>
        /// Execute a mapping from the source object to a new destination object with supplied mapping options.
        /// </summary>
        /// <typeparam name="TDestination">Destination type to create</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>Mapped destination object</returns>
        public static TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> opts)
        {
            return Engine.Map<TDestination>(source, opts);
        }

        /// <summary>
        /// Execute a mapping from the source object to a new destination object.
        /// </summary>
        /// <typeparam name="TSource">Source type to use, regardless of the runtime type</typeparam>
        /// <typeparam name="TDestination">Destination type to create</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <returns>Mapped destination object</returns>
		public static TDestination Map<TSource, TDestination>(TSource source)
		{
			return Engine.Map<TSource, TDestination>(source);
		}

        /// <summary>
        /// Execute a mapping from the source object to the existing destination object.
        /// </summary>
        /// <typeparam name="TSource">Source type to use</typeparam>
        /// <typeparam name="TDestination">Dsetination type</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination">Destination object to map into</param>
        /// <returns>The mapped destination object, same instance as the <paramref name="destination"/> object</returns>
		public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
		{
			return Engine.Map(source, destination);
		}

        /// <summary>
        /// Execute a mapping from the source object to the existing destination object with supplied mapping options.
        /// </summary>
        /// <typeparam name="TSource">Source type to use</typeparam>
        /// <typeparam name="TDestination">Destination type</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination">Destination object to map into</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>The mapped destination object, same instance as the <paramref name="destination"/> object</returns>
        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions> opts)
        {
            return Engine.Map(source, destination, opts);
        }

        /// <summary>
        /// Execute a mapping from the source object to a new destination object with supplied mapping options.
        /// </summary>
        /// <typeparam name="TSource">Source type to use</typeparam>
        /// <typeparam name="TDestination">Destination type to create</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>Mapped destination object</returns>
		public static TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions> opts)
		{
            return Engine.Map<TSource, TDestination>(source, opts);
		}

        /// <summary>
        /// Execute a mapping from the source object to a new destination object with explicit <see cref="System.Type"/> objects
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to create</param>
        /// <returns>Mapped destination object</returns>
		public static object Map(object source, Type sourceType, Type destinationType)
		{
			return Engine.Map(source, sourceType, destinationType);
		}

        /// <summary>
        /// Execute a mapping from the source object to a new destination object with explicit <see cref="System.Type"/> objects and supplied mapping options.
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to create</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>Mapped destination object</returns>
        public static object Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            return Engine.Map(source, sourceType, destinationType, opts);
        }

        /// <summary>
        /// Execute a mapping from the source object to existing destination object with explicit <see cref="System.Type"/> objects
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination">Destination object to map into</param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to use</param>
        /// <returns>Mapped destination object, same instance as the <paramref name="destination"/> object</returns>
		public static object Map(object source, object destination, Type sourceType, Type destinationType)
		{
			return Engine.Map(source, destination, sourceType, destinationType);
		}

        /// <summary>
        /// Execute a mapping from the source object to existing destination object with supplied mapping options and explicit <see cref="System.Type"/> objects
        /// </summary>
        /// <param name="source">Source object to map from</param>
        /// <param name="destination">Destination object to map into</param>
        /// <param name="sourceType">Source type to use</param>
        /// <param name="destinationType">Destination type to use</param>
        /// <param name="opts">Mapping options</param>
        /// <returns>Mapped destination object, same instance as the <paramref name="destination"/> object</returns>
        public static object Map(object source, object destination, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            return Engine.Map(source, destination, sourceType, destinationType, opts);
        }

		public static TDestination DynamicMap<TSource, TDestination>(TSource source)
		{
			return Engine.DynamicMap<TSource, TDestination>(source);
		}

		public static void DynamicMap<TSource, TDestination>(TSource source, TDestination destination)
		{
			Engine.DynamicMap<TSource, TDestination>(source, destination);
		}

		public static TDestination DynamicMap<TDestination>(object source)
		{
			return Engine.DynamicMap<TDestination>(source);
		}

		public static object DynamicMap(object source, Type sourceType, Type destinationType)
		{
			return Engine.DynamicMap(source, sourceType, destinationType);
		}

		public static void DynamicMap(object source, object destination, Type sourceType, Type destinationType)
		{
			Engine.DynamicMap(source, destination, sourceType, destinationType);
		}

		public static void Initialize(Action<IConfiguration> action)
		{
			Reset();

			action(Configuration);

			Configuration.Seal();
		}

		public static IFormatterCtorExpression<TValueFormatter> AddFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			return Configuration.AddFormatter<TValueFormatter>();
		}

		public static IFormatterCtorExpression AddFormatter(Type valueFormatterType)
		{
			return Configuration.AddFormatter(valueFormatterType);
		}

		public static void AddFormatter(IValueFormatter formatter)
		{
			Configuration.AddFormatter(formatter);
		}

		public static void AddFormatExpression(Func<ResolutionContext, string> formatExpression)
		{
			Configuration.AddFormatExpression(formatExpression);
		}

		public static void SkipFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			Configuration.SkipFormatter<TValueFormatter>();
		}

		public static IFormatterExpression ForSourceType<TSource>()
		{
			return Configuration.ForSourceType<TSource>();
		}

		public static IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
		{
			return Configuration.CreateMap<TSource, TDestination>();
		}

        public static IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList)
        {
            return Configuration.CreateMap<TSource, TDestination>(memberList);
        }

		public static IMappingExpression CreateMap(Type sourceType, Type destinationType)
		{
			return Configuration.CreateMap(sourceType, destinationType);
		}

		public static IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList)
		{
			return Configuration.CreateMap(sourceType, destinationType, memberList);
		}

		public static IProfileExpression CreateProfile(string profileName)
		{
			return Configuration.CreateProfile(profileName);
		}

		public static void CreateProfile(string profileName, Action<IProfileExpression> profileConfiguration)
		{
			Configuration.CreateProfile(profileName, profileConfiguration);
		}

		public static void AddProfile(Profile profile)
		{
			Configuration.AddProfile(profile);
		}

		public static void AddProfile<TProfile>() where TProfile : Profile, new()
		{
			Configuration.AddProfile<TProfile>();
		}

		public static TypeMap FindTypeMapFor(Type sourceType, Type destinationType)
		{
			return ConfigurationProvider.FindTypeMapFor(sourceType, destinationType);
		}

		public static TypeMap FindTypeMapFor<TSource, TDestination>()
		{
			return ConfigurationProvider.FindTypeMapFor(typeof(TSource), typeof(TDestination));
		}

		public static TypeMap[] GetAllTypeMaps()
		{
			return ConfigurationProvider.GetAllTypeMaps();
		}

		public static void AssertConfigurationIsValid()
		{
			ConfigurationProvider.AssertConfigurationIsValid();
		}

		public static void AssertConfigurationIsValid(TypeMap tm)
		{
			ConfigurationProvider.AssertConfigurationIsValid(tm);
		}

		public static void AssertConfigurationIsValid(string profileName)
		{
			ConfigurationProvider.AssertConfigurationIsValid(profileName);
		}

		public static void Reset()
		{
            _configuration = LazyFactory.Create(_configurationInit);
            _mappingEngine = LazyFactory.Create(_mappingEngineInit);
		}

		public static IMappingEngine Engine
		{
			get
			{
			    return _mappingEngine.Value;
			}
		}

	    public static IConfiguration Configuration
	    {
	        get { return (IConfiguration) ConfigurationProvider; }
	    }

	    private static IConfigurationProvider ConfigurationProvider
		{
			get
			{
			    return _configuration.Value;
			}
		}

	    public static void AddGlobalIgnore(string startingwith)
	    {
            Configuration.AddGlobalIgnore(startingwith);
	    }
	}
}