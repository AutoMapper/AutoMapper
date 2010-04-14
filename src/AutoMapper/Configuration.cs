using System;
using System.Collections.Generic;

#if !SILVERLIGHT
using System.Data;
#endif

using System.Linq;
using System.Reflection;
using AutoMapper.Internal;
using AutoMapper.Mappers;

namespace AutoMapper
{
	public class Configuration : IConfigurationProvider, IConfiguration
	{
	    private readonly ITypeMapFactory _typeMapFactory;
	    private readonly IEnumerable<IObjectMapper> _mappers;
		internal const string DefaultProfileName = "";

		private readonly IList<TypeMap> _typeMaps = new List<TypeMap>();
		private readonly IDictionary<TypePair, TypeMap> _typeMapCache = new Dictionary<TypePair, TypeMap>();
		private readonly IDictionary<string, FormatterExpression> _formatterProfiles = new Dictionary<string, FormatterExpression>();
		private Func<Type, object> _serviceCtor = ObjectCreator.CreateObject;

		public Configuration(ITypeMapFactory typeMapFactory, IEnumerable<IObjectMapper> mappers)
		{
		    _typeMapFactory = typeMapFactory;
		    _mappers = mappers;
		}

		public event EventHandler<TypeMapCreatedEventArgs> TypeMapCreated;

		public bool AllowNullDestinationValues
		{
			get { return GetProfile(DefaultProfileName).AllowNullDestinationValues; }
			set { GetProfile(DefaultProfileName).AllowNullDestinationValues = value; }
		}

		public INamingConvention SourceMemberNamingConvention
		{
			get { return GetProfile(DefaultProfileName).SourceMemberNamingConvention; }
			set { GetProfile(DefaultProfileName).SourceMemberNamingConvention = value; }
		}

		public INamingConvention DestinationMemberNamingConvention
		{
			get { return GetProfile(DefaultProfileName).DestinationMemberNamingConvention; }
			set { GetProfile(DefaultProfileName).DestinationMemberNamingConvention = value; }
		}

		public Func<string, string> SourceMemberNameTransformer
		{
			get { return GetProfile(DefaultProfileName).SourceMemberNameTransformer; }
			set { GetProfile(DefaultProfileName).SourceMemberNameTransformer = value; }
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

			selfProfiles.Each(SelfProfile);
		}

		public void ConstructServicesUsing(Func<Type, object> constructor)
		{
			_serviceCtor = constructor;
		}

		public void Seal()
		{
			_typeMaps.Each(typeMap => typeMap.Seal());
		}

		public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
		{
		    return CreateMap<TSource, TDestination>(DefaultProfileName);
		}


        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(string profileName)
        {
            TypeMap typeMap = CreateTypeMap(typeof(TSource), typeof(TDestination), profileName);
            return new MappingExpression<TSource, TDestination>(typeMap, _serviceCtor);
        }

		public IMappingExpression CreateMap(Type sourceType, Type destinationType)
		{
		    return CreateMap(sourceType, destinationType, DefaultProfileName);
		}

		public IMappingExpression CreateMap(Type sourceType, Type destinationType, string profileName)
		{
			var typeMap = CreateTypeMap(sourceType, destinationType, profileName);

			return new MappingExpression(typeMap, _serviceCtor);
		}

		public void RecognizePrefixes(params string[] prefixes)
		{
			GetProfile(DefaultProfileName).RecognizePrefixes(prefixes);
		}

		public void RecognizePostfixes(params string[] postfixes)
		{
			GetProfile(DefaultProfileName).RecognizePostfixes(postfixes);
		}

		public void RecognizeAlias(string original, string alias)
		{
			GetProfile(DefaultProfileName).RecognizeAlias(original, alias);
		}

		public TypeMap CreateTypeMap(Type source, Type destination)
		{
		    return CreateTypeMap(source, destination, DefaultProfileName);
		}

