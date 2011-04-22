using System;
using AutoMapper.Mappers;

namespace AutoMapper
{
	public static class Mapper
	{
	    private static readonly Func<ConfigurationStore> _configurationInit =
	        () => new ConfigurationStore(new TypeMapFactory(), MapperRegistry.AllMappers());
        
        private static Lazy<ConfigurationStore> _configuration = new Lazy<ConfigurationStore>(_configurationInit);

        private static readonly Func<IMappingEngine> _mappingEngineInit = 
            () => new MappingEngine(_configuration.Value);

		private static Lazy<IMappingEngine> _mappingEngine = new Lazy<IMappingEngine>(_mappingEngineInit);

		public static bool AllowNullDestinationValues
		{
			get { return Configuration.AllowNullDestinationValues; }
			set { Configuration.AllowNullDestinationValues = value; }
		}

		public static TDestination Map<TSource, TDestination>(TSource source)
		{
			return Engine.Map<TSource, TDestination>(source);
		}

		public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
		{
			return Engine.Map(source, destination);
		}

		public static object Map(object source, Type sourceType, Type destinationType)
		{
			return Engine.Map(source, sourceType, destinationType);
		}

		public static object Map(object source, object destination, Type sourceType, Type destinationType)
		{
			return Engine.Map(source, destination, sourceType, destinationType);
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

		public static IMappingExpression CreateMap(Type sourceType, Type destinationType)
		{
			return Configuration.CreateMap(sourceType, destinationType);
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
			return ConfigurationProvider.FindTypeMapFor(null, sourceType, destinationType);
		}

		public static TypeMap FindTypeMapFor<TSource, TDestination>()
		{
			return ConfigurationProvider.FindTypeMapFor(null, typeof(TSource), typeof(TDestination));
		}

		public static TypeMap[] GetAllTypeMaps()
		{
			return ConfigurationProvider.GetAllTypeMaps();
		}

		public static void AssertConfigurationIsValid()
		{
			ConfigurationProvider.AssertConfigurationIsValid();
		}

		public static void AssertConfigurationIsValid(string profileName)
		{
			ConfigurationProvider.AssertConfigurationIsValid(profileName);
		}

		public static void Reset()
		{
			_configuration = new Lazy<ConfigurationStore>(_configurationInit);
			_mappingEngine = new Lazy<IMappingEngine>(_mappingEngineInit);
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