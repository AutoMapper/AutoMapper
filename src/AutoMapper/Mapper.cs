using System;
using AutoMapper.Mappers;

namespace AutoMapper
{
	public static class Mapper
	{
		private static Configuration _configuration;
		private static IMappingEngine _mappingEngine;

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

		public static TDestination DynamicMap<TDestination>(object source)
		{
			return Engine.DynamicMap<TDestination>(source);
		}

		public static object DynamicMap(object source, Type sourceType, Type destinationType)
		{
			return Engine.DynamicMap(source, sourceType, destinationType);
		}

		public static void Initialize(Action<IConfigurationExpression> action)
		{
			Reset();

			action(ConfigurationExpression);
		}

		public static IFormatterCtorExpression<TValueFormatter> AddFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			return ConfigurationExpression.AddFormatter<TValueFormatter>();
		}

		public static IFormatterCtorExpression AddFormatter(Type valueFormatterType)
		{
			return ConfigurationExpression.AddFormatter(valueFormatterType);
		}

		public static void AddFormatter(IValueFormatter formatter)
		{
			ConfigurationExpression.AddFormatter(formatter);
		}

		public static void AddFormatExpression(Func<ResolutionContext, string> formatExpression)
		{
			ConfigurationExpression.AddFormatExpression(formatExpression);
		}

		public static void SkipFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			ConfigurationExpression.SkipFormatter<TValueFormatter>();
		}

		public static IFormatterExpression ForSourceType<TSource>()
		{
			return ConfigurationExpression.ForSourceType<TSource>();
		}

		public static IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
		{
			return ConfigurationExpression.CreateMap<TSource, TDestination>();
		}

		public static IProfileExpression CreateProfile(string profileName)
		{
			return ConfigurationExpression.CreateProfile(profileName);
		}

		public static void CreateProfile(string profileName, Action<IProfileExpression> profileConfiguration)
		{
			ConfigurationExpression.CreateProfile(profileName, profileConfiguration);
		}

		public static void AddProfile(Profile profile)
		{
			ConfigurationExpression.AddProfile(profile);
		}

		public static void AddProfile<TProfile>() where TProfile : Profile, new()
		{
			ConfigurationExpression.AddProfile<TProfile>();
		}

		public static TypeMap FindTypeMapFor(Type sourceType, Type destinationType)
		{
			return Configuration.FindTypeMapFor(sourceType, destinationType);
		}

		public static TypeMap FindTypeMapFor<TSource, TDestination>()
		{
			return Configuration.FindTypeMapFor<TSource, TDestination>();
		}

		public static TypeMap[] GetAllTypeMaps()
		{
			return Configuration.GetAllTypeMaps();
		}

		public static void AssertConfigurationIsValid()
		{
			Configuration.AssertConfigurationIsValid();
		}

		public static void Reset()
		{
			lock (typeof (IConfiguration))
				lock (typeof (IMappingEngine))
				{
					_configuration = null;
					_mappingEngine = null;
				}
		}

		public static IMappingEngine Engine
		{
			get
			{
				if (_mappingEngine == null)
				{
					lock (typeof(IMappingEngine))
					{
						if (_mappingEngine == null)
						{
							_mappingEngine = new MappingEngine(Configuration);
						}
					}
				}

				return _mappingEngine;
			}
		}

		private static IConfiguration Configuration
		{
			get
			{
				if (_configuration == null)
				{
					lock (typeof (Configuration))
					{
						if (_configuration == null)
						{
							_configuration = new Configuration(BuildMappers());
						}
					}
				}

				return _configuration;
			}
		}

		private static IConfigurationExpression ConfigurationExpression
		{
			get { return (IConfigurationExpression) Configuration; }
		}

		private static IObjectMapper[] BuildMappers()
		{
			return new IObjectMapper[]
				{
					new CustomTypeMapMapper(),
					new TypeMapMapper(),
					new NewOrDefaultMapper(),
					new StringMapper(),
					new EnumMapper(),
					new AssignableMapper(),
					new ArrayMapper(),
					new DictionaryMapper(),
					new EnumerableMapper(),
					new NullableMapper(),
				};
		}

		public static void AddResolver<TSourceMember, TDestinationMember>(IValueResolver resolver)
		{
//			return ConfigurationExpression.AddResolver<TSourceMember, TDestinationMember>();
		}
	}
}