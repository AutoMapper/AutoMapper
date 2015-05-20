using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper.Impl;
using AutoMapper.Mappers;

namespace AutoMapper
{
    using Configuration;
    using Internal;

    public class ConfigurationStore : IConfigurationProvider, IConfiguration
    {
        private static readonly IDictionaryFactory DictionaryFactory = PlatformAdapter.Resolve<IDictionaryFactory>();
        private readonly ITypeMapFactory _typeMapFactory;
        private readonly IEnumerable<IObjectMapper> _mappers;
        internal const string DefaultProfileName = "";

        private readonly IDictionary<TypePair, TypeMap> _typeMaps = DictionaryFactory.CreateDictionary<TypePair, TypeMap>();
        private readonly IDictionary<TypePair, TypeMap> _typeMapCache = DictionaryFactory.CreateDictionary<TypePair, TypeMap>();
        private readonly IDictionary<TypePair, CreateTypeMapExpression> _typeMapExpressionCache = DictionaryFactory.CreateDictionary<TypePair, CreateTypeMapExpression>();
        private readonly IDictionary<string, ProfileConfiguration> _formatterProfiles = DictionaryFactory.CreateDictionary<string, ProfileConfiguration>();
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
            foreach (var typeMap in _typeMaps.Values)
            {
                typeMap.Seal();

                var typePair = new TypePair(typeMap.SourceType, typeMap.DestinationType);
                _typeMapCache.AddOrUpdate(typePair, typeMap, (_, _2) => typeMap);
                if (typeMap.DestinationTypeOverride != null)
                {
                    var includedDerivedType = new TypePair(typeMap.SourceType, typeMap.DestinationTypeOverride);
                    var derivedMap = FindTypeMapFor(includedDerivedType);
                    if (derivedMap != null)
                    {
                        _typeMapCache.AddOrUpdate(typePair, derivedMap, (_, _2) => derivedMap);
                    }
                }
                foreach (var derivedMap in GetDerivedTypeMaps(typeMap))
                {
                    _typeMapCache.AddOrUpdate(new TypePair(derivedMap.SourceType, typeMap.DestinationType), derivedMap, (_, _2) => derivedMap);
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
            if (sourceType.IsGenericTypeDefinition() && destinationType.IsGenericTypeDefinition())
            {
                var typePair = new TypePair(sourceType, destinationType);

                var expression = _typeMapExpressionCache.GetOrAdd(typePair, tp => new CreateTypeMapExpression(tp, memberList, profileName));

                return expression;
            }

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
            var typePair = new TypePair(source, destination);
            var typeMap = _typeMaps.GetOrAdd(typePair, tp =>
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

        private void IncludeBaseMappings(Type source, Type destination, TypeMap typeMap)
        {
            foreach (var inheritedTypeMap in _typeMaps.Values.Where(t => t.TypeHasBeenIncluded(source, destination)))
            {
                typeMap.ApplyInheritedMap(inheritedTypeMap);
            }
        }

        public TypeMap[] GetAllTypeMaps()
        {
            return _typeMaps.Values.ToArray();
        }

        public TypeMap FindTypeMapFor(Type sourceType, Type destinationType)
        {
            var typePair = new TypePair(sourceType, destinationType);

            var typeMap = _typeMapCache.GetOrAdd(typePair, _ => GetRelatedTypePairs(_).Select(FindTypeMapFor).FirstOrDefault(tm => tm != null));

            return typeMap;
        }

        public TypeMap FindTypeMapFor(object source, object destination, Type sourceType, Type destinationType)
        {
            return FindTypeMapFor(source?.GetType() ?? sourceType, destination?.GetType() ?? destinationType);
        }

        public TypeMap FindTypeMapFor(TypePair typePair)
        {
            TypeMap typeMap;
            _typeMaps.TryGetValue(typePair, out typeMap);
            return typeMap;
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

            var typeMap = CreateTypeMap(closedGenericTypePair.SourceType, closedGenericTypePair.DestinationType, genericTypeMapExpression.ProfileName,
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
            return
                from sourceType in GetAllTypes(root.SourceType)
                from destinationType in GetAllTypes(root.DestinationType)
                select new TypePair(sourceType, destinationType);
        }

        private IEnumerable<Type> GetAllTypes(Type type)
        {
            yield return type;

            if (type.IsValueType() && !type.IsNullableType())
                yield return typeof(Nullable<>).MakeGenericType(type);

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

        public TypeMap FindTypeMapFor(ResolutionResult resolutionResult, Type destinationType)
        {
            return FindTypeMapFor(resolutionResult.Value, null, resolutionResult.Type, destinationType) ??
                   FindTypeMapFor(resolutionResult.Value, null, resolutionResult.MemberType, destinationType);
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
            AssertConfigurationIsValid(_typeMaps.Values.Where(typeMap => typeMap.Profile == profileName));
        }

        public void AssertConfigurationIsValid<TProfile>()
            where TProfile : Profile, new()
        {
            AssertConfigurationIsValid(new TProfile().ProfileName);
        }

        public void AssertConfigurationIsValid()
        {
            AssertConfigurationIsValid(_typeMaps.Values);
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
                ? new TypeInfo(typeof(TDestination))
                : new TypeInfo(typeof(TSource));

            foreach (var destProperty in destInfo.GetPublicWriteAccessors())
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
            IMappingExpression mappingExp = new MappingExpression(typeMap, _serviceCtor);

            TypeInfo destInfo = new TypeInfo(destinationType);
            foreach (var destProperty in destInfo.GetPublicWriteAccessors())
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
            return (typeMap.CustomMapper == null && typeMap.CustomProjection == null && typeMap.DestinationTypeOverride == null) && !FeatureDetector.IsIDataRecordType(typeMap.SourceType);
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
                TypeMap itemTypeMap = ((IConfigurationProvider)this).FindTypeMapFor(sourceElementType, destElementType);

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
