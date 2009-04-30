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
		private Func<Type, IValueFormatter> _formatterCtor = type => (IValueFormatter)Activator.CreateInstance(type, true);
		private Func<Type, IValueResolver> _resolverCtor = type => (IValueResolver)Activator.CreateInstance(type, true);
		private Func<Type, object> _typeConverterCtor = type => Activator.CreateInstance(type, true);

		public Configuration(IObjectMapper[] mappers)
		{
			_mappers = mappers;
		}

		public bool AllowNullDestinationValues
		{
			get { return GetProfile(DefaultProfileName).AllowNullDestinationValues; }
			set { GetProfile(DefaultProfileName).AllowNullDestinationValues = value; }
		}

		bool IProfileConfiguration.MapNullSourceValuesAsNull
		{
			get { return AllowNullDestinationValues; }
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

		public void ConstructFormattersUsing(Func<Type, IValueFormatter> constructor)
		{
			_formatterCtor = constructor;
		}

		public void ConstructResolversUsing(Func<Type, IValueResolver> constructor)
		{
			_resolverCtor = constructor;
		}

		public void ConstructTypeConvertersUsing(Func<Type, object> constructor)
		{
			_typeConverterCtor = constructor;
		}

		public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
		{
			TypeMap typeMap = CreateTypeMap(typeof (TSource), typeof (TDestination));
			return new MappingExpression<TSource, TDestination>(typeMap, _formatterCtor, _resolverCtor, _typeConverterCtor);
		}

		public TypeMap CreateTypeMap(Type source, Type destination)
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

		IFormatterConfiguration IConfiguration.GetProfileConfiguration(string profileName)
		{
			return GetProfile(profileName);
		}

		void IConfiguration.AssertConfigurationIsValid(TypeMap typeMap)
		{
			if (typeMap.GetUnmappedPropertyNames().Length > 0)
			{
				throw new AutoMapperConfigurationException(typeMap, typeMap.GetUnmappedPropertyNames());
			}
			DryRunTypeMap(new ResolutionContext(typeMap, null, typeMap.SourceType, typeMap.DestinationType));
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
				DryRunTypeMap(new ResolutionContext(typeMap, null, typeMap.SourceType, typeMap.DestinationType));
			}
		}

		private void DryRunTypeMap(ResolutionContext context)
		{
			var mapperToUse = GetMappers().Where(mapper => !(mapper is NewOrDefaultMapper)).FirstOrDefault(mapper => mapper.IsMatch(context));

			if (mapperToUse == null)
			{
				throw new AutoMapperConfigurationException(context);
			}

			if (mapperToUse is TypeMapMapper)
			{
				foreach (var propertyMap in context.TypeMap.GetPropertyMaps())
				{
					if (!propertyMap.IsIgnored())
					{
						var lastResolver = propertyMap.GetSourceValueResolvers().LastOrDefault(r => r is MemberAccessorBase);

						if (lastResolver != null)
						{
							var sourceType = ((MemberAccessorBase)lastResolver).MemberType;
							var destinationType = propertyMap.DestinationProperty.MemberType;
							var memberTypeMap = ((IConfiguration)this).FindTypeMapFor(sourceType, destinationType);

							var memberContext = context.CreateMemberContext(memberTypeMap, null, sourceType, propertyMap);

							DryRunTypeMap(memberContext);
						}
					}
				}
			} 
			else if (mapperToUse is ArrayMapper || mapperToUse is EnumerableMapper)
			{
				Type sourceElementType = TypeHelper.GetElementType(context.SourceType);
				Type destElementType = TypeHelper.GetElementType(context.DestinationType);
				TypeMap itemTypeMap = ((IConfiguration) this).FindTypeMapFor(sourceElementType, destElementType);
				var memberContext = context.CreateElementContext(itemTypeMap, null, sourceElementType, destElementType, 0);

				DryRunTypeMap(memberContext);
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
				_formatters.Add(profileName, new FormatterExpression(_formatterCtor));
			}

			return _formatters[profileName];
		}
	}
}