using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

		public void SelfConfigure(Assembly assembly)
		{
			IEnumerable<Type> selfProfiles = GetSelfProfilers(assembly);

			selfProfiles.ForEach(SelfProfile);
		}

		public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
		{
			Type modelType = typeof(TSource);
			Type destinationType = typeof(TDestination);

			var typeMapFactory = new TypeMapFactory(modelType, destinationType);
			TypeMap typeMap = typeMapFactory.CreateTypeMap();

			_typeMaps.Add(typeMap);

			return new MappingExpression<TSource, TDestination>(typeMap);
		}

		public IFormatterCtorExpression<TValueFormatter> AddFormatter<TValueFormatter>() where TValueFormatter : IValueFormatter
		{
			return GetProfile(DefaultProfileName).AddFormatter<TValueFormatter>();
		}

		public IFormatterCtorExpression AddFormatter(Type valueFormatterType)
		{
			return GetProfile(DefaultProfileName).AddFormatter(valueFormatterType);
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
			TypeMap typeMap = _typeMaps.FirstOrDefault(x => x.DestinationType == destinationType && x.SourceType == sourceType);

			if ((typeMap == null) && sourceType.BaseType != null)
				return ((IConfiguration)this).FindTypeMapFor(sourceType.BaseType, destinationType);

			return typeMap;
		}

		TypeMap IConfiguration.FindTypeMapFor<TSource, TDestination>()
		{
			return ((IConfiguration)this).FindTypeMapFor(typeof(TSource), typeof(TDestination));
		}

		IValueFormatter IConfiguration.GetValueFormatter()
		{
			return new ValueFormatter(GetProfile(DefaultProfileName));
		}

		IValueFormatter IConfiguration.GetValueFormatter(string profileName)
		{
			return new ValueFormatter(GetProfile(profileName));
		}

		void IConfiguration.AssertConfigurationIsValid()
		{
			var badTypeMaps =
				from typeMap in _typeMaps
				let unmappedPropertyNames = typeMap.GetUnmappedPropertyNames()
				where unmappedPropertyNames.Length > 0
				select new {typeMap, unmappedPropertyNames};

			var firstBadTypeMap = badTypeMaps.FirstOrDefault();

			if (firstBadTypeMap != null)
			{
				throw new AutoMapperConfigurationException(firstBadTypeMap.typeMap, firstBadTypeMap.unmappedPropertyNames);
			}
		}

		private void SelfProfile(Type type)
		{
			var selfProfiler = (ISelfProfiler)Activator.CreateInstance(type, true);
			Profile profile = selfProfiler.GetProfile();

			AddProfile(profile);
		}

		private static IEnumerable<Type> GetSelfProfilers(Assembly assembly)
		{
			return from t in assembly.GetTypes()
				   where typeof(ISelfProfiler).IsAssignableFrom(t) && !t.IsAbstract
				   select t;
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
