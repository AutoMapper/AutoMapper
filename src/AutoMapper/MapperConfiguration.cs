namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Configuration;
    using Mappers;
    using QueryableExtensions;
    using QueryableExtensions.Impl;
    using static System.Linq.Expressions.Expression;
    using static ExpressionExtensions;
    using UntypedMapperFunc = System.Func<object, object, ResolutionContext, object>;


    public class MapperConfiguration : IConfigurationProvider
    {
        private readonly IEnumerable<IObjectMapper> _mappers;
        private readonly TypeMapRegistry _typeMapRegistry = new TypeMapRegistry();
        private readonly ConcurrentDictionary<TypePair, TypeMap> _typeMapPlanCache = new ConcurrentDictionary<TypePair, TypeMap>();
        private readonly ConcurrentDictionary<MapRequest, MapperFuncs> _mapPlanCache = new ConcurrentDictionary<MapRequest, MapperFuncs>();
        private readonly ConfigurationValidator _validator;
        private readonly Func<TypePair, TypeMap> _getTypeMap;
        private readonly Func<MapRequest, MapperFuncs> _createMapperFuncs;
        private readonly ConfigurationExpression _configurationExpression;

        public MapperConfiguration(Action<IMapperConfiguration> configure) : this(configure, MapperRegistry.Mappers)
        {
        }

        public MapperConfiguration(Action<IMapperConfiguration> configure, IEnumerable<IObjectMapper> mappers)
        {
            _mappers = mappers;
            _getTypeMap = GetTypeMap;
            _createMapperFuncs = CreateMapperFuncs;

            _validator = new ConfigurationValidator(this);
            ExpressionBuilder = new ExpressionBuilder(this);

            _configurationExpression = new ConfigurationExpression();

            configure(_configurationExpression);

            Seal(_configurationExpression);
        }

        #region IConfiguration Members

        private class ConfigurationExpression : IMapperConfiguration
        {
            private readonly Profile _defaultProfile;
            private readonly IList<Profile> _profiles = new List<Profile>();
            private readonly List<Action<TypeMap, IMappingExpression>> _allTypeMapActions = new List<Action<TypeMap, IMappingExpression>>();

            public ConfigurationExpression()
            {
                _defaultProfile = new NamedProfile(ProfileName);
                _profiles.Add(_defaultProfile);

            }

            public string ProfileName => "";
            public IEnumerable<Profile> Profiles => _profiles;
            public IEnumerable<Action<TypeMap, IMappingExpression>> AllTypeMapActions => _allTypeMapActions;
            public Func<Type, object> ServiceCtor { get; private set; } = ObjectCreator.CreateObject;

            void IConfiguration.CreateProfile(string profileName, Action<Profile> config)
            {
                var profile = new NamedProfile(profileName);

                config(profile);

                ((IConfiguration)this).AddProfile(profile);
            }

            private class NamedProfile : Profile
            {
                public NamedProfile(string profileName) : base(profileName)
                {
                }
            }

            void IConfiguration.AddProfile(Profile profile)
            {
                profile.Initialize();
                _profiles.Add(profile);
            }

            void IConfiguration.AddProfile<TProfile>() => ((IConfiguration)this).AddProfile(new TProfile());

            void IConfiguration.AddProfile(Type profileType)
                => ((IConfiguration)this).AddProfile((Profile)Activator.CreateInstance(profileType));

            void IConfiguration.ConstructServicesUsing(Func<Type, object> constructor) => ServiceCtor = constructor;

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

            void IProfileExpression.IncludeSourceExtensionMethods(Type type)
            {
                _defaultProfile.IncludeSourceExtensionMethods(type);
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

            bool IProfileExpression.AllowNullDestinationValues { get; set; }

            bool IProfileExpression.AllowNullCollections { get; set; }

            void IProfileExpression.ForAllMaps(Action<TypeMap, IMappingExpression> configuration)
                => _allTypeMapActions.Add(configuration);

            Configuration.Conventions.IMemberConfiguration IProfileExpression.AddMemberConfiguration()
                => _defaultProfile.AddMemberConfiguration();

            IConditionalObjectMapper IProfileExpression.AddConditionalObjectMapper()
                => _defaultProfile.AddConditionalObjectMapper();

            void IProfileExpression.DisableConstructorMapping() => _defaultProfile.DisableConstructorMapping();

            IMappingExpression<TSource, TDestination> IProfileExpression.CreateMap<TSource, TDestination>()
                => _defaultProfile.CreateMap<TSource, TDestination>();

            IMappingExpression<TSource, TDestination> IProfileExpression.CreateMap<TSource, TDestination>(
                MemberList memberList)
                => _defaultProfile.CreateMap<TSource, TDestination>(memberList);

            IMappingExpression IProfileExpression.CreateMap(Type sourceType, Type destinationType)
                => _defaultProfile.CreateMap(sourceType, destinationType, MemberList.Destination);

            IMappingExpression IProfileExpression.CreateMap(Type sourceType, Type destinationType, MemberList memberList)
                => _defaultProfile.CreateMap(sourceType, destinationType, memberList);

            void IProfileExpression.ClearPrefixes() => _defaultProfile.ClearPrefixes();

            void IProfileExpression.RecognizeAlias(string original, string alias)
                => _defaultProfile.RecognizeAlias(original, alias);

            void IProfileExpression.ReplaceMemberName(string original, string newValue)
                => _defaultProfile.ReplaceMemberName(original, newValue);

            void IProfileExpression.RecognizePrefixes(params string[] prefixes)
                => _defaultProfile.RecognizePrefixes(prefixes);

            void IProfileExpression.RecognizePostfixes(params string[] postfixes)
                => _defaultProfile.RecognizePostfixes(postfixes);

            void IProfileExpression.RecognizeDestinationPrefixes(params string[] prefixes)
                => _defaultProfile.RecognizeDestinationPrefixes(prefixes);

            void IProfileExpression.RecognizeDestinationPostfixes(params string[] postfixes)
                => _defaultProfile.RecognizeDestinationPostfixes(postfixes);

            void IProfileExpression.AddGlobalIgnore(string startingwith)
                => _defaultProfile.AddGlobalIgnore(startingwith);

        }

        #endregion

        #region IConfigurationProvider members

        public IExpressionBuilder ExpressionBuilder { get; }

        public Func<Type, object> ServiceCtor { get; private set; }

        public bool AllowNullDestinationValues { get; private set; }

        public bool AllowNullCollections { get; private set; }

        public Func<TSource, TDestination, ResolutionContext, TDestination> GetMapperFunc<TSource, TDestination>(TypePair types)
        {
            var key = new TypePair(typeof(TSource), typeof(TDestination));
            var mapRequest = new MapRequest(key, types);
            return (Func<TSource, TDestination, ResolutionContext, TDestination>)GetMapperFunc(mapRequest);
        }

        public Delegate GetMapperFunc(MapRequest mapRequest)
        {
            return _mapPlanCache.GetOrAdd(mapRequest, _createMapperFuncs).Typed;
        }

        public UntypedMapperFunc GetUntypedMapperFunc(MapRequest mapRequest)
        {
            return _mapPlanCache.GetOrAdd(mapRequest, _createMapperFuncs).Untyped;
        }

        private MapperFuncs CreateMapperFuncs(MapRequest mapRequest)
        {
            var typeMap = ResolveTypeMap(mapRequest.RuntimeTypes);
            if (typeMap != null)
            {
                return new MapperFuncs(mapRequest, typeMap);
            }
            var mapperToUse = _mappers.FirstOrDefault(om => om.IsMatch(mapRequest.RuntimeTypes));
            return new MapperFuncs(mapRequest, mapperToUse);
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
            var typeMap = _typeMapPlanCache.GetOrAdd(typePair, _getTypeMap);
            return typeMap;
        }

        private TypeMap GetTypeMap(TypePair pair)
        {
            foreach (var tp in pair.GetRelatedTypePairs())
            {
                var typeMap =
                          _typeMapPlanCache.GetOrDefault(tp) ??
                          FindTypeMapFor(tp) ??
                          (!CoveredByObjectMap(pair) ? FindConventionTypeMapFor(tp) : null) ??
                          FindClosedGenericTypeMapFor(tp, pair);
                if (typeMap != null)
                {
                    return typeMap;
                }
            }
            return null;
        }

        public TypeMap ResolveTypeMap(object source, object destination, Type sourceType, Type destinationType)
        {
            return ResolveTypeMap(source?.GetType() ?? sourceType, destination?.GetType() ?? destinationType)
                ?? ResolveTypeMap(sourceType, destinationType);
        }

        public TypeMap ResolveTypeMap(Type sourceRuntimeType, Type sourceDeclaredType, Type destinationType)
        {
            return ResolveTypeMap(sourceRuntimeType, destinationType) ??
                      (sourceDeclaredType != sourceRuntimeType ? ResolveTypeMap(sourceDeclaredType, destinationType) : null);
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
            _validator.AssertConfigurationIsValid(_typeMapRegistry.TypeMaps.Where(tm => !tm.SourceType.IsGenericTypeDefinition() && !tm.DestinationType.IsGenericTypeDefinition()));
        }

        public IMapper CreateMapper() => new Mapper(this);

        public IMapper CreateMapper(Func<Type, object> serviceCtor) => new Mapper(this, serviceCtor);

        public IEnumerable<IObjectMapper> GetMappers() => _mappers;

        #endregion

        private void Seal(IMapperConfiguration configuration)
        {
            ServiceCtor = configuration.ServiceCtor;
            AllowNullDestinationValues = configuration.AllowNullDestinationValues;
            AllowNullCollections = configuration.AllowNullCollections;

            var derivedMaps = new List<Tuple<TypePair, TypeMap>>();
            var redirectedTypes = new List<Tuple<TypePair, TypePair>>();

            foreach (var profile in configuration.Profiles.Cast<IProfileConfiguration>())
            {
                profile.Register(_typeMapRegistry);
            }

            foreach (var action in configuration.AllTypeMapActions)
            {
                foreach (var typeMap in _typeMapRegistry.TypeMaps)
                {
                    var expression = new MappingExpression(typeMap.Types, typeMap.ConfiguredMemberList);

                    action(typeMap, expression);

                    expression.Configure(typeMap.Profile, typeMap);
                }
            }

            foreach (var profile in configuration.Profiles.Cast<IProfileConfiguration>())
            {
                profile.Configure(_typeMapRegistry);
            }

            foreach (var typeMap in _typeMapRegistry.TypeMaps)
            {
                _typeMapPlanCache[typeMap.Types] = typeMap;

                if (typeMap.DestinationTypeOverride != null)
                {
                    redirectedTypes.Add(Tuple.Create(typeMap.Types, new TypePair(typeMap.SourceType, typeMap.DestinationTypeOverride)));
                }
                if (typeMap.SourceType.IsNullableType())
                {
                    var nonNullableTypes = new TypePair(Nullable.GetUnderlyingType(typeMap.SourceType), typeMap.DestinationType);
                    redirectedTypes.Add(Tuple.Create(nonNullableTypes, typeMap.Types));
                }
                derivedMaps.AddRange(GetDerivedTypeMaps(typeMap).Select(derivedMap => Tuple.Create(new TypePair(derivedMap.SourceType, typeMap.DestinationType), derivedMap)));
            }
            foreach (var redirectedType in redirectedTypes)
            {
                var derivedMap = FindTypeMapFor(redirectedType.Item2);
                if (derivedMap != null)
                {
                    _typeMapPlanCache[redirectedType.Item1] = derivedMap;
                }
            }
            foreach (var derivedMap in derivedMaps.Where(derivedMap => !_typeMapPlanCache.ContainsKey(derivedMap.Item1)))
            {
                _typeMapPlanCache[derivedMap.Item1] = derivedMap.Item2;
            }

            foreach (var typeMap in _typeMapRegistry.TypeMaps)
            {
                typeMap.Seal(_typeMapRegistry, this);
            }
        }


        private IEnumerable<TypeMap> GetDerivedTypeMaps(TypeMap typeMap)
        {
            foreach (var derivedTypes in typeMap.IncludedDerivedTypes)
            {
                var derivedMap = FindTypeMapFor(derivedTypes);
                if (derivedMap == null)
                {
                    throw QueryMapperHelper.MissingMapException(derivedTypes.SourceType, derivedTypes.DestinationType);
                }
                yield return derivedMap;
                foreach (var derivedTypeMap in GetDerivedTypeMaps(derivedMap))
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
            var typeMap = _configurationExpression.Profiles
                .Cast<IProfileConfiguration>()
                .Select(p => p.ConfigureConventionTypeMap(_typeMapRegistry, typePair))
                .FirstOrDefault(t => t != null);

            typeMap?.Seal(_typeMapRegistry, this);

            return typeMap;
        }

        private TypeMap FindClosedGenericTypeMapFor(TypePair typePair, TypePair requestedTypes)
        {
            if (typePair.GetOpenGenericTypePair() == null)
                return null;

            var typeMap = _configurationExpression.Profiles
                .Cast<IProfileConfiguration>()
                .Select(p => p.ConfigureClosedGenericTypeMap(_typeMapRegistry, typePair, requestedTypes))
                .FirstOrDefault(t => t != null);

            typeMap?.Seal(_typeMapRegistry, this);

            return typeMap;
        }

        struct MapperFuncs
        {
            private Lazy<UntypedMapperFunc> _untyped;

            public Delegate Typed { get; }

            public UntypedMapperFunc Untyped => _untyped.Value;

            public MapperFuncs(MapRequest mapRequest, TypeMap typeMap) : this(mapRequest, GenerateTypeMapExpression(mapRequest, typeMap))
            {
            }

            public MapperFuncs(MapRequest mapRequest, IObjectMapper mapperToUse) : this(mapRequest, GenerateObjectMapperExpression(mapRequest, mapperToUse))
            {
            }

            public MapperFuncs(MapRequest mapRequest, LambdaExpression typedExpression)
            {
                Typed = typedExpression.Compile();
                _untyped = new Lazy<UntypedMapperFunc>(() => Wrap(mapRequest, typedExpression).Compile());
            }

            private static Expression<UntypedMapperFunc> Wrap(MapRequest mapRequest, LambdaExpression typedExpression)
            {
                var sourceParameter = Parameter(typeof(object), "src");
                var destinationParameter = Parameter(typeof(object), "dest");
                var contextParameter = Parameter(typeof(ResolutionContext), "ctxt");
                var requestedSourceType = mapRequest.RequestedTypes.SourceType;
                var requestedDestinationType = mapRequest.RequestedTypes.DestinationType;

                var destination = requestedDestinationType.IsValueType() ? Coalesce(destinationParameter, New(requestedDestinationType)) : (Expression)destinationParameter;
                return Lambda<UntypedMapperFunc>(
                            ToType(
                                Invoke(typedExpression, ToType(sourceParameter, requestedSourceType), ToType(destination, requestedDestinationType), contextParameter)
                                , typeof(object)),
                          sourceParameter, destinationParameter, contextParameter);
            }

            private static LambdaExpression GenerateTypeMapExpression(MapRequest mapRequest, TypeMap typeMap)
            {
                var mapExpression = typeMap.MapExpression;
                var typeMapSourceParameter = mapExpression.Parameters[0];
                var typeMapDestinationParameter = mapExpression.Parameters[1];
                var requestedSourceType = mapRequest.RequestedTypes.SourceType;
                var requestedDestinationType = mapRequest.RequestedTypes.DestinationType;

                if (typeMapSourceParameter.Type != requestedSourceType || typeMapDestinationParameter.Type != requestedDestinationType)
                {
                    var requestedSourceParameter = Parameter(requestedSourceType, "src");
                    var requestedDestinationParameter = Parameter(requestedDestinationType, "dest");
                    var contextParameter = Parameter(typeof(ResolutionContext), "ctxt");

                    mapExpression = Lambda(ToType(Invoke(typeMap.MapExpression,
                        ToType(requestedSourceParameter, typeMapSourceParameter.Type),
                        ToType(requestedDestinationParameter, typeMapDestinationParameter.Type),
                        contextParameter
                        ), mapRequest.RuntimeTypes.DestinationType),
                        requestedSourceParameter, requestedDestinationParameter, contextParameter);
                }

                return mapExpression;
            }

            private static LambdaExpression GenerateObjectMapperExpression(MapRequest mapRequest, IObjectMapper mapperToUse)
            {
                var destinationType = mapRequest.RequestedTypes.DestinationType;

                var source = Parameter(mapRequest.RequestedTypes.SourceType, "source");
                var destination = Parameter(destinationType, "destination");
                var context = Parameter(typeof(ResolutionContext), "context");

                var ctor = (from c in typeof(AutoMapperMappingException).GetConstructors()
                            let parameters = c.GetParameters()
                            where parameters.Length == 2 && parameters[0].ParameterType == typeof(ResolutionContext) && parameters[1].ParameterType == typeof(string)
                            select c).Single();

                LambdaExpression fullExpression;
                if (mapperToUse == null)
                {
                    var message = Constant("Missing type map configuration or unsupported mapping.");
                    fullExpression = Lambda(Block(Throw(New(ctor, context, message)), Default(destinationType)), source, destination, context);
                }
                else
                {
                    var map = Call(Constant(mapperToUse), "Map", new Type[0], context);
                    var mapToDestination = Lambda(ToType(map, destinationType), context);
                    fullExpression = TryCatch(mapToDestination, source, destination, context);
                }
                return fullExpression;
            }

            private static LambdaExpression TryCatch(LambdaExpression mapExpression, ParameterExpression source, ParameterExpression destination, ParameterExpression context)
            {
                var autoMapException = Parameter(typeof(AutoMapperMappingException), "ex");
                var exception = Parameter(typeof(Exception), "ex");

                var mappingExceptionCtor =
                    (from c in typeof(AutoMapperMappingException).GetConstructors()
                     let parameters = c.GetParameters()
                     where parameters.Length == 2 && parameters[0].ParameterType == typeof(ResolutionContext) && parameters[1].ParameterType == typeof(Exception)
                     select c).Single();

                return Lambda(Expression.TryCatch(mapExpression.Body,
                    MakeCatchBlock(typeof(AutoMapperMappingException), autoMapException,
                        Block(Assign(Property(autoMapException, "Context"), context),
                        Rethrow(),
                        Default(destination.Type)), null),
                    MakeCatchBlock(typeof(Exception), exception, Block(
                        Throw(New(mappingExceptionCtor, context, exception)),
                        Default(destination.Type)), null)),
                    source, destination, context);
            }
        }
    }
}
