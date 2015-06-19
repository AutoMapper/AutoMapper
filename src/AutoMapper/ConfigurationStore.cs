// ReSharper disable ConvertToAutoProperty
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

    /// <summary>
    /// 
    /// </summary>
    public class ConfigurationStore : IConfigurationProvider, IConfiguration
    {
        /// <summary>
        /// 
        /// </summary>
        private static IDictionaryFactory DictionaryFactory { get; }
            = PlatformAdapter.Resolve<IDictionaryFactory>();

        private readonly ITypeMapFactory _typeMapFactory;
        private readonly IObjectMapperCollection _objectMappers;
        internal const string DefaultProfileName = "";

        /// <summary>
        /// 
        /// </summary>
        private readonly Internal.IDictionary<TypePair, TypeMap> _userDefinedTypeMaps =
            DictionaryFactory.CreateDictionary<TypePair, TypeMap>();

        /// <summary>
        /// 
        /// </summary>
        private readonly Internal.IDictionary<TypePair, TypeMap> _typeMapPlanCache =
            DictionaryFactory.CreateDictionary<TypePair, TypeMap>();

        /// <summary>
        /// 
        /// </summary>
        private readonly Internal.IDictionary<TypePair, CreateTypeMapExpression> _typeMapExpressionCache =
            DictionaryFactory.CreateDictionary<TypePair, CreateTypeMapExpression>();

        /// <summary>
        /// 
        /// </summary>
        private readonly Internal.IDictionary<string, ProfileConfiguration> _formatterProfiles =
            DictionaryFactory.CreateDictionary<string, ProfileConfiguration>();

        /// <summary>
        /// 
        /// </summary>
        private Func<Type, object> _serviceCtor = ObjectCreator.CreateObject;

        /// <summary>
        /// MapperContext backing field.
        /// </summary>
        private readonly IMapperContext _mapperContext;

        /// <summary>
        /// 
        /// </summary>
        private readonly List<string> _globalIgnore;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapperContext"></param>
        /// <param name="typeMapFactory"></param>
        /// <param name="objectMappers"></param>
        public ConfigurationStore(IMapperContext mapperContext, ITypeMapFactory typeMapFactory, IObjectMapperCollection objectMappers)
        {
            _mapperContext = mapperContext;
            _typeMapFactory = typeMapFactory;
            _objectMappers = objectMappers;
            _globalIgnore = new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<TypeMapCreatedEventArgs> TypeMapCreated;

        /// <summary>
        /// 
        /// </summary>
        public Func<Type, object> ServiceCtor => _serviceCtor;

        /// <summary>
        /// 
        /// </summary>
        public bool AllowNullDestinationValues
        {
            get { return GetProfile(DefaultProfileName).AllowNullDestinationValues; }
            set { GetProfile(DefaultProfileName).AllowNullDestinationValues = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool AllowNullCollections
        {
            get { return GetProfile(DefaultProfileName).AllowNullCollections; }
            set { GetProfile(DefaultProfileName).AllowNullCollections = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        public void IncludeSourceExtensionMethods(Assembly assembly)
        {
            GetProfile(DefaultProfileName).IncludeSourceExtensionMethods(assembly);
        }

        /// <summary>
        /// 
        /// </summary>
        public INamingConvention SourceMemberNamingConvention
        {
            get { return GetProfile(DefaultProfileName).SourceMemberNamingConvention; }
            set { GetProfile(DefaultProfileName).SourceMemberNamingConvention = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public INamingConvention DestinationMemberNamingConvention
        {
            get { return GetProfile(DefaultProfileName).DestinationMemberNamingConvention; }
            set { GetProfile(DefaultProfileName).DestinationMemberNamingConvention = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> Prefixes => GetProfile(DefaultProfileName).Prefixes;

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> Postfixes => GetProfile(DefaultProfileName).Postfixes;

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> DestinationPrefixes => GetProfile(DefaultProfileName).DestinationPrefixes;

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> DestinationPostfixes => GetProfile(DefaultProfileName).DestinationPostfixes;

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<MemberNameReplacer> MemberNameReplacers => GetProfile(DefaultProfileName).MemberNameReplacers;

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<AliasedMember> Aliases => GetProfile(DefaultProfileName).Aliases;

        /// <summary>
        /// 
        /// </summary>
        public bool ConstructorMappingEnabled => GetProfile(DefaultProfileName).ConstructorMappingEnabled;

        /// <summary>
        /// 
        /// </summary>
        public bool DataReaderMapperYieldReturnEnabled => GetProfile(DefaultProfileName).DataReaderMapperYieldReturnEnabled;

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<MethodInfo> SourceExtensionMethods => GetProfile(DefaultProfileName).SourceExtensionMethods;

        /// <summary>
        /// 
        /// </summary>
        bool IProfileConfiguration.MapNullSourceValuesAsNull => AllowNullDestinationValues;

        /// <summary>
        /// 
        /// </summary>
        bool IProfileConfiguration.MapNullSourceCollectionsAsNull => AllowNullCollections;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileName"></param>
        /// <returns></returns>
        public IProfileExpression CreateProfile(string profileName)
        {
            var profileExpression = new Profile(profileName);

            profileExpression.Initialize(this);

            return profileExpression;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileName"></param>
        /// <param name="profileConfiguration"></param>
        public void CreateProfile(string profileName, Action<IProfileExpression> profileConfiguration)
        {
            var profileExpression = new Profile(profileName);

            profileExpression.Initialize(this);

            profileConfiguration(profileExpression);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        public void AddProfile(Profile profile)
        {
            profile.Initialize(this);

            profile.Configure();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TProfile"></typeparam>
        public void AddProfile<TProfile>() where TProfile : Profile, new()
        {
            AddProfile(new TProfile());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="constructor"></param>
        public void ConstructServicesUsing(Func<Type, object> constructor)
        {
            _serviceCtor = constructor;
        }

        /// <summary>
        /// 
        /// </summary>
        public void DisableConstructorMapping()
        {
            GetProfile(DefaultProfileName).ConstructorMappingEnabled = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void EnableYieldReturnForDataReaderMapper()
        {
            GetProfile(DefaultProfileName).DataReaderMapperYieldReturnEnabled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Seal()
        {
            foreach (var typeMap in _userDefinedTypeMaps.Values)
            {
                typeMap.Seal();

                var typePair = new TypePair(typeMap.SourceType, typeMap.DestinationType);
                _typeMapPlanCache.AddOrUpdate(typePair, typeMap, (_, __) => typeMap);
                if (typeMap.DestinationTypeOverride != null)
                {
                    var includedDerivedType = new TypePair(typeMap.SourceType, typeMap.DestinationTypeOverride);
                    var derivedMap = FindTypeMapFor(includedDerivedType);
                    if (derivedMap != null)
                    {
                        _typeMapPlanCache.AddOrUpdate(typePair, derivedMap, (_, __) => derivedMap);
                    }
                }
                foreach (var derivedMap in GetDerivedTypeMaps(typeMap))
                {
                    _typeMapPlanCache.AddOrUpdate(new TypePair(derivedMap.SourceType, typeMap.DestinationType),
                        derivedMap, (_, __) => derivedMap);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeMap"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <returns></returns>
        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            return CreateMap<TSource, TDestination>(DefaultProfileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="memberList"></param>
        /// <returns></returns>
        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList)
        {
            return CreateMap<TSource, TDestination>(DefaultProfileName, memberList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="profileName"></param>
        /// <returns></returns>
        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(string profileName)
        {
            return CreateMap<TSource, TDestination>(profileName, MemberList.Destination);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="profileName"></param>
        /// <param name="memberList"></param>
        /// <returns></returns>
        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(string profileName,
            MemberList memberList)
        {
            TypeMap typeMap = CreateTypeMap(typeof (TSource), typeof (TDestination), profileName, memberList);

            return CreateMappingExpression<TSource, TDestination>(typeMap);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public IMappingExpression CreateMap(Type sourceType, Type destinationType)
        {
            return CreateMap(sourceType, destinationType, MemberList.Destination);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <param name="memberList"></param>
        /// <returns></returns>
        public IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList)
        {
            return CreateMap(sourceType, destinationType, memberList, DefaultProfileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <param name="memberList"></param>
        /// <param name="profileName"></param>
        /// <returns></returns>
        public IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList,
            string profileName)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefixes"></param>
        public void RecognizePrefixes(params string[] prefixes)
        {
            GetProfile(DefaultProfileName).RecognizePrefixes(prefixes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postfixes"></param>
        public void RecognizePostfixes(params string[] postfixes)
        {
            GetProfile(DefaultProfileName).RecognizePostfixes(postfixes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="original"></param>
        /// <param name="alias"></param>
        public void RecognizeAlias(string original, string alias)
        {
            GetProfile(DefaultProfileName).RecognizeAlias(original, alias);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="original"></param>
        /// <param name="newValue"></param>
        public void ReplaceMemberName(string original, string newValue)
        {
            GetProfile(DefaultProfileName).ReplaceMemberName(original, newValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefixes"></param>
        public void RecognizeDestinationPrefixes(params string[] prefixes)
        {
            GetProfile(DefaultProfileName).RecognizeDestinationPrefixes(prefixes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postfixes"></param>
        public void RecognizeDestinationPostfixes(params string[] postfixes)
        {
            GetProfile(DefaultProfileName).RecognizeDestinationPostfixes(postfixes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public TypeMap CreateTypeMap(Type source, Type destination)
        {
            return CreateTypeMap(source, destination, DefaultProfileName, MemberList.Destination);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="profileName"></param>
        /// <param name="memberList"></param>
        /// <returns></returns>
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

                OnTypeMapCreated(tm);

                return tm;
            });

            return typeMap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="typeMap"></param>
        private void IncludeBaseMappings(Type source, Type destination, TypeMap typeMap)
        {
            foreach (
                var inheritedTypeMap in
                    _userDefinedTypeMaps.Values.Where(t => t.TypeHasBeenIncluded(source, destination)))
            {
                typeMap.ApplyInheritedMap(inheritedTypeMap);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TypeMap[] GetAllTypeMaps()
        {
            return _userDefinedTypeMaps.Values.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public TypeMap FindTypeMapFor(Type sourceType, Type destinationType)
        {
            var typePair = new TypePair(sourceType, destinationType);

            return FindTypeMapFor(typePair);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typePair"></param>
        /// <returns></returns>
        public TypeMap FindTypeMapFor(TypePair typePair)
        {
            TypeMap typeMap;
            _userDefinedTypeMaps.TryGetValue(typePair, out typeMap);
            return typeMap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public TypeMap ResolveTypeMap(Type sourceType, Type destinationType)
        {
            var typePair = new TypePair(sourceType, destinationType);

            return ResolveTypeMap(typePair);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typePair"></param>
        /// <returns></returns>
        public TypeMap ResolveTypeMap(TypePair typePair)
        {
            var typeMap = _typeMapPlanCache.GetOrAdd(typePair,
                _ =>
                    GetRelatedTypePairs(_)
                        .Select(
                            tp =>
                                FindTypeMapFor(tp) ?? (_typeMapPlanCache.ContainsKey(tp) ? _typeMapPlanCache[tp] : null))
                        .FirstOrDefault(tm => tm != null));

            return typeMap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="sourceType"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public TypeMap ResolveTypeMap(object source, object destination, Type sourceType, Type destinationType)
        {
            return ResolveTypeMap(source?.GetType() ?? sourceType, destination?.GetType() ?? destinationType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resolutionResult"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public TypeMap ResolveTypeMap(ResolutionResult resolutionResult, Type destinationType)
        {
            return ResolveTypeMap(resolutionResult.Value, null, resolutionResult.Type, destinationType) ??
                   ResolveTypeMap(resolutionResult.Value, null, resolutionResult.MemberType, destinationType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool HasOpenGenericTypeMapDefined(ResolutionContext context)
        {
            var sourceGenericDefinition = context.SourceType.GetGenericTypeDefinition();
            var destGenericDefinition = context.DestinationType.GetGenericTypeDefinition();

            var genericTypePair = new TypePair(sourceGenericDefinition, destGenericDefinition);

            return _typeMapExpressionCache.ContainsKey(genericTypePair);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        private IEnumerable<TypePair> GetRelatedTypePairs(TypePair root)
        {
            return from sourceType in GetAllTypes(root.SourceType)
                from destinationType in GetAllTypes(root.DestinationType)
                select new TypePair(sourceType, destinationType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private IEnumerable<Type> GetAllTypes(Type type)
        {
            yield return type;

            if (type.IsValueType() && !type.IsNullableType())
                yield return typeof (Nullable<>).MakeGenericType(type);

            var baseType = type.BaseType();
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileName"></param>
        /// <returns></returns>
        public IProfileConfiguration GetProfileConfiguration(string profileName)
        {
            return GetProfile(profileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeMap"></param>
        public void AssertConfigurationIsValid(TypeMap typeMap)
        {
            AssertConfigurationIsValid(Enumerable.Repeat(typeMap, 1));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileName"></param>
        public void AssertConfigurationIsValid(string profileName)
        {
            AssertConfigurationIsValid(_userDefinedTypeMaps.Values.Where(typeMap => typeMap.Profile == profileName));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TProfile"></typeparam>
        public void AssertConfigurationIsValid<TProfile>()
            where TProfile : Profile, new()
        {
            AssertConfigurationIsValid(new TProfile().ProfileName);
        }

        /// <summary>
        /// 
        /// </summary>
        public void AssertConfigurationIsValid()
        {
            AssertConfigurationIsValid(_userDefinedTypeMaps.Values);
        }

        /// <summary>
        /// 
        /// </summary>
        public IObjectMapperCollection ObjectMappers => _objectMappers;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="typeMap"></param>
        /// <returns></returns>
        private IMappingExpression<TSource, TDestination> CreateMappingExpression<TSource, TDestination>(TypeMap typeMap)
        {
            IMappingExpression<TSource, TDestination> mappingExp =
                new MappingExpression<TSource, TDestination>(typeMap, _serviceCtor, this);

            var destInfo = typeMap.ConfiguredMemberList == MemberList.Destination
                ? new TypeInfo(typeof (TDestination))
                : new TypeInfo(typeof (TSource));

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeMap"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        private IMappingExpression CreateMappingExpression(TypeMap typeMap, Type destinationType)
        {
            IMappingExpression mappingExp = new MappingExpression(typeMap, _serviceCtor);

            TypeInfo destInfo = new TypeInfo(destinationType);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeMaps"></param>
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
                DryRunTypeMap(typeMapsChecked,
                    new ResolutionContext(typeMap, null, typeMap.SourceType, typeMap.DestinationType,
                        new MappingOperationOptions(), _mapperContext));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeMap"></param>
        /// <returns></returns>
        private static bool ShouldCheckMap(TypeMap typeMap)
        {
            return (typeMap.CustomMapper == null && typeMap.CustomProjection == null &&
                    typeMap.DestinationTypeOverride == null) && !FeatureDetector.IsIDataRecordType(typeMap.SourceType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeMapsChecked"></param>
        /// <param name="context"></param>
        private void DryRunTypeMap(ICollection<TypeMap> typeMapsChecked, ResolutionContext context)
        {
            if (context.TypeMap != null)
            {
                typeMapsChecked.Add(context.TypeMap);
            }

            var mapperToUse = ObjectMappers.FirstOrDefault(mapper => mapper.IsMatch(context));

            if (mapperToUse == null && context.SourceType.IsNullableType())
            {
                var nullableContext = context.CreateValueContext(null, Nullable.GetUnderlyingType(context.SourceType));

                mapperToUse = ObjectMappers.FirstOrDefault(mapper => mapper.IsMatch(nullableContext));
            }

            if (mapperToUse == null)
            {
                throw new AutoMapperConfigurationException(context);
            }

            if (mapperToUse is TypeMapMapper)
            {
                // ReSharper disable once PossibleNullReferenceException
                foreach (var propertyMap in context.TypeMap.GetPropertyMaps())
                {
                    if (propertyMap.IsIgnored()) continue;

                    var lastResolver =
                        propertyMap.GetSourceValueResolvers().OfType<IMemberResolver>().LastOrDefault();

                    if (lastResolver == null) continue;

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
            else if (mapperToUse is ArrayMapper || mapperToUse is EnumerableMapper || mapperToUse is CollectionMapper)
            {
                var sourceElementType = context.SourceType.GetNullEnumerableElementType();
                var destElementType = context.DestinationType.GetNullEnumerableElementType();
                var itemTypeMap = ((IConfigurationProvider) this).ResolveTypeMap(sourceElementType, destElementType);

                if (typeMapsChecked.Any(typeMap => Equals(typeMap, itemTypeMap)))
                    return;

                var memberContext = context.CreateElementContext(itemTypeMap, null, sourceElementType, destElementType,
                    0);

                DryRunTypeMap(typeMapsChecked, memberContext);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeMap"></param>
        protected void OnTypeMapCreated(TypeMap typeMap)
        {
            TypeMapCreated?.Invoke(this, new TypeMapCreatedEventArgs(typeMap));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileName"></param>
        /// <returns></returns>
        internal ProfileConfiguration GetProfile(string profileName)
        {
            var expr = _formatterProfiles.GetOrAdd(profileName,
                name => new ProfileConfiguration());

            return expr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startingwith"></param>
        public void AddGlobalIgnore(string startingwith)
        {
            _globalIgnore.Add(startingwith);
        }
    }
}