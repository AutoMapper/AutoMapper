using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper.Internal;
using AutoMapper.Mappers;

namespace AutoMapper
{
	public class Configuration : IConfiguration, IConfigurationExpression
	{
		private readonly IObjectMapper[] _mappers;
		internal const string DefaultProfileName = "";

		private readonly IList<TypeMap> _typeMaps = new List<TypeMap>();
		private readonly IDictionary<string, FormatterExpression> _formatters = new Dictionary<string, FormatterExpression>();

		public Configuration(IObjectMapper[] mappers)
		{
			_mappers = mappers;
		}

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
			TypeMap typeMap = CreateTypeMap(typeof (TSource), typeof (TDestination));
			return new MappingExpression<TSource, TDestination>(typeMap);
		}

		private TypeMap CreateTypeMap(Type source, Type destination)
		{
			var typeMapFactory = new TypeMapFactory(source, destination);
			TypeMap typeMap = typeMapFactory.CreateTypeMap();

			_typeMaps.Add(typeMap);
			return typeMap;
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
			if (typeMap != null)
				return typeMap;

			typeMap = _typeMaps.FirstOrDefault(x => x.SourceType == sourceType && x.GetDerivedTypeFor(sourceType) == destinationType);
			if (typeMap != null)
				return typeMap;

			foreach (var sourceInterface in sourceType.GetInterfaces())
			{
				typeMap = ((IConfiguration) this).FindTypeMapFor(sourceInterface, destinationType);
				if (typeMap != null)
				{
					var derivedTypeFor = typeMap.GetDerivedTypeFor(sourceType);
					if (derivedTypeFor != null)
					{
						CreateTypeMap(sourceType, derivedTypeFor);
						return ((IConfiguration) this).FindTypeMapFor(sourceType, derivedTypeFor);
					}
					return typeMap;
				}
			}

			if (sourceType.BaseType != null)
				return ((IConfiguration) this).FindTypeMapFor(sourceType.BaseType, destinationType);

			return typeMap;
		}

		TypeMap IConfiguration.FindTypeMapFor<TSource, TDestination>()
		{
			return ((IConfiguration) this).FindTypeMapFor(typeof (TSource), typeof (TDestination));
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
				where typeMap.CustomMapper == null
				let unmappedPropertyNames = typeMap.GetUnmappedPropertyNames()
				where unmappedPropertyNames.Length > 0
				select new {typeMap, unmappedPropertyNames};

			var firstBadTypeMap = badTypeMaps.FirstOrDefault();

			if (firstBadTypeMap != null)
			{
				throw new AutoMapperConfigurationException(firstBadTypeMap.typeMap, firstBadTypeMap.unmappedPropertyNames);
			}

			foreach (var typeMap in _typeMaps)
			{
				foreach (var propertyMap in typeMap.GetPropertyMaps())
				{
					var lastResolver = propertyMap.GetSourceValueResolvers().LastOrDefault(r => r is MemberAccessorBase);

					if (lastResolver != null)
					{
						var sourceType = ((MemberAccessorBase) lastResolver).MemberType;
						var destinationType = propertyMap.DestinationProperty.MemberType;
						var customTypeMap = ((IConfiguration) this).FindTypeMapFor(destinationType, sourceType);
						var context = new ResolutionContext(customTypeMap, null, sourceType, destinationType);

						IObjectMapper mapperToUse = GetMappers().Where(mapper => !(mapper is NewOrDefaultMapper)).FirstOrDefault(mapper => mapper.IsMatch(context));

						if (mapperToUse == null)
						{
							throw new AutoMapperConfigurationException();
						}
					}
				}
			}
		}

		public IObjectMapper[] GetMappers()
		{
			return _mappers;
		}

		private void SelfProfile(Type type)
		{
			var selfProfiler = (ISelfProfiler) Activator.CreateInstance(type, true);
			Profile profile = selfProfiler.GetProfile();

			AddProfile(profile);
		}

		private static IEnumerable<Type> GetSelfProfilers(Assembly assembly)
		{
			return from t in assembly.GetTypes()
			       where typeof (ISelfProfiler).IsAssignableFrom(t) && !t.IsAbstract
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