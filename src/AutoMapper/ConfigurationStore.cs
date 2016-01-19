
namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using Configuration;
    using Internal;
    using Mappers;
    using QueryableExtensions.Impl;

    public class ConfigurationStore : IConfigurationProvider
    {
        private readonly ITypeMapFactory _typeMapFactory;
        private readonly IEnumerable<IObjectMapper> _mappers;
        private readonly IEnumerable<ITypeMapObjectMapper> _typeMapObjectMappers;
        internal const string DefaultProfileName = "";

        private readonly ConcurrentDictionary<TypePair, TypeMap> _userDefinedTypeMaps =
            new ConcurrentDictionary<TypePair, TypeMap>();

        private readonly ConcurrentDictionary<TypePair, TypeMap> _typeMapPlanCache =
            new ConcurrentDictionary<TypePair, TypeMap>();

        private readonly ConcurrentDictionary<TypePair, CreateTypeMapExpression> _typeMapExpressionCache =
            new ConcurrentDictionary<TypePair, CreateTypeMapExpression>();

        internal readonly ConcurrentDictionary<string, IProfileExpression> _formatterProfiles =
            new ConcurrentDictionary<string, IProfileExpression>();

        private Func<Type, object> _serviceCtor = ObjectCreator.CreateObject;

        private readonly List<string> _globalIgnore;

        public ConfigurationStore(ITypeMapFactory typeMapFactory, IEnumerable<IObjectMapper> mappers, IEnumerable<ITypeMapObjectMapper> typeMapObjectMappers)
        {
            _typeMapFactory = typeMapFactory;
            _mappers = mappers;
            _typeMapObjectMappers = typeMapObjectMappers;
            _globalIgnore = new List<string>();
        }

        public ConfigurationStore() : this(new TypeMapFactory(), MapperRegistry.Mappers, TypeMapObjectMapperRegistry.Mappers)
        {
        }

        public Func<Type, object> ServiceCtor => _serviceCtor;

        public void ForAllMaps(Action<TypeMap, IMappingExpression> configuration)
        {
            ForAllMaps(DefaultProfileName, configuration);
        }

        public TypeDetails GetTypeInfo(Type type)
        {
            return TypeMapFactory.GetTypeInfo(type, GetProfile(DefaultProfileName));
        }

        internal void ForAllMaps(string profileName, Action<TypeMap, IMappingExpression> configuration)
        {
            foreach (var typeMap in _userDefinedTypeMaps.Select(kv => kv.Value).Where(tm => tm.Profile == profileName))
            {
                configuration(typeMap, CreateMappingExpression(typeMap, typeMap.DestinationType));
            }
        }

        public Func<PropertyInfo, bool> ShouldMapProperty
        {
            get { return GetProfile(DefaultProfileName).ShouldMapProperty; }
            set { GetProfile(DefaultProfileName).ShouldMapProperty = value; }
        }

        public Func<FieldInfo, bool> ShouldMapField
        {
            get { return GetProfile(DefaultProfileName).ShouldMapField; }
            set { GetProfile(DefaultProfileName).ShouldMapField = value; }
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

        /// <summary>
        /// Create any missing type maps, if found
        /// </summary>
        public bool CreateMissingTypeMaps
        {
            set { GetProfile(DefaultProfileName).CreateMissingTypeMaps = value; }
        }


        public void IncludeSourceExtensionMethods(Assembly assembly)
        {
            GetProfile(DefaultProfileName).IncludeSourceExtensionMethods(assembly);
        }

        public INamingConvention SourceMemberNamingConvention
        {
            get
            {
                INamingConvention convention = null;
                GetProfile(DefaultProfileName).DefaultMemberConfig.AddMember<NameSplitMember>(_ => convention = _.SourceMemberNamingConvention);
                return convention;
        }
            set { GetProfile(DefaultProfileName).DefaultMemberConfig.AddMember<NameSplitMember>(_ => _.SourceMemberNamingConvention = value); }
        }

        public INamingConvention DestinationMemberNamingConvention
        {
            get
            {
                INamingConvention convention = null;
                GetProfile(DefaultProfileName).DefaultMemberConfig.AddMember<NameSplitMember>(_ => convention = _.DestinationMemberNamingConvention);
                return convention;
        }
            set { GetProfile(DefaultProfileName).DefaultMemberConfig.AddMember<NameSplitMember>(_ => _.DestinationMemberNamingConvention = value); }
        }

        public IEnumerable<MethodInfo> SourceExtensionMethods => GetProfile(DefaultProfileName).SourceExtensionMethods;

        public IEnumerable<IMemberConfiguration> MemberConfigurations => GetProfile(DefaultProfileName).MemberConfigurations;

        public IMemberConfiguration DefaultMemberConfig => GetProfile(DefaultProfileName).DefaultMemberConfig;
        public IMemberConfiguration AddMemberConfiguration()
        {
            return GetProfile(DefaultProfileName).AddMemberConfiguration();
        }

        public IEnumerable<IConditionalObjectMapper> TypeConfigurations => GetProfile(DefaultProfileName).TypeConfigurations;

        public IConditionalObjectMapper AddConditionalObjectMapper()
        {
            var condition = new ConditionalObjectMapper(DefaultProfileName);
            (TypeConfigurations as List<IConditionalObjectMapper>).Add(condition);
            return condition;
        }

        public bool ConstructorMappingEnabled => GetProfile(DefaultProfileName).ConstructorMappingEnabled;

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
            GetProfile(DefaultProfileName).DisableConstructorMapping();
        }


        public void Seal()
        {
            var derivedMaps = new List<Tuple<TypePair, TypeMap>>();
            var redirectedTypes = new List<Tuple<TypePair, TypePair>>();
            foreach (var typeMap in _userDefinedTypeMaps.Select(kv => kv.Value))
            {
                typeMap.Seal();

                _typeMapPlanCache.AddOrUpdate(typeMap.Types, typeMap, (_, _2) => typeMap);
                if (typeMap.DestinationTypeOverride != null)
                {
                    redirectedTypes.Add(Tuple.Create(typeMap.Types, new TypePair(typeMap.SourceType, typeMap.DestinationTypeOverride)));
                }
                derivedMaps.AddRange(GetDerivedTypeMaps(typeMap).Select(derivedMap => Tuple.Create(new TypePair(derivedMap.SourceType, typeMap.DestinationType), derivedMap)));
            }
            foreach (var redirectedType in redirectedTypes)
            {
                var derivedMap = FindTypeMapFor(redirectedType.Item2);
                if (derivedMap != null)
                {
                    _typeMapPlanCache.AddOrUpdate(redirectedType.Item1, derivedMap, (_, _2) => derivedMap);
                }
            }
            foreach (var derivedMap in derivedMaps)
            {
                _typeMapPlanCache.GetOrAdd(derivedMap.Item1, _ => derivedMap.Item2);
            }
        }

        private IEnumerable<TypeMap> GetDerivedTypeMaps(TypeMap typeMap)
        {
            foreach (var derivedMap in typeMap.IncludedDerivedTypes.Select(FindTypeMapFor))
            {
                if(derivedMap == null)
                {
                    throw QueryMapperHelper.MissingMapException(typeMap.SourceType, typeMap.DestinationType);
                }
                yield return derivedMap;
                foreach(var derivedTypeMap in GetDerivedTypeMaps(derivedMap))
                {
                    yield return derivedTypeMap;
                }
            }
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

        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(string profileName,
            MemberList memberList)
        {
            TypeMap typeMap = CreateTypeMap(new TypePair(typeof(TSource), typeof(TDestination)), profileName, memberList);

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
            var typePair = new TypePair(sourceType, destinationType);

            if (sourceType.IsGenericTypeDefinition() && destinationType.IsGenericTypeDefinition())
            {
                var expression = _typeMapExpressionCache.GetOrAdd(typePair,
                    tp => new CreateTypeMapExpression(tp, memberList, profileName));

                return expression;
            }

            var typeMap = CreateTypeMap(typePair, profileName, memberList);

            return CreateMappingExpression(typeMap, destinationType);
        }

        public void ClearPrefixes()
        {
            GetProfile(DefaultProfileName).DefaultMemberConfig.AddName<PrePostfixName>(_ => _.Prefixes.Clear());
        }

        public void RecognizeAlias(string original, string alias)
        {
            GetProfile(DefaultProfileName).DefaultMemberConfig.AddName<ReplaceName>(_ => _.AddReplace(original, alias));
        }

        public void ReplaceMemberName(string original, string newValue)
        {
            GetProfile(DefaultProfileName).DefaultMemberConfig.AddName<ReplaceName>(_ => _.AddReplace(original, newValue));
        }

        public void RecognizePrefixes(params string[] prefixes)
        {
            GetProfile(DefaultProfileName).DefaultMemberConfig.AddName<PrePostfixName>(_ => _.AddStrings(p => p.Prefixes, prefixes));
        }

        public void RecognizePostfixes(params string[] postfixes)
        {
            GetProfile(DefaultProfileName).DefaultMemberConfig.AddName<PrePostfixName>(_ => _.AddStrings(p => p.Postfixes, postfixes));
        }

        public void RecognizeDestinationPrefixes(params string[] prefixes)
        {
            GetProfile(DefaultProfileName).DefaultMemberConfig.AddName<PrePostfixName>(_ => _.AddStrings(p => p.DestinationPrefixes, prefixes));
        }

        public void RecognizeDestinationPostfixes(params string[] postfixes)
        {
            GetProfile(DefaultProfileName).DefaultMemberConfig.AddName<PrePostfixName>(_ => _.AddStrings(p => p.DestinationPostfixes, postfixes));
        }

        public TypeMap CreateTypeMap(TypePair types, string profileName = DefaultProfileName)
        {
		    return CreateTypeMap(types, profileName, MemberList.Destination);
        }

        public TypeMap CreateTypeMap(TypePair types, string profileName, MemberList memberList)
        {
            var typeMap = _userDefinedTypeMaps.GetOrAdd(types, tp =>
            {
                var profileConfiguration = GetProfile(profileName);

                var tm = _typeMapFactory.CreateTypeMap(types.SourceType, types.DestinationType, profileConfiguration, memberList);

                tm.Profile = profileName;
                tm.IgnorePropertiesStartingWith = _globalIgnore;

                IncludeBaseMappings(types, tm);

                // keep the cache in sync
                TypeMap _;
                _typeMapPlanCache.TryRemove(tp, out _);

                return tm;
            });

            return typeMap;
        }

        private void IncludeBaseMappings(TypePair types, TypeMap typeMap)
        {
            foreach(var inheritedTypeMap in _userDefinedTypeMaps.Select(kv => kv.Value).Where(t => t.TypeHasBeenIncluded(types)))
            {
                typeMap.ApplyInheritedMap(inheritedTypeMap);
                IncludeBaseMappings(inheritedTypeMap.Types, typeMap);
            }
        }

        public TypeMap[] GetAllTypeMaps()
        {
            return _userDefinedTypeMaps.Select(kv => kv.Value).ToArray();
        }

        public TypeMap FindTypeMapFor(Type sourceType, Type destinationType)
        {
            var typePair = new TypePair(sourceType, destinationType);

            return FindTypeMapFor(typePair);
        }

        public TypeMap FindTypeMapFor(TypePair typePair)
        {
            TypeMap typeMap;
            _userDefinedTypeMaps.TryGetValue(typePair, out typeMap);
            return typeMap;
        }

        private bool CoveredByObjectMap(TypePair typePair)
        {
            return GetMappers().FirstOrDefault(m => m.IsMatch(typePair)) != null;
        }

        private TypeMap FindConventionTypeMapFor(TypePair typePair)
        {
            var matchingTypeMapConfiguration = _formatterProfiles.Select(kv => kv.Value).SelectMany(p => p.TypeConfigurations).FirstOrDefault(tc => tc.IsMatch(typePair));
            return matchingTypeMapConfiguration != null ? CreateTypeMap(typePair, matchingTypeMapConfiguration.ProfileName) : null;
        }

        public TypeMap ResolveTypeMap(Type sourceType, Type destinationType)
        {
            return ResolveTypeMap(sourceType, destinationType, destinationObjectExists: false);
        }

        public TypeMap ResolveTypeMap(Type sourceType, Type destinationType, bool destinationObjectExists)
        {
            var typePair = new TypePair(sourceType, destinationType);

            return ResolveTypeMap(typePair, destinationObjectExists);
        }

        public TypeMap ResolveTypeMap(TypePair typePair)
        {
            return ResolveTypeMap(typePair, destinationObjectExists: false);
        }

        public TypeMap ResolveTypeMap(TypePair typePair, bool destinationObjectExists)
        {
            var typeMap = _typeMapPlanCache.GetOrAdd(typePair,
                _ =>
                    GetRelatedTypePairs(_destinationObjectExists)
                        .Select(
                            tp =>
                                _typeMapPlanCache.GetOrDefault(tp) ??
                                FindTypeMapFor(tp) ??
                                (!CoveredByObjectMap(typePair)
                                    ? FindConventionTypeMapFor(tp) ??
                                      FindClosedGenericTypeMapFor(tp)
                                    : null))
                        .FirstOrDefault(tm => tm != null));

            return typeMap;
        }

        public TypeMap ResolveTypeMap(object source, object destination, Type sourceType, Type destinationType)
        {
            return ResolveTypeMap(source?.GetType() ?? sourceType, destination?.GetType() ?? destinationType, destinationObjectExists: destination != null);
        }

        public TypeMap ResolveTypeMap(ResolutionResult resolutionResult, Type destinationType)
        {
            return ResolveTypeMap(resolutionResult.Type, destinationType) ?? ResolveTypeMap(resolutionResult.MemberType, destinationType);
        }

        public TypeMap FindClosedGenericTypeMapFor(TypePair typePair)
        {
            if(!HasOpenGenericTypeMapDefined(typePair))
                return null;
            var closedGenericTypePair = new TypePair(typePair.SourceType, typePair.DestinationType);
            var sourceGenericDefinition = typePair.SourceType.GetGenericTypeDefinition();
            var destGenericDefinition = typePair.DestinationType.GetGenericTypeDefinition();

            var genericTypePair = new TypePair(sourceGenericDefinition, destGenericDefinition);
            CreateTypeMapExpression genericTypeMapExpression;

            if (!_typeMapExpressionCache.TryGetValue(genericTypePair, out genericTypeMapExpression))
            {
                throw new AutoMapperMappingException("Missing type map configuration or unsupported mapping.");
            }

            var typeMap = CreateTypeMap(closedGenericTypePair,
                genericTypeMapExpression.ProfileName,
                genericTypeMapExpression.MemberList);

            var mappingExpression = CreateMappingExpression(typeMap, closedGenericTypePair.DestinationType);

            genericTypeMapExpression.Accept(mappingExpression);

            return typeMap;
        }

        private bool HasOpenGenericTypeMapDefined(TypePair typePair)
        {
            var isGeneric = typePair.SourceType.IsGenericType()
                            && typePair.DestinationType.IsGenericType()
                            && (typePair.SourceType.GetGenericTypeDefinition() != null)
                            && (typePair.DestinationType.GetGenericTypeDefinition() != null);
            if (!isGeneric)
                return false;
            var sourceGenericDefinition = typePair.SourceType.GetGenericTypeDefinition();
            var destGenericDefinition = typePair.DestinationType.GetGenericTypeDefinition();

            var genericTypePair = new TypePair(sourceGenericDefinition, destGenericDefinition);

            return _typeMapExpressionCache.ContainsKey(genericTypePair);
        }

        private IEnumerable<TypePair> GetRelatedTypePairs(TypePair root, bool includeDestinationBases)
        {
            var subTypePairs =
                from destinationType in GetAllTypes(root.DestinationType, includeDestinationBases)
                from sourceType in GetAllTypes(root.SourceType)
                select new TypePair(sourceType, destinationType);
            return subTypePairs;
        }

        private IEnumerable<Type> GetAllTypes(Type type, bool includeBases)
        {
            yield return type;

            if(includeBases)
            {
                Type baseType = type.BaseType();
                while(baseType != null)
                {
                    yield return baseType;
                    baseType = baseType.BaseType();
                }
            }

            foreach (var interfaceType in type.GetTypeInfo().ImplementedInterfaces)
            {
                yield return interfaceType;
            }
        }

		public IProfileExpression GetProfileConfiguration(string profileName)
        {
            return GetProfile(profileName);
        }

        public void AssertConfigurationIsValid(TypeMap typeMap)
        {
            AssertConfigurationIsValid(Enumerable.Repeat(typeMap, 1));
        }

        public void AssertConfigurationIsValid(string profileName)
        {
            AssertConfigurationIsValid(_userDefinedTypeMaps.Select(kv => kv.Value).Where(typeMap => typeMap.Profile == profileName));
        }

        public void AssertConfigurationIsValid<TProfile>()
            where TProfile : Profile, new()
        {
            AssertConfigurationIsValid(new TProfile().ProfileName);
        }

        public void AssertConfigurationIsValid()
        {
            AssertConfigurationIsValid(_userDefinedTypeMaps.Select(kv => kv.Value));
        }

	    public IEnumerable<IObjectMapper> GetMappers()
        {
	        return _mappers;
        }

        public IEnumerable<ITypeMapObjectMapper> GetTypeMapMappers() => _typeMapObjectMappers;

        private IMappingExpression<TSource, TDestination> CreateMappingExpression<TSource, TDestination>(TypeMap typeMap)
        {
            var mappingExp = new MappingExpression<TSource, TDestination>(typeMap, _serviceCtor, this);
            var type = (typeMap.ConfiguredMemberList == MemberList.Destination) ? typeof(TDestination) : typeof(TSource);
            return Ignore(mappingExp, type);
        }

        private IMappingExpression<TSource, TDestination> Ignore<TSource, TDestination>(IMappingExpression<TSource, TDestination> mappingExp, Type destinationType)
        {
            var destInfo = new TypeDetails(destinationType, ShouldMapProperty, ShouldMapField);
            foreach (var destProperty in destInfo.PublicWriteAccessors)
            {
                var attrs = destProperty.GetCustomAttributes(true);
                if (attrs.Any(x => x is IgnoreMapAttribute))
                {
                    mappingExp = mappingExp.ForMember(destProperty.Name, y => y.Ignore());
                }
                if (_globalIgnore.Contains(destProperty.Name))
                {
                    mappingExp = mappingExp.ForMember(destProperty.Name, y => y.Ignore());
                }
            }
            return mappingExp;
        }

        private IMappingExpression CreateMappingExpression(TypeMap typeMap, Type destinationType)
        {
            var mappingExp = new MappingExpression(typeMap, _serviceCtor, this);
            return (IMappingExpression)Ignore(mappingExp, destinationType);
        }

        private void AssertConfigurationIsValid(IEnumerable<TypeMap> typeMaps)
        {
            Seal();
            var maps = typeMaps as TypeMap[] ?? typeMaps.ToArray();
            var badTypeMaps =
                (from typeMap in maps
                 where typeMap.ShouldCheckForValid()
                    let unmappedPropertyNames = typeMap.GetUnmappedPropertyNames()
                    where unmappedPropertyNames.Length > 0
                    select new AutoMapperConfigurationException.TypeMapConfigErrors(typeMap, unmappedPropertyNames)
                    ).ToArray();

            if (badTypeMaps.Any())
            {
                throw new AutoMapperConfigurationException(badTypeMaps);
            }

            var typeMapsChecked = new List<TypeMap>();
            var configExceptions = new List<Exception>();

            foreach (var typeMap in maps)
            {
                try
                {
                    DryRunTypeMap(typeMapsChecked,
                        new ResolutionContext(typeMap, null, typeMap.SourceType, typeMap.DestinationType,
                            new MappingOperationOptions(), Mapper.Engine));
                }
                catch (Exception e)
                {
                    configExceptions.Add(e);
                }
            }

            if (configExceptions.Count > 1)
            {
                throw new AggregateException(configExceptions);
            }
            if (configExceptions.Count > 0)
            {
                throw configExceptions[0];
            }
        }

        private void DryRunTypeMap(ICollection<TypeMap> typeMapsChecked, ResolutionContext context)
        {
            var typeMap = context.TypeMap;
            if (typeMap != null)
            {
                typeMapsChecked.Add(typeMap);
                CheckPropertyMaps(typeMapsChecked, context);
            }
            else
            {
                var mapperToUse = GetMappers().FirstOrDefault(mapper => mapper.IsMatch(context.Types));
                if (mapperToUse == null && context.SourceType.IsNullableType())
                {
                    var nullableTypes = new TypePair(Nullable.GetUnderlyingType(context.SourceType),
                        context.DestinationType);
                    mapperToUse = GetMappers().FirstOrDefault(mapper => mapper.IsMatch(nullableTypes));
                }
                if (mapperToUse == null)
                {
                    throw new AutoMapperConfigurationException(context);
                }
                if (mapperToUse is ArrayMapper || mapperToUse is EnumerableMapper || mapperToUse is CollectionMapper)
                {
                    CheckElementMaps(typeMapsChecked, context);
                }
            }
        }

        private void CheckElementMaps(ICollection<TypeMap> typeMapsChecked, ResolutionContext context)
        {
            Type sourceElementType = TypeHelper.GetElementType(context.SourceType);
            Type destElementType = TypeHelper.GetElementType(context.DestinationType);
            TypeMap itemTypeMap = ((IConfigurationProvider)this).ResolveTypeMap(sourceElementType, destElementType);

            if(typeMapsChecked.Any(typeMap => Equals(typeMap, itemTypeMap)))
                return;

            var memberContext = context.CreateElementContext(itemTypeMap, null, sourceElementType, destElementType,
                0);

            DryRunTypeMap(typeMapsChecked, memberContext);
        }

        private void CheckPropertyMaps(ICollection<TypeMap> typeMapsChecked, ResolutionContext context)
        {
            foreach(var propertyMap in context.TypeMap.GetPropertyMaps())
            {
                if(!propertyMap.IsIgnored())
                {
                    var lastResolver =
                        propertyMap.GetSourceValueResolvers().OfType<IMemberResolver>().LastOrDefault();

                    if(lastResolver != null)
                    {
                        var sourceType = lastResolver.MemberType;
                        var destinationType = propertyMap.DestinationProperty.MemberType;
                        var memberTypeMap = ((IConfigurationProvider)this).ResolveTypeMap(sourceType,
                            destinationType);

                        if(typeMapsChecked.Any(typeMap => Equals(typeMap, memberTypeMap)))
                            continue;

                        var memberContext = context.CreateMemberContext(memberTypeMap, null, null, sourceType,
                            propertyMap);

                        DryRunTypeMap(typeMapsChecked, memberContext);
                    }
                }
            }
        }

        internal IProfileExpression GetProfile(string profileName)
        {
            var expr = _formatterProfiles.GetOrAdd(profileName, name => new Profile(profileName));

            return expr;
        }

        public void AddGlobalIgnore(string startingwith)
        {
            _globalIgnore.Add(startingwith);
        }
    }
}
