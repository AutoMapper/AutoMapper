using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper.Internal;
using AutoMapper.Mappers;

namespace AutoMapper
{
	public class Configuration : IConfigurationProvider, IConfiguration
	{
		private struct TypePair
		{
			public TypePair(Type sourceType, Type destinationType) : this()
			{
				SourceType = sourceType;
				DestinationType = destinationType;
			}

			private Type SourceType { get; set; }
			private Type DestinationType { get; set; }

			public override int GetHashCode()
			{
				return SourceType.GetHashCode() ^ DestinationType.GetHashCode();
			}
		}

		private readonly IEnumerable<IObjectMapper> _mappers;
		internal const string DefaultProfileName = "";

		private readonly IList<TypeMap> _typeMaps = new List<TypeMap>();
		private readonly IDictionary<TypePair, TypeMap> _typeMapCache = new Dictionary<TypePair, TypeMap>();
		private readonly IDictionary<string, FormatterExpression> _formatterProfiles = new Dictionary<string, FormatterExpression>();
		private Func<Type, IValueFormatter> _formatterCtor = type => (IValueFormatter)Activator.CreateInstance(type, true);
		private Func<Type, IValueResolver> _resolverCtor = type => (IValueResolver)Activator.CreateInstance(type, true);
		private Func<Type, object> _typeConverterCtor = type => Activator.CreateInstance(type, true);

		public Configuration(IEnumerable<IObjectMapper> mappers)
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

		public IMappingExpression CreateMap(Type sourceType, Type destinationType)
		{
			var typeMap = CreateTypeMap(sourceType, destinationType);

			return new MappingExpression(typeMap, _typeConverterCtor);
		}

		public TypeMap CreateTypeMap(Type source, Type destination)
		{
			var typeMapFactory = new TypeMapFactory(source, destination);
			TypeMap typeMap = typeMapFactory.CreateTypeMap();

			_typeMaps.Add(typeMap);
			_typeMapCache[new TypePair(source, destination)] = typeMap;
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

		public TypeMap[] GetAllTypeMaps()
		{
			return _typeMaps.ToArray();
		}

		public TypeMap FindTypeMapFor(Type sourceType, Type destinationType)
		{
			var typeMapPair = new TypePair(sourceType, destinationType);
			
			// Cache miss
			if (_typeMapCache.ContainsKey(typeMapPair))
				return _typeMapCache[typeMapPair];

			TypeMap typeMap = _typeMaps.FirstOrDefault(x => x.DestinationType == destinationType && x.SourceType == sourceType);
			if (typeMap == null)
			{
				typeMap = _typeMaps.FirstOrDefault(x => x.SourceType == sourceType && x.GetDerivedTypeFor(sourceType) == destinationType);

				if (typeMap == null)
				{
					foreach (var sourceInterface in sourceType.GetInterfaces())
					{
						typeMap = ((IConfigurationProvider) this).FindTypeMapFor(sourceInterface, destinationType);
						
						if (typeMap == null) continue;

						var derivedTypeFor = typeMap.GetDerivedTypeFor(sourceType);
						if (derivedTypeFor != null)
						{
							typeMap = CreateTypeMap(sourceType, derivedTypeFor);
						}
					}

					if ((sourceType.BaseType != null) && (typeMap == null))
						typeMap = ((IConfigurationProvider) this).FindTypeMapFor(sourceType.BaseType, destinationType);
				}
			}

			_typeMapCache[typeMapPair] = typeMap;

			return typeMap;
		}

		public TypeMap FindTypeMapFor<TSource, TDestination>()
		{
			return ((IConfigurationProvider) this).FindTypeMapFor(typeof (TSource), typeof (TDestination));
		}

		public IFormatterConfiguration GetProfileConfiguration(string profileName)
		{
			return GetProfile(profileName);
		}

		public void AssertConfigurationIsValid(TypeMap typeMap)
		{
			if (typeMap.GetUnmappedPropertyNames().Length > 0)
			{
				throw new AutoMapperConfigurationException(typeMap, typeMap.GetUnmappedPropertyNames());
			}
		    var typeMaps = new List<TypeMap> {typeMap};
			DryRunTypeMap(typeMaps, new ResolutionContext(typeMap, null, typeMap.SourceType, typeMap.DestinationType));
		}

		public void AssertConfigurationIsValid()
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

		    var typeMapsChecked = new List<TypeMap>();

			foreach (var typeMap in _typeMaps)
			{
				DryRunTypeMap(typeMapsChecked, new ResolutionContext(typeMap, null, typeMap.SourceType, typeMap.DestinationType));
			}
		}

	    public IObjectMapper[] GetMappers()
	    {
	        return _mappers.ToArray();
	    }

	    private void DryRunTypeMap(ICollection<TypeMap> typeMapsChecked, ResolutionContext context)
		{
            if (context.TypeMap != null)
            {
                typeMapsChecked.Add(context.TypeMap);
            }

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
							var memberTypeMap = ((IConfigurationProvider)this).FindTypeMapFor(sourceType, destinationType);

                            if (typeMapsChecked.Any(typeMap => Equals(typeMap, memberTypeMap)))
                                continue;
                            
                            var memberContext = context.CreateMemberContext(memberTypeMap, null, sourceType, propertyMap);

                            DryRunTypeMap(typeMapsChecked, memberContext);
						}
					}
				}
			} 
			else if (mapperToUse is ArrayMapper || mapperToUse is EnumerableMapper)
			{
				Type sourceElementType = TypeHelper.GetElementType(context.SourceType);
				Type destElementType = TypeHelper.GetElementType(context.DestinationType);
				TypeMap itemTypeMap = ((IConfigurationProvider) this).FindTypeMapFor(sourceElementType, destElementType);

                if (typeMapsChecked.Any(typeMap => Equals(typeMap, itemTypeMap)))
                    return;
                
                var memberContext = context.CreateElementContext(itemTypeMap, null, sourceElementType, destElementType, 0);
                
                DryRunTypeMap(typeMapsChecked, memberContext);
			}

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
			if (!_formatterProfiles.ContainsKey(profileName))
			{
				_formatterProfiles.Add(profileName, new FormatterExpression(_formatterCtor));
			}

			return _formatterProfiles[profileName];
		}
	}
}
