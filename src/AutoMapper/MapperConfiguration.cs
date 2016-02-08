namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using Configuration;
    using Configuration.Conventions;
    using Mappers;
    using QueryableExtensions;
    using QueryableExtensions.Impl;

    public class MapperConfiguration : IConfigurationProvider, IMapperConfiguration
    {
        private readonly IEnumerable<IObjectMapper> _mappers;
        private readonly IEnumerable<ITypeMapObjectMapper> _typeMapObjectMappers;
        private readonly List<Action<TypeMap, IMappingExpression>> _allTypeMapActions = new List<Action<TypeMap, IMappingExpression>>();
        private readonly Profile _defaultProfile;
        private readonly TypeMapRegistry _typeMapRegistry = new TypeMapRegistry();
        private readonly ConcurrentDictionary<TypePair, TypeMap> _typeMapPlanCache = new ConcurrentDictionary<TypePair, TypeMap>();
        private readonly ConcurrentBag<Profile> _profiles = new ConcurrentBag<Profile>();
        private readonly ConfigurationValidator _validator;
        private Func<Type, object> _serviceCtor = ObjectCreator.CreateObject;


        public MapperConfiguration(Action<IMapperConfiguration> configure) : this(configure, MapperRegistry.Mappers, TypeMapObjectMapperRegistry.Mappers)
        {
        }

        public MapperConfiguration(Action<IMapperConfiguration> configure, IEnumerable<IObjectMapper> mappers, IEnumerable<ITypeMapObjectMapper> typeMapObjectMappers) 
        {
            _mappers = mappers;
            _typeMapObjectMappers = typeMapObjectMappers;
            var profileExpression = new NamedProfile(ProfileName);

            _profiles.Add(profileExpression);

            _defaultProfile = profileExpression;

            _validator = new ConfigurationValidator(this);

            configure(this);

            Seal();

            ExpressionBuilder = new ExpressionBuilder(this);
        }

        public string ProfileName => "";

        #region IConfiguration Members

        void IConfiguration.CreateProfile(string profileName, Action<Profile> config)
        {
            var profile = new NamedProfile(profileName);

            config(profile);

            ((IConfiguration) this).AddProfile(profile);
        }

        private class NamedProfile : Profile
        {
            public NamedProfile(string profileName) : base(profileName) { }
        }

        void IConfiguration.AddProfile(Profile profile)
        {
            profile.Initialize();
            _profiles.Add(profile);
        }

        void IConfiguration.AddProfile<TProfile>() => ((IConfiguration)this).AddProfile(new TProfile());

        void IConfiguration.AddProfile(Type profileType) => ((IConfiguration)this).AddProfile((Profile)Activator.CreateInstance(profileType));

        void IConfiguration.ConstructServicesUsing(Func<Type, object> constructor) => _serviceCtor = constructor;

        Func<PropertyInfo, bool> IProfileExpression.ShouldMapProperty
        {
            get { return _defaultProfile.ShouldMapProperty; }
            set { _defaultProfile.ShouldMapProperty = value; }
        }

        Func<FieldInfo, bool> IProfileExpression.ShouldMapField
        {
            get { return _defaultProfile.ShouldMapField; }
            set { _defaultProfile.ShouldMapField = value; }
        }

        bool IConfiguration.CreateMissingTypeMaps
        {
            get { return _defaultProfile.CreateMissingTypeMaps; }
            set { _defaultProfile.CreateMissingTypeMaps = value; }
        }

        void IProfileExpression.IncludeSourceExtensionMethods(Assembly assembly)
        {
            _defaultProfile.IncludeSourceExtensionMethods(assembly);
        }

        INamingConvention IProfileExpression.SourceMemberNamingConvention
        {
            get { return _defaultProfile.SourceMemberNamingConvention; }
            set { _defaultProfile.SourceMemberNamingConvention = value; }
        }

        INamingConvention IProfileExpression.DestinationMemberNamingConvention
        {
            get { return _defaultProfile.DestinationMemberNamingConvention; }
            set { _defaultProfile.DestinationMemberNamingConvention = value; }
        }

        bool IProfileExpression.AllowNullDestinationValues
        {
            get { return AllowNullDestinationValues; }
            set { AllowNullDestinationValues = value; }
        }

        bool IProfileExpression.AllowNullCollections
        {
            get { return AllowNullCollections; }
            set { AllowNullCollections = value; }
        }

        void IProfileExpression.ForAllMaps(Action<TypeMap, IMappingExpression> configuration) => _allTypeMapActions.Add(configuration);

        IMemberConfiguration IProfileExpression.AddMemberConfiguration() => _defaultProfile.AddMemberConfiguration();

        IConditionalObjectMapper IProfileExpression.AddConditionalObjectMapper() => _defaultProfile.AddConditionalObjectMapper();

        void IProfileExpression.DisableConstructorMapping() => _defaultProfile.DisableConstructorMapping();

        IMappingExpression<TSource, TDestination> IProfileExpression.CreateMap<TSource, TDestination>() 
            => _defaultProfile.CreateMap<TSource, TDestination>();

        IMappingExpression<TSource, TDestination> IProfileExpression.CreateMap<TSource, TDestination>(MemberList memberList)
            => _defaultProfile.CreateMap<TSource, TDestination>(memberList);

        IMappingExpression IProfileExpression.CreateMap(Type sourceType, Type destinationType)
            => _defaultProfile.CreateMap(sourceType, destinationType, MemberList.Destination);

        IMappingExpression IProfileExpression.CreateMap(Type sourceType, Type destinationType, MemberList memberList)
            => _defaultProfile.CreateMap(sourceType, destinationType, memberList);

        void IProfileExpression.ClearPrefixes() => _defaultProfile.ClearPrefixes();

        void IProfileExpression.RecognizeAlias(string original, string alias) => _defaultProfile.RecognizeAlias(original, alias);

        void IProfileExpression.ReplaceMemberName(string original, string newValue) => _defaultProfile.ReplaceMemberName(original, newValue);

        void IProfileExpression.RecognizePrefixes(params string[] prefixes) => _defaultProfile.RecognizePrefixes(prefixes);

        void IProfileExpression.RecognizePostfixes(params string[] postfixes) => _defaultProfile.RecognizePostfixes(postfixes);

        void IProfileExpression.RecognizeDestinationPrefixes(params string[] prefixes) => _defaultProfile.RecognizeDestinationPrefixes(prefixes);

        void IProfileExpression.RecognizeDestinationPostfixes(params string[] postfixes) => _defaultProfile.RecognizeDestinationPostfixes(postfixes);

        void IProfileExpression.AddGlobalIgnore(string startingwith) => _defaultProfile.AddGlobalIgnore(startingwith);

        #endregion

        #region IConfigurationProvider members

        public IExpressionBuilder ExpressionBuilder { get; }

        public Func<Type, object> ServiceCtor => _serviceCtor;

        public bool AllowNullDestinationValues
        {
            get { return _defaultProfile.AllowNullDestinationValues; }
            private set { _defaultProfile.AllowNullDestinationValues = value; }
        }

        public bool AllowNullCollections
        {
            get { return _defaultProfile.AllowNullCollections; }
            private set { _defaultProfile.AllowNullCollections = value; }
        }

        public TypeMap[] GetAllTypeMaps() => _typeMapRegistry.TypeMaps.ToArray();

        public TypeMap FindTypeMapFor(Type sourceType, Type destinationType) => FindTypeMapFor(new TypePair(sourceType, destinationType));

        public TypeMap FindTypeMapFor<TSource, TDestination>() => FindTypeMapFor(new TypePair(typeof(TSource), typeof(TDestination)));

        public TypeMap FindTypeMapFor(TypePair typePair) => _typeMapRegistry.GetTypeMap(typePair);

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
            return ResolveTypeMap(source?.GetType() ?? sourceType, destination?.GetType() ?? destinationType);
        }

        public TypeMap ResolveTypeMap(ResolutionResult resolutionResult, Type destinationType)
        {
            return ResolveTypeMap(resolutionResult.Type, destinationType) ?? ResolveTypeMap(resolutionResult.MemberType, destinationType);
        }

        public void AssertConfigurationIsValid(TypeMap typeMap)
        {
            _validator.AssertConfigurationIsValid(Enumerable.Repeat(typeMap, 1));
        }

        public void AssertConfigurationIsValid(string profileName)
        {
            _validator.AssertConfigurationIsValid(_typeMapRegistry.TypeMaps.Where(typeMap => typeMap.Profile.ProfileName == profileName));
        }

        public void AssertConfigurationIsValid<TProfile>()
            where TProfile : Profile, new()
        {
            AssertConfigurationIsValid(new TProfile().ProfileName);
        }

        public void AssertConfigurationIsValid()
        {
            _validator.AssertConfigurationIsValid(_typeMapRegistry.TypeMaps);
        }

        public IMapper CreateMapper() => new Mapper(this);

        public IMapper CreateMapper(Func<Type, object> serviceCtor) => new Mapper(this, serviceCtor);

        public IEnumerable<IObjectMapper> GetMappers() => _mappers;

        public IEnumerable<ITypeMapObjectMapper> GetTypeMapMappers() => _typeMapObjectMappers;

        #endregion

        private void Seal()
        {
            var derivedMaps = new List<Tuple<TypePair, TypeMap>>();
            var redirectedTypes = new List<Tuple<TypePair, TypePair>>();

            foreach (var profile in _profiles.Cast<IProfileConfiguration>())
            {
                profile.Register(_typeMapRegistry);
            }

            foreach (var action in _allTypeMapActions)
            {
                foreach (var typeMap in _typeMapRegistry.TypeMaps)
                {
                    var expression = new MappingExpression(typeMap.Types, typeMap.ConfiguredMemberList);

                    action(typeMap, expression);

                    expression.Configure(typeMap.Profile, typeMap);
                }
            }

            foreach (var profile in _profiles.Cast<IProfileConfiguration>())
            {
                profile.Configure(_typeMapRegistry);
            }

            foreach (var typeMap in _typeMapRegistry.TypeMaps)
            {
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

            foreach (var typeMap in _typeMapRegistry.TypeMaps)
            {
                typeMap.Seal();
            }
        }


        private IEnumerable<TypeMap> GetDerivedTypeMaps(TypeMap typeMap)
        {
            foreach (var derivedTypes in typeMap.IncludedDerivedTypes)
            {
                var derivedMap = FindTypeMapFor(derivedTypes);
                if(derivedMap == null)
                {
                    throw QueryMapperHelper.MissingMapException(derivedTypes.SourceType, derivedTypes.DestinationType);
                }
                yield return derivedMap;
                foreach(var derivedTypeMap in GetDerivedTypeMaps(derivedMap))
                {
                    yield return derivedTypeMap;
                }
            }
        }

        private bool CoveredByObjectMap(TypePair typePair)
        {
            return GetMappers().FirstOrDefault(m => m.IsMatch(typePair)) != null;
        }

        private TypeMap FindConventionTypeMapFor(TypePair typePair)
        {
            var typeMap = _profiles
                .Cast<IProfileConfiguration>()
                .Select(p => p.ConfigureConventionTypeMap(_typeMapRegistry, typePair))
                .FirstOrDefault(t => t != null);

            return typeMap;
        }

        private TypeMap FindClosedGenericTypeMapFor(TypePair typePair)
        {
            var openGenericTypes = GetOpenGenericTypePair(typePair);
            if (openGenericTypes == null)
                return null;

            var typeMap = _profiles
                .Cast<IProfileConfiguration>()
                .Select(p => p.ConfigureClosedGenericTypeMap(_typeMapRegistry, typePair, openGenericTypes))
                .FirstOrDefault(t => t != null);

            return typeMap;
        }

        private TypePair GetOpenGenericTypePair(TypePair typePair)
        {
            var isGeneric = typePair.SourceType.IsGenericType()
                            && typePair.DestinationType.IsGenericType()
                            && (typePair.SourceType.GetGenericTypeDefinition() != null)
                            && (typePair.DestinationType.GetGenericTypeDefinition() != null);
            if (!isGeneric)
                return null;

            var sourceGenericDefinition = typePair.SourceType.GetGenericTypeDefinition();
            var destGenericDefinition = typePair.DestinationType.GetGenericTypeDefinition();

            var genericTypePair = new TypePair(sourceGenericDefinition, destGenericDefinition);

            return genericTypePair;
        }

        private IEnumerable<TypePair> GetRelatedTypePairs(TypePair root)
        {
            var subTypePairs =
                from destinationType in GetAllTypes(root.DestinationType)
                from sourceType in GetAllTypes(root.SourceType)
                select new TypePair(sourceType, destinationType);
            return subTypePairs;
        }

        private IEnumerable<Type> GetAllTypes(Type type)
        {
            yield return type;
            foreach(var interfaceType in GetInterfaces(type))
            {
                yield return interfaceType;
            }
            Type baseType = type.BaseType();
            while (baseType != null)
            {
                yield return baseType;
                foreach(var interfaceType in GetInterfaces(baseType))
                {
                    yield return interfaceType;
                }
                baseType = baseType.BaseType();
            }
        }

        private static IEnumerable<Type> GetInterfaces(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            var interfaces = typeInfo.ImplementedInterfaces;
            if(!typeInfo.IsArray && !typeInfo.IsInterface)
            {
                interfaces = interfaces.Where(i => typeInfo.GetRuntimeInterfaceMap(i).TargetMethods.Any(m => m.DeclaringType == type));
            }
            foreach(var interfaceType in interfaces)
            {
                yield return interfaceType;
            }
        }

    }
}