		public TypeMap CreateTypeMap(Type source, Type destination, string profileName)
		{
			TypeMap typeMap = FindExplicitlyDefinedTypeMap(source, destination);
				
			if (typeMap == null)
			{
			    var profileConfiguration = GetProfile(profileName);

				typeMap = _typeMapFactory.CreateTypeMap(source, destination, profileConfiguration);

                typeMap.Profile = profileName;

				_typeMaps.Add(typeMap);
				_typeMapCache[new TypePair(source, destination)] = typeMap;

				OnTypeMapCreated(typeMap);
			}

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

		public TypeMap FindTypeMapFor(object source, Type sourceType, Type destinationType)
		{
			var typeMapPair = new TypePair(sourceType, destinationType);
			
			TypeMap typeMap;

            if (!_typeMapCache.TryGetValue(typeMapPair, out typeMap))
            {
                lock (_typeMapCache)
                {
                    if (!_typeMapCache.TryGetValue(typeMapPair, out typeMap))
                    {
                        // Cache miss
                        typeMap = FindTypeMap(source, sourceType, destinationType, DefaultProfileName);

                        _typeMapCache[typeMapPair] = typeMap;
                    }
                }
            }

            // Check for runtime derived types
		    var shouldCheckDerivedType = (typeMap != null) && (typeMap.HasDerivedTypesToInclude()) && (source != null) && (source.GetType() != sourceType);
		    
            if (shouldCheckDerivedType)
            {
                var potentialSourceType = source.GetType();
                var potentialDestType = typeMap.GetDerivedTypeFor(potentialSourceType);

                var targetSourceType = potentialDestType != destinationType ? potentialSourceType : typeMap.SourceType;
                var targetDestinationType = potentialDestType;
                typeMap = FindTypeMap(source, targetSourceType, targetDestinationType, DefaultProfileName);
            }

		    return typeMap;
		}

		public TypeMap FindTypeMapFor(ResolutionResult resolutionResult, Type destinationType)
		{
			return FindTypeMapFor(resolutionResult.Value, resolutionResult.Type, destinationType) ??
			       FindTypeMapFor(resolutionResult.Value, resolutionResult.MemberType, destinationType);
		}

		public IFormatterConfiguration GetProfileConfiguration(string profileName)
		{
			return GetProfile(profileName);
		}

		public void AssertConfigurationIsValid(TypeMap typeMap)
		{
			AssertConfigurationIsValid(Enumerable.Repeat(typeMap, 1));
		}

		public void AssertConfigurationIsValid(string profileName)
		{
			AssertConfigurationIsValid(_typeMaps.Where(typeMap => typeMap.Profile == profileName));
		}

		public void AssertConfigurationIsValid()
		{
			AssertConfigurationIsValid(_typeMaps);
		}

	    public IObjectMapper[] GetMappers()
	    {
	        return _mappers.ToArray();
	    }

		private void AssertConfigurationIsValid(IEnumerable<TypeMap> typeMaps)
		{
			var badTypeMaps =
				from typeMap in typeMaps
				where ShouldCheckMap(typeMap)
				let unmappedPropertyNames = typeMap.GetUnmappedPropertyNames()
				where unmappedPropertyNames.Length > 0
				select new { typeMap, unmappedPropertyNames };

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

	    private static bool ShouldCheckMap(TypeMap typeMap)
	    {
#if !SILVERLIGHT
	        return typeMap.CustomMapper == null && !typeof(IDataRecord).IsAssignableFrom(typeMap.SourceType);
#else
            return typeMap.CustomMapper == null;
#endif
	    }


	    private TypeMap FindTypeMap(object source, Type sourceType, Type destinationType, string profileName)
        {
            TypeMap typeMap = FindExplicitlyDefinedTypeMap(sourceType, destinationType);

            if (typeMap == null && destinationType.IsNullableType())
            {
                typeMap = FindExplicitlyDefinedTypeMap(sourceType, destinationType.GetTypeOfNullable());
            }

            if (typeMap == null)
            {
                typeMap = _typeMaps.FirstOrDefault(x => x.SourceType == sourceType && x.GetDerivedTypeFor(sourceType) == destinationType);

                if (typeMap == null)
                {
                    foreach (var sourceInterface in sourceType.GetInterfaces())
                    {
                        typeMap = ((IConfigurationProvider)this).FindTypeMapFor(source, sourceInterface, destinationType);

                        if (typeMap == null) continue;

                        var derivedTypeFor = typeMap.GetDerivedTypeFor(sourceType);
                        if (derivedTypeFor != destinationType)
                        {
                            typeMap = CreateTypeMap(sourceType, derivedTypeFor, profileName);
                        }

                        break;
                    }

                    if ((sourceType.BaseType != null) && (typeMap == null))
                        typeMap = ((IConfigurationProvider)this).FindTypeMapFor(source, sourceType.BaseType, destinationType);
                }
            }
            return typeMap;
        }

		private TypeMap FindExplicitlyDefinedTypeMap(Type sourceType, Type destinationType)
		{
			return _typeMaps.FirstOrDefault(x => x.DestinationType == destinationType && x.SourceType == sourceType);
		}

		private void DryRunTypeMap(ICollection<TypeMap> typeMapsChecked, ResolutionContext context)
		{
            if (context.TypeMap != null)
            {
                typeMapsChecked.Add(context.TypeMap);
            }

			var mapperToUse = GetMappers().FirstOrDefault(mapper => mapper.IsMatch(context));

            if (mapperToUse == null && context.SourceType.IsNullableType())
            {
                var nullableContext = context.CreateValueContext(null, Nullable.GetUnderlyingType(context.SourceType));

                mapperToUse = GetMappers().FirstOrDefault(mapper => mapper.IsMatch(nullableContext));
            }

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
						var lastResolver = propertyMap.GetSourceValueResolvers().OfType<IMemberResolver>().LastOrDefault();

						if (lastResolver != null)
						{
							var sourceType = lastResolver.MemberType;
							var destinationType = propertyMap.DestinationProperty.MemberType;
							var memberTypeMap = ((IConfigurationProvider)this).FindTypeMapFor(null, sourceType, destinationType);

                            if (typeMapsChecked.Any(typeMap => Equals(typeMap, memberTypeMap)))
                                continue;
                            
                            var memberContext = context.CreateMemberContext(memberTypeMap, null, null, sourceType, propertyMap);

                            DryRunTypeMap(typeMapsChecked, memberContext);
						}
					}
				}
			} 
			else if (mapperToUse is ArrayMapper || mapperToUse is EnumerableMapper || mapperToUse is CollectionMapper)
			{
				Type sourceElementType = TypeHelper.GetElementType(context.SourceType);
				Type destElementType = TypeHelper.GetElementType(context.DestinationType);
				TypeMap itemTypeMap = ((IConfigurationProvider) this).FindTypeMapFor(null, sourceElementType, destElementType);

                if (typeMapsChecked.Any(typeMap => Equals(typeMap, itemTypeMap)))
                    return;
                
                var memberContext = context.CreateElementContext(itemTypeMap, null, sourceElementType, destElementType, 0);
                
                DryRunTypeMap(typeMapsChecked, memberContext);
			}

		}

	    private void SelfProfile(Type type)
		{
			var selfProfiler = (ISelfProfiler) ObjectCreator.CreateObject(type);
			Profile profile = selfProfiler.GetProfile();

			AddProfile(profile);
		}

		protected void OnTypeMapCreated(TypeMap typeMap)
		{
			var typeMapCreated = TypeMapCreated;
			if (typeMapCreated != null)
				typeMapCreated(this, new TypeMapCreatedEventArgs(typeMap));
		}

		private static IEnumerable<Type> GetSelfProfilers(Assembly assembly)
		{
			return from t in assembly.GetTypes()
			       where typeof (ISelfProfiler).IsAssignableFrom(t) && !t.IsAbstract
			       select t;
		}

		internal FormatterExpression GetProfile(string profileName)
		{
			FormatterExpression expr;

            if (!_formatterProfiles.TryGetValue(profileName, out expr))
            {
                lock(_formatterProfiles)
                {
                    if (!_formatterProfiles.TryGetValue(profileName, out expr))
                    {
						expr = new FormatterExpression(t => (IValueFormatter)_serviceCtor(t));

                        _formatterProfiles.Add(profileName, expr);
                    }
                }
            }
			
            return expr;
		}
	}
}
