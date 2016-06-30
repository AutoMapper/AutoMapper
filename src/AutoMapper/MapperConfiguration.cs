using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;

namespace AutoMapper
{
    using Configuration;
    using Mappers;
    using QueryableExtensions;
    using QueryableExtensions.Impl;
    using static Expression;
    using static ExpressionExtensions;
    using UntypedMapperFunc = System.Func<object, object, ResolutionContext, object>;
    using IMemberConfiguration = Configuration.Conventions.IMemberConfiguration;
    using TypeMapActions = IEnumerable<Action<TypeMap, IMappingExpression>>;
    using PropertyMapActions = IEnumerable<Action<PropertyMap, IMemberConfigurationExpression>>;

    public class MapperConfiguration : IConfigurationProvider
    {
        private readonly IEnumerable<IObjectMapper> _mappers;
        private readonly TypeMapRegistry _typeMapRegistry = new TypeMapRegistry();
        private readonly ConcurrentDictionary<TypePair, TypeMap> _typeMapPlanCache = new ConcurrentDictionary<TypePair, TypeMap>();
        private readonly ConcurrentDictionary<MapRequest, MapperFuncs> _mapPlanCache = new ConcurrentDictionary<MapRequest, MapperFuncs>();
        private readonly ConfigurationValidator _validator;
        private readonly Func<TypePair, TypeMap> _getTypeMap;
        private readonly Func<MapRequest, MapperFuncs> _createMapperFuncs;
        private readonly IConfiguration _mapperConfigurationExpression;

        public MapperConfiguration(MapperConfigurationExpression configurationExpression)
            : this(configurationExpression, MapperRegistry.Mappers)
        {
            
        }

        public MapperConfiguration(MapperConfigurationExpression configurationExpression, IEnumerable<IObjectMapper> mappers)
        {
            _mappers = mappers;
            _getTypeMap = GetTypeMap;
            _createMapperFuncs = CreateMapperFuncs;

            _validator = new ConfigurationValidator(this);
            ExpressionBuilder = new ExpressionBuilder(this);

            _mapperConfigurationExpression = configurationExpression;

            Seal(_mapperConfigurationExpression);
        }

        public MapperConfiguration(Action<IMapperConfigurationExpression> configure) : this(configure, MapperRegistry.Mappers)
        {
        }

        public MapperConfiguration(Action<IMapperConfigurationExpression> configure, IEnumerable<IObjectMapper> mappers)
            : this(Build(configure), mappers)
        {
        }

        TypeMapActions IProfileConfiguration.AllTypeMapActions => _mapperConfigurationExpression.AllTypeMapActions;

        PropertyMapActions IProfileConfiguration.AllPropertyMapActions => _mapperConfigurationExpression.AllPropertyMapActions;

        IEnumerable<IMemberConfiguration> IProfileConfiguration.MemberConfigurations => _mapperConfigurationExpression.MemberConfigurations;

        IEnumerable<IConditionalObjectMapper> IProfileConfiguration.TypeConfigurations => _mapperConfigurationExpression.TypeConfigurations;

        bool IProfileConfiguration.ConstructorMappingEnabled => _mapperConfigurationExpression.ConstructorMappingEnabled;

        bool IProfileConfiguration.CreateMissingTypeMaps => _mapperConfigurationExpression.CreateMissingTypeMaps;

        IMemberConfiguration IProfileConfiguration.DefaultMemberConfig => _mapperConfigurationExpression.DefaultMemberConfig;

        IEnumerable<MethodInfo> IProfileConfiguration.SourceExtensionMethods => _mapperConfigurationExpression.SourceExtensionMethods;

        Func<PropertyInfo, bool> IProfileConfiguration.ShouldMapProperty => _mapperConfigurationExpression.ShouldMapProperty;

        Func<FieldInfo, bool> IProfileConfiguration.ShouldMapField => _mapperConfigurationExpression.ShouldMapField;

        string IProfileConfiguration.ProfileName => _mapperConfigurationExpression.ProfileName;

        IEnumerable<string> IProfileConfiguration.GlobalIgnores => _mapperConfigurationExpression.GlobalIgnores;

