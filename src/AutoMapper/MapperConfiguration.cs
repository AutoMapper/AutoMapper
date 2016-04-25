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

    public class MapperConfiguration : IConfigurationProvider, IMapperConfiguration
    {
        private readonly IEnumerable<IObjectMapper> _mappers;
        private readonly List<Action<TypeMap, IMappingExpression>> _allTypeMapActions = new List<Action<TypeMap, IMappingExpression>>();
        private readonly Profile _defaultProfile;
        private readonly TypeMapRegistry _typeMapRegistry = new TypeMapRegistry();
        private readonly ConcurrentDictionary<TypePair, TypeMap> _typeMapPlanCache = new ConcurrentDictionary<TypePair, TypeMap>();
        private readonly ConcurrentDictionary<MapRequest, Delegate> _mapPlanCache = new ConcurrentDictionary<MapRequest, Delegate>();
        private readonly IList<Profile> _profiles = new List<Profile>();
        private readonly ConfigurationValidator _validator;
        private Func<Type, object> _serviceCtor = ObjectCreator.CreateObject;
        private readonly Func<TypePair, TypeMap> _getTypeMap;


        public MapperConfiguration(Action<IMapperConfiguration> configure) : this(configure, MapperRegistry.Mappers)
        {
        }

        public MapperConfiguration(Action<IMapperConfiguration> configure, IEnumerable<IObjectMapper> mappers)
        {
            _mappers = mappers;
            var profileExpression = new NamedProfile(ProfileName);

            _profiles.Add(profileExpression);

            _defaultProfile = profileExpression;

            _validator = new ConfigurationValidator(this);

            configure(this);

            Seal();

            ExpressionBuilder = new ExpressionBuilder(this);
            _getTypeMap = GetTypeMap;
        }

        public string ProfileName => "";

        #region IConfiguration Members

        void IConfiguration.CreateProfile(string profileName, Action<Profile> config)
        {
            var profile = new NamedProfile(profileName);

            config(profile);

            ((IConfiguration)this).AddProfile(profile);
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

        Configuration.Conventions.IMemberConfiguration IProfileExpression.AddMemberConfiguration() => _defaultProfile.AddMemberConfiguration();

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

        public Func<TSource, TDestination, ResolutionContext, TDestination> GetMapperFunc<TSource, TDestination>(
            TypePair types)
        {
            var key = new TypePair(typeof (TSource), typeof (TDestination));
            var mapRequest = new MapRequest(key, types);
            return (Func<TSource, TDestination, ResolutionContext, TDestination>) _mapPlanCache.GetOrAdd(mapRequest, mapReq =>
            {
                var typeMap = ResolveTypeMap(types);

                if (typeMap != null)
                {
                    var mapExpression = typeMap.MapExpression;

                    if (mapExpression.Parameters[0].Type != typeof (TSource)
                        || mapExpression.Parameters[1].Type != typeof (TDestination))
                    {
                        var srcParam = Parameter(typeof (TSource), "src");
                        var destParam = Parameter(typeof (TDestination), "dest");
                        var ctxtParam = Parameter(typeof (ResolutionContext), "ctxt");

                        mapExpression = Lambda(ToType(Invoke(typeMap.MapExpression,
                            ToType(srcParam, typeMap.MapExpression.Parameters[0].Type),
                            ToType(destParam, typeMap.MapExpression.Parameters[1].Type),
                            ctxtParam
                            ), typeof(TDestination)),
                            srcParam, destParam, ctxtParam);
                    }

                    var autoMapException = Parameter(typeof(AutoMapperMappingException), "ex");
                    var exception = Parameter(typeof(Exception), "ex");

                    var mappingExceptionCtor = typeof(AutoMapperMappingException).GetTypeInfo().DeclaredConstructors
                        .Where(ci => ci.GetParameters().Count() == 2)
                        .First(ci => ci.GetParameters()[0].ParameterType == typeof(ResolutionContext) && ci.GetParameters()[1].ParameterType == typeof(Exception));

                    mapExpression = Lambda(TryCatch(mapExpression.Body,
                        MakeCatchBlock(typeof(AutoMapperMappingException), autoMapException,
                            Block(Assign(Property(autoMapException, "Context"), mapExpression.Parameters[2]),
                            Rethrow(),
                            Default(typeof(TDestination))), null),
                        MakeCatchBlock(typeof(Exception), exception, Block(
                            Throw(New(mappingExceptionCtor, mapExpression.Parameters[2], exception)),
                            Default(typeof(TDestination))), null)),
                        mapExpression.Parameters);

                    return (Func<TSource, TDestination, ResolutionContext, TDestination>) mapExpression.Compile();
                    //return new Func<TSource, TDestination, ResolutionContext, TDestination>((src, dest, context) =>
                    //{
                    //    try
                    //    {
                    //        return (TDestination) typeMap.Map(src, context);
                    //    }
                    //    catch (AutoMapperMappingException)
                    //    {
                    //        throw;
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        throw new AutoMapperMappingException(context, ex);
                    //    }
                    //});
                }

                IObjectMapper mapperToUse = _mappers.FirstOrDefault(om => om.IsMatch(mapReq.RequestedTypes));

                return new Func<TSource, TDestination, ResolutionContext, TDestination>((src, dest, context) =>
                {
                    if (mapperToUse == null)
                    {
                        throw new AutoMapperMappingException(context,
                            "Missing type map configuration or unsupported mapping.");
                    }
                    try
                    {
                        return (TDestination) mapperToUse.Map(context);
                    }
                    catch (AutoMapperMappingException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new AutoMapperMappingException(context, ex);
                    }
                });
            });
        }

        public Delegate GetMapperFunc(MapRequest mapRequest)
        {
            return _mapPlanCache.GetOrAdd(mapRequest, mapReq =>
            {
                var tp = mapReq.RuntimeTypes;
                var typeMap = ResolveTypeMap(tp);

                if (typeMap != null)
                {
                    var mapExpression = typeMap.MapExpression;

                    if (mapExpression.Parameters[0].Type != tp.SourceType
                        || mapExpression.Parameters[1].Type != tp.DestinationType)
                    {
                        var srcParam = Parameter(tp.SourceType, "src");
                        var destParam = Parameter(tp.DestinationType, "dest");
                        var ctxtParam = Parameter(typeof(ResolutionContext), "ctxt");

                        mapExpression = Lambda(ToType(Invoke(typeMap.MapExpression,
                            ToType(srcParam, typeMap.MapExpression.Parameters[0].Type),
                            ToType(destParam, typeMap.MapExpression.Parameters[1].Type),
                            ctxtParam
                            ), tp.DestinationType),
                            srcParam, destParam, ctxtParam);
                    }

                    return mapExpression.Compile();
                    //return new Func<TSource, TDestination, ResolutionContext, TDestination>((src, dest, context) =>
                    //{
                    //    try
                    //    {
                    //        return (TDestination) typeMap.Map(src, context);
                    //    }
                    //    catch (AutoMapperMappingException)
                    //    {
                    //        throw;
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        throw new AutoMapperMappingException(context, ex);
                    //    }
                    //});
                }

                IObjectMapper mapperToUse = _mappers.FirstOrDefault(om => om.IsMatch(tp));

                return new Func<object, object, ResolutionContext, object>((src, dest, context) =>
                {
                    if (mapperToUse == null)
                    {
                        throw new AutoMapperMappingException(context,
                            "Missing type map configuration or unsupported mapping.");
                    }
                    try
                    {
                        return mapperToUse.Map(context);
                    }
                    catch (AutoMapperMappingException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new AutoMapperMappingException(context, ex);
                    }
                });
            });
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
            foreach(var tp in pair.GetRelatedTypePairs())
            {
                var typeMap =
                          _typeMapPlanCache.GetOrDefault(tp) ??
                          FindTypeMapFor(tp) ??
                          (!CoveredByObjectMap(pair) ? FindConventionTypeMapFor(tp) : null) ??
                          FindClosedGenericTypeMapFor(tp, pair);
                if(typeMap != null)
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
                typeMap.Seal(_typeMapRegistry);
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
            var typeMap = _profiles
                .Cast<IProfileConfiguration>()
                .Select(p => p.ConfigureConventionTypeMap(_typeMapRegistry, typePair))
                .FirstOrDefault(t => t != null);

            typeMap?.Seal(_typeMapRegistry);

            return typeMap;
        }

        private TypeMap FindClosedGenericTypeMapFor(TypePair typePair, TypePair requestedTypes)
        {
            if (typePair.GetOpenGenericTypePair() == null)
                return null;

            var typeMap = _profiles
                .Cast<IProfileConfiguration>()
                .Select(p => p.ConfigureClosedGenericTypeMap(_typeMapRegistry, typePair, requestedTypes))
                .FirstOrDefault(t => t != null);

            typeMap?.Seal(_typeMapRegistry);

            return typeMap;
        }
    }
}
