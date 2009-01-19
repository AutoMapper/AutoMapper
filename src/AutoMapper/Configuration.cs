using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoMapper
{
	public class Configuration : IConfiguration, IConfigurationExpression
	{
		internal const string DefaultProfileName = "";

		private readonly IList<TypeMap> _typeMaps = new List<TypeMap>();
		private readonly IDictionary<string, FormatterExpression> _formatters = new Dictionary<string, FormatterExpression>();

		public IProfileExpression CreateProfile(string profileName)
		{
			var profileExpression = new Profile(profileName);

			profileExpression.Initialize(this);

			return profileExpression;
		}

		public void CreateProfile(string profileName, Action<IProfileExpression> initializationExpression)
		{
			var profileExpression = new Profile(profileName);

			profileExpression.Initialize(this);

			initializationExpression(profileExpression);
		}

		public void AddProfile(Profile profile)
		{
			profile.Initialize(this);

			profile.Configure();
		}

		public void AddProfile<TProfile>() where TProfile : Profile, new()
		{
			AddProfile(new TProfile());
		}

		public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
		{
			Type modelType = typeof(TSource);
			Type dtoType = typeof(TDestination);

			var typeMapFactory = new TypeMapFactory(modelType, dtoType);
			TypeMap typeMap = typeMapFactory.CreateTypeMap();

			_typeMaps.Add(typeMap);

			return new MappingExpression<TSource, TDestination>(typeMap);
		}

		public void AddFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			GetProfile(DefaultProfileName).AddFormatter<TValueFormatter>();
		}

		public void AddFormatter(Type valueFormatterType)
		{
			GetProfile(DefaultProfileName).AddFormatter(valueFormatterType);
		}

		public void AddFormatter(IValueFormatter formatter)
		{
			GetProfile(DefaultProfileName).AddFormatter(formatter);
		}

		public void AddFormatExpression(Func<ResolutionContext, string> formatExpression)
		{
			GetProfile(DefaultProfileName).AddFormatExpression(formatExpression);
		}

		public void SkipFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			GetProfile(DefaultProfileName).SkipFormatter<TValueFormatter>();
		}

		public IFormatterExpression ForSourceType<TSource>()
		{
			return GetProfile(DefaultProfileName).ForSourceType<TSource>();
		}

		TypeMap[] IConfiguration.GetAllTypeMaps()
		{
			return _typeMaps.ToArray();
		}

		TypeMap IConfiguration.FindTypeMapFor(Type sourceType, Type destinationType)
		{
			var typeMap = _typeMaps.FirstOrDefault(x => x.DestinationType == destinationType && x.SourceType == sourceType);

			if ((typeMap == null) && sourceType.BaseType != null)
				return  ((IConfiguration) this).FindTypeMapFor(sourceType.BaseType, destinationType);

			return typeMap;
		}

		TypeMap IConfiguration.FindTypeMapFor<TSource, TDestination>()
		{
			return ((IConfiguration) this).FindTypeMapFor(typeof(TSource), typeof(TDestination));
		}

		IValueFormatter IConfiguration.GetValueFormatter()
		{
			return new ValueFormatter(GetProfile(DefaultProfileName));
		}

		IValueFormatter IConfiguration.GetValueFormatter(string profileName)
		{
			return new ValueFormatter(GetProfile(profileName));
		}

		internal FormatterExpression GetProfile(string profileName)
		{
			if (!_formatters.ContainsKey(profileName))
			{
				_formatters.Add(profileName, new FormatterExpression());
			}

			return _formatters[profileName];
		}
	}
}