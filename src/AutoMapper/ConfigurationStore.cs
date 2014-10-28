using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper.Impl;
using AutoMapper.Mappers;

namespace AutoMapper
{
    using Internal;

	public class ConfigurationStore : IConfigurationProvider, IConfiguration
	{
	    private static readonly IDictionaryFactory DictionaryFactory = PlatformAdapter.Resolve<IDictionaryFactory>();
	    private readonly ITypeMapFactory _typeMapFactory;
	    private readonly IEnumerable<IObjectMapper> _mappers;
		internal const string DefaultProfileName = "";
		
		private readonly ThreadSafeList<TypeMap> _typeMaps = new ThreadSafeList<TypeMap>();

        private readonly IDictionary<TypePair, TypeMap> _typeMapCache = DictionaryFactory.CreateDictionary<TypePair, TypeMap>();
        private readonly IDictionary<string, FormatterExpression> _formatterProfiles = DictionaryFactory.CreateDictionary<string, FormatterExpression>();
		private Func<Type, object> _serviceCtor = ObjectCreator.CreateObject;

	    private readonly List<string> _globalIgnore;

	    public ConfigurationStore(ITypeMapFactory typeMapFactory, IEnumerable<IObjectMapper> mappers)
		{
		    _typeMapFactory = typeMapFactory;
		    _mappers = mappers;
            _globalIgnore = new List<string>();
		}

	    public event EventHandler<TypeMapCreatedEventArgs> TypeMapCreated;

	    public Func<Type, object> ServiceCtor
	    {
	        get { return _serviceCtor; }
	    }

	    public bool AllowNullDestinationValues
		{
			get { return GetProfile(DefaultProfileName).AllowNullDestinationValues; }
			set { GetProfile(DefaultProfileName).AllowNullDestinationValues = value; }
		}

	    public bool AllowNullCollections
		{
			get { return GetProfile(DefaultProfileName).AllowNullCollections; }
			set { GetProfile(DefaultProfileName).AllowNullCollections = value; }
		}

	    public void IncludeSourceExtensionMethods(Assembly assembly)
	    {
	        GetProfile(DefaultProfileName).IncludeSourceExtensionMethods(assembly);
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

		public BindingFlags BindingFlags
		{
			get { return GetProfile(DefaultProfileName).BindingFlags; }
			set { GetProfile(DefaultProfileName).BindingFlags = value; }
		}

	    public IEnumerable<string> Prefixes
	    {
            get { return GetProfile(DefaultProfileName).Prefixes; }
	    }

	    public IEnumerable<string> Postfixes
	    {
            get { return GetProfile(DefaultProfileName).Postfixes; }
	    }

	    public IEnumerable<string> DestinationPrefixes
	    {
            get { return GetProfile(DefaultProfileName).DestinationPrefixes; }
	    }

	    public IEnumerable<string> DestinationPostfixes
	    {
            get { return GetProfile(DefaultProfileName).DestinationPostfixes; }
	    }

	    public IEnumerable<MemberNameReplacer> MemberNameReplacers
        {
            get { return GetProfile(DefaultProfileName).MemberNameReplacers; }
        }

	    public IEnumerable<AliasedMember> Aliases
	    {
	        get { return GetProfile(DefaultProfileName).Aliases; }
	    }

	    public bool ConstructorMappingEnabled
	    {
	        get { return GetProfile(DefaultProfileName).ConstructorMappingEnabled; }
	    }

	    public bool DataReaderMapperYieldReturnEnabled
	    {
            get { return GetProfile(DefaultProfileName).DataReaderMapperYieldReturnEnabled; }
	    }

	    public IEnumerable<MethodInfo> SourceExtensionMethods
	    {
	        get
	        {
	            return GetProfile(DefaultProfileName).SourceExtensionMethods;
	        }
	    }

		bool IProfileConfiguration.MapNullSourceValuesAsNull
		{
			get { return AllowNullDestinationValues; }
		}

		bool IProfileConfiguration.MapNullSourceCollectionsAsNull
		{
			get { return AllowNullCollections; }
		}

		public IProfileExpression CreateProfile(string profileName)
		{
			var profileExpression = new Profile(profileName);

			profileExpression.Initialize(this);

			return profileExpression;
		}

		public void CreateProfile(string profileName, Action<IProfileExpression> profileConfiguration)
		{
			var profileExpression = new Profile(profileName);

			profileExpression.Initialize(this);

			profileConfiguration(profileExpression);
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

		public void ConstructServicesUsing(Func<Type, object> constructor)
		{
			_serviceCtor = constructor;
		}

	    public void DisableConstructorMapping()
	    {
	        GetProfile(DefaultProfileName).ConstructorMappingEnabled = false;
	    }

	    public void EnableYieldReturnForDataReaderMapper()
	    {
	        GetProfile(DefaultProfileName).DataReaderMapperYieldReturnEnabled = true;
	    }

	    public void Seal()
		{
			_typeMaps.Each(typeMap => typeMap.Seal());
		}

		public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
		{
		    return CreateMap<TSource, TDestination>(DefaultProfileName);
		}

		public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList)
		{
		    return CreateMap<TSource, TDestination>(DefaultProfileName, memberList);
		}

        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(string profileName)
        {
            return CreateMap<TSource, TDestination>(profileName, MemberList.Destination);
        }

        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(string profileName, MemberList memberList)
        {
            TypeMap typeMap = CreateTypeMap(typeof(TSource), typeof(TDestination), profileName, memberList);

            return CreateMappingExpression<TSource, TDestination>(typeMap);
        }

		public IMappingExpression CreateMap(Type sourceType, Type destinationType)
		{
		    return CreateMap(sourceType, destinationType, MemberList.Destination);
		}

	    public IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList)
	    {
	        return CreateMap(sourceType, destinationType, memberList, DefaultProfileName);
	    }