        void IProfileConfiguration.Register(TypeMapRegistry typeMapRegistry) => _mapperConfigurationExpression.Register(typeMapRegistry);

        void IProfileConfiguration.Configure(TypeMapRegistry typeMapRegistry) => _mapperConfigurationExpression.Configure(typeMapRegistry);

        TypeMap IProfileConfiguration.ConfigureConventionTypeMap(TypeMapRegistry typeMapRegistry, TypePair conventionTypes)
            => _mapperConfigurationExpression.ConfigureConventionTypeMap(typeMapRegistry, conventionTypes);

        TypeMap IProfileConfiguration.ConfigureClosedGenericTypeMap(TypeMapRegistry typeMapRegistry, TypePair closedTypes, TypePair requestedTypes)
            => _mapperConfigurationExpression.ConfigureClosedGenericTypeMap(typeMapRegistry, closedTypes, requestedTypes);

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
            return new MapperFuncs(mapRequest, mapperToUse, this);
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

        private static MapperConfigurationExpression Build(Action<IMapperConfigurationExpression> configure)
        {
            var expr = new MapperConfigurationExpression();
            configure(expr);
            return expr;
        }

        private void Seal(IConfiguration configuration)
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

            foreach (var action in configuration.AllPropertyMapActions)
            {
                foreach (var typeMap in _typeMapRegistry.TypeMaps)
                {
                    foreach (var propertyMap in typeMap.GetPropertyMaps())
                    {
                        var memberExpression = new MappingExpression.MemberConfigurationExpression(propertyMap.DestinationProperty, typeMap.SourceType);

                        action(propertyMap, memberExpression);

                        memberExpression.Configure(typeMap);
                    }
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
            var typeMap = _mapperConfigurationExpression.Profiles
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

            var typeMap = _mapperConfigurationExpression.Profiles
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

            public MapperFuncs(MapRequest mapRequest, IObjectMapper mapperToUse, MapperConfiguration mapperConfiguration) : this(mapRequest, GenerateObjectMapperExpression(mapRequest, mapperToUse, mapperConfiguration))
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
                // Invoking a delegate here
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

            private static readonly Expression<Func<AutoMapperMappingException>> ResolutionContextCtor = () => new AutoMapperMappingException(null, null, default(TypePair));
            private static LambdaExpression GenerateObjectMapperExpression(MapRequest mapRequest, IObjectMapper mapperToUse, MapperConfiguration mapperConfiguration)
            {
                var destinationType = mapRequest.RequestedTypes.DestinationType;

                var source = Parameter(mapRequest.RequestedTypes.SourceType, "source");
                var destination = Parameter(destinationType, "destination");
                var context = Parameter(typeof(ResolutionContext), "context");

                var ctor = ((NewExpression)ResolutionContextCtor.Body).Constructor;

                LambdaExpression fullExpression;
                if (mapperToUse == null)
                {
                    var message = Constant("Missing type map configuration or unsupported mapping.");
                    fullExpression = Lambda(Block(Throw(New(ctor, message, Constant(null, typeof(Exception)), Constant(mapRequest.RequestedTypes))), Default(destinationType)), source, destination, context);
                }
                else
                {
                    var map = mapperToUse.MapExpression(mapperConfiguration._typeMapRegistry, mapperConfiguration, null, ToType(source, mapRequest.RuntimeTypes.SourceType), destination, context);
                    var mapToDestination = Lambda(ToType(map, destinationType), source, destination, context);
                    fullExpression = TryCatch(mapToDestination, source, destination, context, mapRequest.RequestedTypes);
                }
                return fullExpression;
            }

            private static LambdaExpression TryCatch(LambdaExpression mapExpression, ParameterExpression source, ParameterExpression destination, ParameterExpression context, TypePair types)
            {
                var exception = Parameter(typeof(Exception), "ex");

                var ctor = ((NewExpression)ResolutionContextCtor.Body).Constructor;
                return Lambda(Expression.TryCatch(mapExpression.Body,
                    MakeCatchBlock(typeof(Exception), exception, Block(
                        Throw(New(ctor, Constant("Error mapping types."), exception, Constant(types))),
                        Default(destination.Type)), null)),
                    source, destination, context);
            }
        }
    }
}
