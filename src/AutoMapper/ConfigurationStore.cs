namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Configuration;
    using Impl;
    using Internal;
    using Mappers;

    public class ConfigurationStore : IConfigurationProvider, IConfiguration
    {
        private static readonly IDictionaryFactory DictionaryFactory = PlatformAdapter.Resolve<IDictionaryFactory>();
        private readonly ITypeMapFactory _typeMapFactory;
        private readonly IEnumerable<IObjectMapper> _mappers;
        internal const string DefaultProfileName = "";

        private readonly Internal.IDictionary<TypePair, TypeMap> _userDefinedTypeMaps =
            DictionaryFactory.CreateDictionary<TypePair, TypeMap>();

        private readonly Internal.IDictionary<TypePair, TypeMap> _typeMapPlanCache =
            DictionaryFactory.CreateDictionary<TypePair, TypeMap>();

        private readonly Internal.IDictionary<TypePair, CreateTypeMapExpression> _typeMapExpressionCache =
            DictionaryFactory.CreateDictionary<TypePair, CreateTypeMapExpression>();

        private readonly Internal.IDictionary<string, ProfileConfiguration> _formatterProfiles =
            DictionaryFactory.CreateDictionary<string, ProfileConfiguration>();

        private Func<Type, object> _serviceCtor = ObjectCreator.CreateObject;

        private readonly List<string> _globalIgnore;

        public ConfigurationStore(ITypeMapFactory typeMapFactory, IEnumerable<IObjectMapper> mappers)
        {
            _typeMapFactory = typeMapFactory;
            _mappers = mappers;
            _globalIgnore = new List<string>();
        }

        public event EventHandler<TypeMapCreatedEventArgs> TypeMapCreated;

        public Func<Type, object> ServiceCtor => _serviceCtor;

        public void ForAllMaps(Action<TypeMap, IMappingExpression> configuration)
        {
            ForAllMaps(DefaultProfileName, configuration);
        }

        internal void ForAllMaps(string profileName, Action<TypeMap, IMappingExpression> configuration)
        {
            foreach(var typeMap in _userDefinedTypeMaps.Values.Where(tm => tm.Profile == profileName))
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

        public IEnumerable<string> Prefixes => GetProfile(DefaultProfileName).Prefixes;

        public IEnumerable<string> Postfixes => GetProfile(DefaultProfileName).Postfixes;

        public IEnumerable<string> DestinationPrefixes => GetProfile(DefaultProfileName).DestinationPrefixes;

        public IEnumerable<string> DestinationPostfixes => GetProfile(DefaultProfileName).DestinationPostfixes;

        public IEnumerable<MemberNameReplacer> MemberNameReplacers => GetProfile(DefaultProfileName).MemberNameReplacers;

        public IEnumerable<AliasedMember> Aliases => GetProfile(DefaultProfileName).Aliases;

        public bool ConstructorMappingEnabled => GetProfile(DefaultProfileName).ConstructorMappingEnabled;

        public bool DataReaderMapperYieldReturnEnabled => GetProfile(DefaultProfileName).DataReaderMapperYieldReturnEnabled;

        public IEnumerable<MethodInfo> SourceExtensionMethods => GetProfile(DefaultProfileName).SourceExtensionMethods;

        bool IProfileConfiguration.MapNullSourceValuesAsNull => AllowNullDestinationValues;

        bool IProfileConfiguration.MapNullSourceCollectionsAsNull => AllowNullCollections;

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
            foreach (var typeMap in _userDefinedTypeMaps.Values)
            {
                typeMap.Seal();

                var typePair = new TypePair(typeMap.SourceType, typeMap.DestinationType);
                _typeMapPlanCache.AddOrUpdate(typePair, typeMap, (_, _2) => typeMap);
                if (typeMap.DestinationTypeOverride != null)
                {
                    var includedDerivedType = new TypePair(typeMap.SourceType, typeMap.DestinationTypeOverride);
                    var derivedMap = FindTypeMapFor(includedDerivedType);
                    if (derivedMap != null)
                    {
                        _typeMapPlanCache.AddOrUpdate(typePair, derivedMap, (_, _2) => derivedMap);
                    }
                }
                foreach (var derivedMap in GetDerivedTypeMaps(typeMap))
                {
                    _typeMapPlanCache.AddOrUpdate(new TypePair(derivedMap.SourceType, typeMap.DestinationType),
                        derivedMap, (_, _2) => derivedMap);
                }
            }
        }

        private IEnumerable<TypeMap> GetDerivedTypeMaps(TypeMap typeMap)
        {
            if (typeMap == null)
                yield break;

            foreach (var derivedMap in typeMap.IncludedDerivedTypes.Select(FindTypeMapFor))
            {
                if (derivedMap != null)
                    yield return derivedMap;

                foreach (var derivedTypeMap in GetDerivedTypeMaps(derivedMap))
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
            TypeMap typeMap = CreateTypeMap(typeof (TSource), typeof (TDestination), profileName, memberList);

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
            if (sourceType.IsGenericTypeDefinition() && destinationType.IsGenericTypeDefinition())
            {
                var typePair = new TypePair(sourceType, destinationType);

                var expression = _typeMapExpressionCache.GetOrAdd(typePair,
                    tp => new CreateTypeMapExpression(tp, memberList, profileName));

                return expression;
            }

            var typeMap = CreateTypeMap(sourceType, destinationType, profileName, memberList);

            return CreateMappingExpression(typeMap, destinationType);
        }

        public void ClearPrefixes()
        {
            GetProfile(DefaultProfileName).ClearPrefixes();
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
            var typePair = new TypePair(source, destination);
            var typeMap = _userDefinedTypeMaps.GetOrAdd(typePair, tp =>
            {
                var profileConfiguration = GetProfile(profileName);

                var tm = _typeMapFactory.CreateTypeMap(source, destination, profileConfiguration, memberList);

                tm.Profile = profileName;
                tm.IgnorePropertiesStartingWith = _globalIgnore;

                IncludeBaseMappings(source, destination, tm);

                // keep the cache in sync
                TypeMap _;
                _typeMapPlanCache.TryRemove(tp, out _);

                OnTypeMapCreated(tm);

                return tm;
            });

            return typeMap;
        }

        private void IncludeBaseMappings(Type source, Type destination, TypeMap typeMap)
        {
            foreach (
                var inheritedTypeMap in
                    _userDefinedTypeMaps.Values.Where(t => t.TypeHasBeenIncluded(source, destination)))
            {
                typeMap.ApplyInheritedMap(inheritedTypeMap);
            }
        }

        public TypeMap[] GetAllTypeMaps()
        {
            return _userDefinedTypeMaps.Values.ToArray();
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

        public TypeMap ResolveTypeMap(Type sourceType, Type destinationType)
        {
            var typePair = new TypePair(sourceType, destinationType);

            return ResolveTypeMap(typePair);
        }

        public TypeMap ResolveTypeMap(TypePair typePair)
        {
            var typeMap = _typeMapPlanCache.GetOrAdd(typePair,
                _ =>
                    GetRelatedTypePairs(_)
                        .Select(tp => FindTypeMapFor(tp) ?? _typeMapPlanCache.GetOrDefault(tp))
                        .FirstOrDefault(tm => tm != null));

            return typeMap;
        }

        public TypeMap ResolveTypeMap(object source, object destination, Type sourceType, Type destinationType)
        {
            return ResolveTypeMap(source?.GetType() ?? sourceType, destination?.GetType() ?? destinationType);
        }

        public TypeMap ResolveTypeMap(ResolutionResult resolutionResult, Type destinationType)
        {
            return ResolveTypeMap(resolutionResult.Value, null, resolutionResult.Type, destinationType) ??
                   ResolveTypeMap(resolutionResult.Value, null, resolutionResult.MemberType, destinationType);
        }

        public TypeMap FindClosedGenericTypeMapFor(ResolutionContext context)
        {
            var closedGenericTypePair = new TypePair(context.SourceType, context.DestinationType);
            var sourceGenericDefinition = context.SourceType.GetGenericTypeDefinition();
            var destGenericDefinition = context.DestinationType.GetGenericTypeDefinition();

            var genericTypePair = new TypePair(sourceGenericDefinition, destGenericDefinition);
            CreateTypeMapExpression genericTypeMapExpression;

            if (!_typeMapExpressionCache.TryGetValue(genericTypePair, out genericTypeMapExpression))
            {
                throw new AutoMapperMappingException(context, "Missing type map configuration or unsupported mapping.");
            }

            var typeMap = CreateTypeMap(closedGenericTypePair.SourceType, closedGenericTypePair.DestinationType,
                genericTypeMapExpression.ProfileName,
                genericTypeMapExpression.MemberList);

            var mappingExpression = CreateMappingExpression(typeMap, closedGenericTypePair.DestinationType);

            genericTypeMapExpression.Accept(mappingExpression);

            return typeMap;
        }

        public bool HasOpenGenericTypeMapDefined(ResolutionContext context)
        {
            var sourceGenericDefinition = context.SourceType.GetGenericTypeDefinition();
            var destGenericDefinition = context.DestinationType.GetGenericTypeDefinition();

            var genericTypePair = new TypePair(sourceGenericDefinition, destGenericDefinition);

            return _typeMapExpressionCache.ContainsKey(genericTypePair);
        }

        private IEnumerable<TypePair> GetRelatedTypePairs(TypePair root)
        {
            var includeOverrideTypePairs = 
                GetAllTypeMaps()
                    .Where(tm => tm.HasDerivedTypesToInclude() && tm.SourceType.IsAssignableFrom(root.SourceType) && (tm.DestinationTypeOverride ?? tm.DestinationType) != root.DestinationType && tm.DestinationType.IsAssignableFrom(root.DestinationType))
                    .Select(tm => new TypePair(tm.SourceType,tm.DestinationTypeOverride ?? tm.GetDerivedTypeFor(root.SourceType))).ToList();
            var subTypePairs =
                from sourceType in GetAllTypes(root.SourceType)
                from destinationType in GetAllTypes(root.DestinationType)
                select new TypePair(sourceType, destinationType);
            return includeOverrideTypePairs.Concat(subTypePairs);
        }

        private IEnumerable<Type> GetAllTypes(Type type)
        {
            yield return type;

            if (type.IsValueType() && !type.IsNullableType())
                yield return typeof (Nullable<>).MakeGenericType(type);

            Type baseType = type.BaseType();
            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseType();
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                yield return interfaceType;
            }
        }

        public IProfileConfiguration GetProfileConfiguration(string profileName)
        {
            return GetProfile(profileName);
        }

        public void AssertConfigurationIsValid(TypeMap typeMap)
        {
            AssertConfigurationIsValid(Enumerable.Repeat(typeMap, 1));
        }

        public void AssertConfigurationIsValid(string profileName)
        {
            AssertConfigurationIsValid(_userDefinedTypeMaps.Values.Where(typeMap => typeMap.Profile == profileName));
        }

        public void AssertConfigurationIsValid<TProfile>()
            where TProfile : Profile, new()
        {
            AssertConfigurationIsValid(new TProfile().ProfileName);
        }

        public void AssertConfigurationIsValid()
        {
            AssertConfigurationIsValid(_userDefinedTypeMaps.Values);
        }

        public IObjectMapper[] GetMappers()
        {
            return _mappers.ToArray();
        }

        private IMappingExpression<TSource, TDestination> CreateMappingExpression<TSource, TDestination>(TypeMap typeMap)
        {
            var mappingExp = new MappingExpression<TSource, TDestination>(typeMap, _serviceCtor, this);
            var type = (typeMap.ConfiguredMemberList == MemberList.Destination) ? typeof(TDestination) : typeof(TSource);
            return Ignore(mappingExp, type);
        }

        private IMappingExpression<TSource, TDestination> Ignore<TSource, TDestination>(IMappingExpression<TSource, TDestination> mappingExp, Type destinationType)
        {
            var destInfo = new TypeInfo(destinationType, ShouldMapProperty, ShouldMapField);
            foreach(var destProperty in destInfo.PublicWriteAccessors)
            {
                var attrs = destProperty.GetCustomAttributes(true);
                if(attrs.Any(x => x is IgnoreMapAttribute))
                {
                    mappingExp = mappingExp.ForMember(destProperty.Name, y => y.Ignore());
                }
                if(_globalIgnore.Contains(destProperty.Name))
                {
                    mappingExp = mappingExp.ForMember(destProperty.Name, y => y.Ignore());
                }
            }
            return mappingExp;
        }

        private IMappingExpression CreateMappingExpression(TypeMap typeMap, Type destinationType)
        {
            var mappingExp = new MappingExpression(typeMap, _serviceCtor, this);
            return (IMappingExpression) Ignore(mappingExp, destinationType);
        }

        private void AssertConfigurationIsValid(IEnumerable<TypeMap> typeMaps)
        {
            Seal();
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

        private static bool ShouldCheckMap(TypeMap typeMap)
        {
            return (typeMap.CustomMapper == null && typeMap.CustomProjection == null &&
                    typeMap.DestinationTypeOverride == null) && !FeatureDetector.IsIDataRecordType(typeMap.SourceType);
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
                        var lastResolver =
                            propertyMap.GetSourceValueResolvers().OfType<IMemberResolver>().LastOrDefault();

                        if (lastResolver != null)
                        {
                            var sourceType = lastResolver.MemberType;
                            var destinationType = propertyMap.DestinationProperty.MemberType;
                            var memberTypeMap = ((IConfigurationProvider) this).ResolveTypeMap(sourceType,
                                destinationType);

                            if (typeMapsChecked.Any(typeMap => Equals(typeMap, memberTypeMap)))
                                continue;

                            var memberContext = context.CreateMemberContext(memberTypeMap, null, null, sourceType,
                                propertyMap);

                            DryRunTypeMap(typeMapsChecked, memberContext);
                        }
                    }
                }
            }
            else if (mapperToUse is ArrayMapper || mapperToUse is EnumerableMapper || mapperToUse is CollectionMapper)
            {
                Type sourceElementType = TypeHelper.GetElementType(context.SourceType);
                Type destElementType = TypeHelper.GetElementType(context.DestinationType);
                TypeMap itemTypeMap = ((IConfigurationProvider) this).ResolveTypeMap(sourceElementType, destElementType);

                if (typeMapsChecked.Any(typeMap => Equals(typeMap, itemTypeMap)))
                    return;

                var memberContext = context.CreateElementContext(itemTypeMap, null, sourceElementType, destElementType,
                    0);

                DryRunTypeMap(typeMapsChecked, memberContext);
            }
        }

        protected void OnTypeMapCreated(TypeMap typeMap)
        {
            TypeMapCreated?.Invoke(this, new TypeMapCreatedEventArgs(typeMap));
        }

        internal ProfileConfiguration GetProfile(string profileName)
        {
            ProfileConfiguration expr = _formatterProfiles.GetOrAdd(profileName,
                name => new ProfileConfiguration());

            return expr;
        }

        public void AddGlobalIgnore(string startingwith)
        {
            _globalIgnore.Add(startingwith);
        }
    }
}