	    public IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList, string profileName)
		{
			var typeMap = CreateTypeMap(sourceType, destinationType, profileName, memberList);

			return CreateMappingExpression(typeMap, destinationType);
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

        public void ReplaceMemberName(string original, string newValue)
        {
            GetProfile(DefaultProfileName).ReplaceMemberName(original, newValue);
        }

        public void RecognizeDestinationPrefixes(params string[] prefixes)
        {
            GetProfile(DefaultProfileName).RecognizeDestinationPrefixes(prefixes);
        }

        public void RecognizeDestinationPostfixes(params string[] postfixes)
        {
            GetProfile(DefaultProfileName).RecognizeDestinationPostfixes(postfixes);
        }

		public TypeMap CreateTypeMap(Type source, Type destination)
		{
		    return CreateTypeMap(source, destination, DefaultProfileName, MemberList.Destination);
		}

		public TypeMap CreateTypeMap(Type source, Type destination, string profileName, MemberList memberList)
		{
			TypeMap typeMap = FindExplicitlyDefinedTypeMap(source, destination);
				
			if (typeMap == null)
			{
			    var profileConfiguration = GetProfile(profileName);

				typeMap = _typeMapFactory.CreateTypeMap(source, destination, profileConfiguration, memberList);

                typeMap.Profile = profileName;
			    typeMap.IgnorePropertiesStartingWith = _globalIgnore;

			    IncludeBaseMappings(source, destination, typeMap);

				_typeMaps.Add(typeMap);

			    var typePair = new TypePair(source, destination);
			    _typeMapCache.AddOrUpdate(typePair, typeMap, (tp, tm) => typeMap);

				OnTypeMapCreated(typeMap);
			}

			return typeMap;
		}

        private void IncludeBaseMappings(Type source, Type destination, TypeMap typeMap)
        {
            foreach (var inheritedTypeMap in _typeMaps.Where(t => t.TypeHasBeenIncluded(source, destination)))
            {
                typeMap.ApplyInheritedMap(inheritedTypeMap);
            }
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
			return FindTypeMapFor( null, null, sourceType, destinationType ) ;
		}

		public TypeMap FindTypeMapFor(object source, object destination, Type sourceType, Type destinationType)
		{
			var typeMapPair = new TypePair(sourceType, destinationType);
			
			TypeMap typeMap;

            if (!_typeMapCache.TryGetValue(typeMapPair, out typeMap))
            {
                // Cache miss
                typeMap = FindTypeMap(source, destination, sourceType, destinationType, DefaultProfileName);

                //We don't want to inherit base mappings which may be ambiguous or too specific resulting in cast exceptions
                if (source == null || source.GetType() == sourceType)
                    _typeMapCache[typeMapPair] = typeMap;
            }
            // Due to the inheritance we can have derrived mapping cached which is not valid for this source object
            else if (source != null && typeMap != null && !typeMap.SourceType.IsAssignableFrom(source.GetType()))
            {
                typeMap = FindTypeMapFor(source, destination, source.GetType(), destinationType);
            }

            if (typeMap == null && destination != null && destination.GetType() != destinationType)
            {
                typeMap = FindTypeMapFor(source, destination, sourceType, destination.GetType());
            }
            if (typeMap != null && typeMap.DestinationTypeOverride != null)
            {
                return FindTypeMapFor(source, destination, sourceType, typeMap.DestinationTypeOverride);
            }
            // Check for runtime derived types
		    var shouldCheckDerivedType = (typeMap != null) && (typeMap.HasDerivedTypesToInclude()) && (source != null) && (source.GetType() != sourceType);
		    
            if (shouldCheckDerivedType)
            {
                var potentialSourceType = source.GetType();
                //Try and get the most specific type map possible
                var potentialTypes = _typeMaps
                    .Where(t => ((destination == null && destinationType.IsAssignableFrom(t.DestinationType))
                                 || (destination != null && t.DestinationType.IsInstanceOfType(destination))) &&
                                t.SourceType.IsInstanceOfType(source));

                var potentialDestTypeMap =
                    potentialTypes
                        .OrderByDescending(t => GetInheritanceDepth(t.DestinationType))
                        .FirstOrDefault();
                var ambiguousPotentialTypes = potentialTypes
                    .Where(t => t.DestinationType == potentialDestTypeMap.DestinationType)
                    .ToList();

                if (ambiguousPotentialTypes.Count > 1)
                {
                    potentialDestTypeMap = ambiguousPotentialTypes
                        .OrderByDescending(t => GetInheritanceDepth(t.SourceType))
                        .FirstOrDefault();
                }

                if (potentialDestTypeMap == typeMap)
                    return typeMap;

                var targetDestinationType = potentialDestTypeMap.DestinationType;
                var potentialTypeMap = FindExplicitlyDefinedTypeMap(potentialSourceType, targetDestinationType);
                if (potentialTypeMap == null)
                {
                    var targetSourceType = targetDestinationType != destinationType ? potentialSourceType : typeMap.SourceType;
                    typeMap = FindTypeMap(source, destination, targetSourceType, targetDestinationType, DefaultProfileName);
                }
                else
                    typeMap = potentialTypeMap;
            }

		    return typeMap;
		}

        private static int GetInheritanceDepth(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return InheritanceTree(type).Count();
        }

        private static IEnumerable<Type> InheritanceTree(Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }

		public TypeMap FindTypeMapFor(ResolutionResult resolutionResult, Type destinationType)
		{
			return FindTypeMapFor(resolutionResult.Value, null, resolutionResult.Type, destinationType) ??
			       FindTypeMapFor(resolutionResult.Value, null, resolutionResult.MemberType, destinationType);
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

		public void AssertConfigurationIsValid<TProfile>()
            where TProfile : Profile, new()
		{
			AssertConfigurationIsValid(new TProfile().ProfileName);
		}

		public void AssertConfigurationIsValid()
		{
			AssertConfigurationIsValid(_typeMaps);
		}

	    public IObjectMapper[] GetMappers()
	    {
	        return _mappers.ToArray();
	    }

		private IMappingExpression<TSource, TDestination> CreateMappingExpression<TSource, TDestination>(TypeMap typeMap)
		{
			IMappingExpression<TSource, TDestination> mappingExp =
				new MappingExpression<TSource, TDestination>(typeMap, _serviceCtor, this);

		    TypeInfo destInfo = typeMap.ConfiguredMemberList == MemberList.Destination
                ? new TypeInfo(typeof(TDestination), BindingFlags)
                : new TypeInfo(typeof(TSource), BindingFlags);

			foreach (var destProperty in destInfo.GetPublicWriteAccessors())
			{
				object[] attrs = destProperty.GetCustomAttributes(true);
				if (attrs.Any(x => x is IgnoreMapAttribute))
				{
					mappingExp = mappingExp.ForMember(destProperty.Name, y => y.Ignore());
				}
			}

			return mappingExp;
		}

		private IMappingExpression CreateMappingExpression(TypeMap typeMap, Type destinationType)
		{
			IMappingExpression mappingExp = new MappingExpression(typeMap, _serviceCtor);

            TypeInfo destInfo = new TypeInfo(destinationType, BindingFlags);
			foreach (var destProperty in destInfo.GetPublicWriteAccessors())
			{
				object[] attrs = destProperty.GetCustomAttributes(true);
				if (attrs.Any(x => x is IgnoreMapAttribute))
				{
					mappingExp = mappingExp.ForMember(destProperty.Name, y => y.Ignore());
				}
			}

			return mappingExp;
		}

		private void AssertConfigurationIsValid(IEnumerable<TypeMap> typeMaps)
		{
		    var maps = typeMaps as TypeMap[] ?? typeMaps.ToArray();
		    var badTypeMaps =
				(from typeMap in maps
				where ShouldCheckMap(typeMap)
				let unmappedPropertyNames = typeMap.GetUnmappedPropertyNames()
				where unmappedPropertyNames.Length > 0
                select new AutoMapperConfigurationException.TypeMapConfigErrors(typeMap, unmappedPropertyNames)
                ).ToArray();

			if (badTypeMaps.Any())
			{
				throw new AutoMapperConfigurationException(badTypeMaps);
			}

			var typeMapsChecked = new List<TypeMap>();

			foreach (var typeMap in maps)
			{
				DryRunTypeMap(typeMapsChecked, new ResolutionContext(typeMap, null, typeMap.SourceType, typeMap.DestinationType, new MappingOperationOptions(), Mapper.Engine));
			}
		}

	    private static bool ShouldCheckMap(TypeMap typeMap)
	    {
	        return (typeMap.CustomMapper == null && typeMap.CustomProjection == null) && !FeatureDetector.IsIDataRecordType(typeMap.SourceType);
	    }

	    private TypeMap FindTypeMap(object source, object destination, Type sourceType, Type destinationType, string profileName)
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
                        typeMap = ((IConfigurationProvider)this).FindTypeMapFor(source, destination, sourceInterface, destinationType);

                        if (typeMap == null) continue;

                        var derivedTypeFor = typeMap.GetDerivedTypeFor(sourceType);
                        if (derivedTypeFor != destinationType)
                        {
                            typeMap = CreateTypeMap(sourceType, derivedTypeFor, profileName, typeMap.ConfiguredMemberList);
                        }

                        break;
                    }

                    if ((sourceType.BaseType != null) && (typeMap == null))
                        typeMap = ((IConfigurationProvider)this).FindTypeMapFor(source, destination, sourceType.BaseType, destinationType);
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
							var memberTypeMap = ((IConfigurationProvider)this).FindTypeMapFor(sourceType, destinationType);

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
				TypeMap itemTypeMap = ((IConfigurationProvider) this).FindTypeMapFor(sourceElementType, destElementType);

                if (typeMapsChecked.Any(typeMap => Equals(typeMap, itemTypeMap)))
                    return;
                
                var memberContext = context.CreateElementContext(itemTypeMap, null, sourceElementType, destElementType, 0);
                
                DryRunTypeMap(typeMapsChecked, memberContext);
			}

		}

		protected void OnTypeMapCreated(TypeMap typeMap)
		{
			var typeMapCreated = TypeMapCreated;
			if (typeMapCreated != null)
				typeMapCreated(this, new TypeMapCreatedEventArgs(typeMap));
		}

		internal FormatterExpression GetProfile(string profileName)
		{
		    FormatterExpression expr = _formatterProfiles.GetOrAdd(profileName,
		                                                           name => new FormatterExpression(t => (IValueFormatter) _serviceCtor(t)));

		    return expr;
		}

	    public void AddGlobalIgnore(string startingwith)
	    {
	        _globalIgnore.Add(startingwith);
	    }
	}
}
