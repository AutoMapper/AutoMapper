using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoMapper
{
    using Configuration;
    using Mappers;
    using QueryableExtensions;
    using QueryableExtensions.Impl;
    using static Expression;
    using static ExpressionExtensions;
    using UntypedMapperFunc = System.Func<object, object, ResolutionContext, object>;

    public class MapperConfiguration : IConfigurationProvider
    {
        private readonly IEnumerable<IObjectMapper> _mappers;
        private readonly TypeMapRegistry _typeMapRegistry = new TypeMapRegistry();
        private readonly Dictionary<TypePair, TypeMap> _typeMapPlanCache = new Dictionary<TypePair, TypeMap>();
        private readonly LockingConcurrentDictionary<MapRequest, MapperFuncs> _mapPlanCache;
        private readonly ConfigurationValidator _validator;

        public MapperConfiguration(MapperConfigurationExpression configurationExpression)
            : this(configurationExpression, MapperRegistry.Mappers)
        {
            
        }

        public MapperConfiguration(MapperConfigurationExpression configurationExpression, IEnumerable<IObjectMapper> mappers)
        {
            _mappers = mappers;
            _mapPlanCache = new LockingConcurrentDictionary<MapRequest, MapperFuncs>(CreateMapperFuncs);
            _validator = new ConfigurationValidator(this);
            ExpressionBuilder = new ExpressionBuilder(this);

            Configuration = configurationExpression;

            Seal(Configuration);
        }

        public MapperConfiguration(Action<IMapperConfigurationExpression> configure) : this(configure, MapperRegistry.Mappers)
        {
        }

        public MapperConfiguration(Action<IMapperConfigurationExpression> configure, IEnumerable<IObjectMapper> mappers)
            : this(Build(configure), mappers)
        {
        }

        public IExpressionBuilder ExpressionBuilder { get; }

        public Func<Type, object> ServiceCtor { get; private set; }

        public bool AllowNullDestinationValues { get; private set; }

        public bool AllowNullCollections { get; private set; }

        public bool EnableNullPropagationForQueryMapping { get; private set; }

        public IConfiguration Configuration { get; }

        public Func<TSource, TDestination, ResolutionContext, TDestination> GetMapperFunc<TSource, TDestination>(TypePair types)
        {
            var key = new TypePair(typeof(TSource), typeof(TDestination));
            var mapRequest = new MapRequest(key, types);
            var mapperFuncs = GetMapperFuncs(mapRequest);
            return GetSubMap<TSource, TDestination>(mapperFuncs)?.Compile() ?? (Func<TSource, TDestination, ResolutionContext, TDestination>)mapperFuncs.Typed;
        }

        public Expression<Func<TSource, TDestination, ResolutionContext, TDestination>> GetMapperExpression<TSource, TDestination>(TypePair typePair)
        {
            var key = new TypePair(typeof(TSource), typeof(TDestination));
            var mapRequest = new MapRequest(key, typePair);
            var mapperFuncs = GetMapperFuncs(mapRequest);
            return GetSubMap<TSource, TDestination>(mapperFuncs) ?? (Expression<Func<TSource, TDestination, ResolutionContext, TDestination>>)mapperFuncs.TypedExpression;
        }

        private Expression<Func<TSource, TDestination, ResolutionContext, TDestination>> GetSubMap<TSource, TDestination>(MapperFuncs mapperFuncs)
        {
            var mapExpression = mapperFuncs.TypedExpression;
            if (mapExpression != null && (mapExpression.Parameters[0].Type != typeof(TSource) ||
                mapExpression.Parameters[1].Type != typeof(TDestination)))
            {
                var requestedSourceParameter = Parameter(typeof(TSource), "src");
                var requestedDestinationParameter = Parameter(typeof(TDestination), "dest");
                var contextParameter = Parameter(typeof(ResolutionContext), "ctxt");
                return (Expression<Func<TSource, TDestination, ResolutionContext, TDestination>>)
                    Lambda(
                        ToType(
                            mapExpression.ReplaceParameters(
                                ToType(requestedSourceParameter, mapExpression.Parameters[0].Type),
                                ToType(requestedDestinationParameter, mapExpression.Parameters[1].Type),
                                contextParameter),
                            typeof(TDestination)),
                        requestedSourceParameter, requestedDestinationParameter, contextParameter);
            }
            return null;
        }

        private MapperFuncs GetMapperFuncs(MapRequest mapRequest)
        {
            var mapperFunc = _mapPlanCache.GetOrAdd(mapRequest);
            if (mapperFunc.TypedExpression == null)
                mapperFunc = _mapPlanCache.AddOrUpdate(mapRequest);
            return mapperFunc;
        }

        public Delegate GetMapperFunc(MapRequest mapRequest)
        {
            return _mapPlanCache.GetOrAdd(mapRequest).Typed ??
                _mapPlanCache.AddOrUpdate(mapRequest).Typed;
        }

        public UntypedMapperFunc GetUntypedMapperFunc(MapRequest mapRequest)
        {
            return _mapPlanCache.GetOrAdd(mapRequest).Untyped ??
                _mapPlanCache.AddOrUpdate(mapRequest).Untyped;
        }

        private MapperFuncs CreateMapperFuncs(MapRequest mapRequest)
        {
            var typeMap = ResolveTypeMap(mapRequest.RuntimeTypes);
            if(typeMap != null)
            {
                return new MapperFuncs(mapRequest, typeMap, this);
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
            TypeMap typeMap;
            if(!_typeMapPlanCache.TryGetValue(typePair, out typeMap))
            {
                typeMap = GetTypeMap(typePair);
                _typeMapPlanCache.Add(typePair, typeMap);
            }
            if(typeMap != null && typeMap.MapExpression == null && Configuration.CreateMissingTypeMaps)
            {
                Seal(typeMap);
            }
            return typeMap;
        }

        public void Seal(TypeMap typeMap)
        {
            typeMap.Seal(_typeMapRegistry, this);
        }

        private TypeMap GetTypeMap(TypePair initialTypes)
        {
            TypeMap typeMap;
            foreach(var types in initialTypes.GetRelatedTypePairs())
            {
                if(_typeMapPlanCache.TryGetValue(types, out typeMap))
                {
                    return typeMap;
                }
                typeMap = FindTypeMapFor(types);
                if(typeMap != null)
                {
                    return typeMap;
                }
                typeMap = FindClosedGenericTypeMapFor(types, initialTypes);
                if(typeMap != null)
                {
                    return typeMap;
                }
                if(!CoveredByObjectMap(initialTypes))
                {
                    typeMap = FindConventionTypeMapFor(types);
                    if(typeMap != null)
                    {
                        return typeMap;
                    }
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
            EnableNullPropagationForQueryMapping = configuration.EnableNullPropagationForQueryMapping;

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
            var typeMap = Configuration.Profiles
                .Cast<IProfileConfiguration>()
                .Select(p => p.ConfigureConventionTypeMap(_typeMapRegistry, typePair))
                .FirstOrDefault(t => t != null);

            if(!Configuration.CreateMissingTypeMaps)
            {
                typeMap?.Seal(_typeMapRegistry, this);
            }

            return typeMap;
        }

        private TypeMap FindClosedGenericTypeMapFor(TypePair typePair, TypePair requestedTypes)
        {
            if (typePair.GetOpenGenericTypePair() == null)
                return null;

            var typeMap = Configuration.Profiles
                .Cast<IProfileConfiguration>()
                .Select(p => p.ConfigureClosedGenericTypeMap(_typeMapRegistry, typePair, requestedTypes))
                .FirstOrDefault(t => t != null);

            typeMap?.Seal(_typeMapRegistry, this);

            return typeMap;
        }

        internal struct MapperFuncs
        {
            private Lazy<UntypedMapperFunc> _untyped;
            
            public LambdaExpression TypedExpression { get; }
            public Delegate Typed { get; }

            public UntypedMapperFunc Untyped => _untyped.Value;

            public MapperFuncs(MapRequest mapRequest, TypeMap typeMap, MapperConfiguration mapperConfiguration) : this(mapRequest, GenerateTypeMapExpression(typeMap, mapperConfiguration))
            {
            }

            public MapperFuncs(MapRequest mapRequest, IObjectMapper mapperToUse, MapperConfiguration mapperConfiguration) : this(mapRequest, GenerateObjectMapperExpression(mapRequest, mapperToUse, mapperConfiguration))
            {
            }

            public MapperFuncs(MapRequest mapRequest, LambdaExpression typedExpression)
            {
                TypedExpression = typedExpression;
                Typed = TypedExpression?.Compile();
                _untyped = new Lazy<UntypedMapperFunc>(() => Wrap(mapRequest, typedExpression)?.Compile());
            }

            private static Expression<UntypedMapperFunc> Wrap(MapRequest mapRequest, LambdaExpression typedExpression)
            {
                if (typedExpression == null)
                    return null;
                var sourceParameter = Parameter(typeof(object), "src");
                var destinationParameter = Parameter(typeof(object), "dest");
                var contextParameter = Parameter(typeof(ResolutionContext), "ctxt");
                var requestedSourceType = mapRequest.RequestedTypes.SourceType;
                var requestedDestinationType = typedExpression.ReturnType;

                var destination = requestedDestinationType.IsValueType() ? Coalesce(destinationParameter, New(requestedDestinationType)) : (Expression)destinationParameter;
                // Invoking a delegate here
                return Lambda<UntypedMapperFunc>(
                            ToType(
                                Invoke(typedExpression, ToType(sourceParameter, requestedSourceType), ToType(destination, requestedDestinationType), contextParameter)
                                , typeof(object)),
                          sourceParameter, destinationParameter, contextParameter);
            }
            
            private static readonly ConstructorInfo ExceptionConstructor = typeof(AutoMapperMappingException).GetConstructors().Single(c => c.GetParameters().Length == 3);

            private static LambdaExpression GenerateTypeMapExpression(TypeMap typeMap, IConfigurationProvider configurationProvider)
            {
                var mapExpression = typeMap.MapExpression;
                if (typeMap.IncludedDerivedTypes.Any(d => d.SourceType != typeMap.SourceType) || typeMap.DestinationTypeOverride != null)
                {
                    var parameters = mapExpression.Parameters;
                    var ifTypeMaps = typeMap.IncludedDerivedTypes.Select(configurationProvider.ResolveTypeMap).Reverse();
                    var seed = mapExpression.Body;
                    var expression = ifTypeMaps.Aggregate(seed, (s, tm) => IfThenMap(tm, configurationProvider, s, parameters));
                    mapExpression = Lambda(expression, parameters);
                }

                return mapExpression;
            }

            private static Expression IfThenMap(TypeMap typeMap, IConfigurationProvider configurationProvider, Expression elseExpression, IList<ParameterExpression> parameters)
            {
                configurationProvider.Seal(typeMap);
                return Condition(TypeIs(parameters[0], typeMap.SourceType),
                    ToType(GenerateTypeMapExpression(typeMap, configurationProvider).ReplaceParameters(
                        TypeAs(parameters[0], typeMap.SourceType),
                        TypeAs(parameters[1], typeMap.DestinationType),
                        parameters[2]), elseExpression.Type), elseExpression);
            }
            
            private static LambdaExpression GenerateObjectMapperExpression(MapRequest mapRequest, IObjectMapper mapperToUse, MapperConfiguration mapperConfiguration)
            {
                var typePair = mapRequest.RequestedTypes;

                var destinationType = typePair.DestinationType;
                var source = Parameter(typePair.SourceType, "source");
                var destination = Parameter(destinationType, "mapperDestination");
                var context = Parameter(typeof(ResolutionContext), "context");
                LambdaExpression fullExpression;
                if (mapperToUse == null)
                {
                    var message = Constant("Missing type map configuration or unsupported mapping.");
                    fullExpression = Lambda(Block(Throw(New(ExceptionConstructor, message, Constant(null, typeof(Exception)), Constant(typePair))), Default(destinationType)), source, destination, context);
                }
                else
                {
                    var map = mapperToUse.MapExpression(mapperConfiguration._typeMapRegistry, mapperConfiguration, null, ToType(source, mapRequest.RuntimeTypes.SourceType), destination, context);
                    var mapToDestination = Lambda(ToType(map, destinationType), source, destination, context);
                    fullExpression = TryCatch(mapToDestination, source, destination, context, typePair);
                }
                return fullExpression;
            }


            private static LambdaExpression TryCatch(LambdaExpression mapExpression, ParameterExpression source, ParameterExpression destination, ParameterExpression context, TypePair types)
            {
                var exception = Parameter(typeof(Exception), "ex");

                return Lambda(Expression.TryCatch(mapExpression.Body,
                    MakeCatchBlock(typeof(Exception), exception, Block(
                        Throw(New(ExceptionConstructor, Constant("Error mapping types."), exception, Constant(types))),
                        Default(destination.Type)), null)),
                    source, destination, context);
            }
        }
    }

    internal struct LockingConcurrentDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _dictionary;
        private readonly Func<TKey, Lazy<TValue>> _valueFactory;

        public LockingConcurrentDictionary(Func<TKey, TValue> valueFactory)
        {
            _dictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>();
            _valueFactory = key => new Lazy<TValue>(() => valueFactory(key));
        }

        public TValue GetOrAdd(TKey key)
        {
            try
            {
                return _dictionary.GetOrAdd(key, _valueFactory).Value;
            }
            catch (InvalidOperationException e) when(e.Message == "ValueFactory attempted to access the Value property of this instance.")
            {
                return default(TValue);
            }
        }

        public TValue AddOrUpdate(TKey key)
        {
            LockingConcurrentDictionary<TKey, TValue> tmpThis = this;
            return tmpThis._dictionary.AddOrUpdate(key, tmpThis._valueFactory, (tp, mf) => tmpThis._valueFactory(tp)).Value;
        }
    }
}
