using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Configuration;
using AutoMapper.Internal;
using AutoMapper.QueryableExtensions;
using AutoMapper.QueryableExtensions.Impl;

namespace AutoMapper
{
    using static Expression;
    using static ExpressionFactory;
    using static Execution.ExpressionBuilder;
    using UntypedMapperFunc = Func<object, object, ResolutionContext, object>;
    using Validator = Action<ValidationContext>;

    public class MapperConfiguration : IConfigurationProvider
    {
        private static readonly ConstructorInfo ExceptionConstructor = typeof(AutoMapperMappingException).GetDeclaredConstructors().Single(c => c.GetParameters().Length == 3);

        private readonly IEnumerable<IObjectMapper> _mappers;
        private readonly TypeMapRegistry _typeMapRegistry = new TypeMapRegistry();
        private LockingConcurrentDictionary<TypePair, TypeMap> _typeMapPlanCache;
        private readonly LockingConcurrentDictionary<MapRequest, MapperFuncs> _mapPlanCache;
        private readonly ConfigurationValidator _validator;
        private readonly MapperConfigurationExpressionValidator _expressionValidator;

        public MapperConfiguration(MapperConfigurationExpression configurationExpression)
        {
            _mappers = configurationExpression.Mappers.ToArray();
            _typeMapPlanCache = new LockingConcurrentDictionary<TypePair, TypeMap>(GetTypeMap);
            _mapPlanCache = new LockingConcurrentDictionary<MapRequest, MapperFuncs>(CreateMapperFuncs);
            Validators = configurationExpression.Advanced.GetValidators();
            _validator = new ConfigurationValidator(this);
            _expressionValidator = new MapperConfigurationExpressionValidator(configurationExpression);
            ExpressionBuilder = new ExpressionBuilder(this);

            ServiceCtor = configurationExpression.ServiceCtor;
            EnableNullPropagationForQueryMapping = configurationExpression.EnableNullPropagationForQueryMapping ?? false;
            MaxExecutionPlanDepth = configurationExpression.Advanced.MaxExecutionPlanDepth + 1;

            Configuration = new ProfileMap(configurationExpression);
            Profiles = new[] { Configuration }.Concat(configurationExpression.Profiles.Select(p => new ProfileMap(p, configurationExpression))).ToArray();

            foreach (var beforeSealAction in configurationExpression.Advanced.BeforeSealActions)
                beforeSealAction?.Invoke(this);
            Seal();
        }


        public MapperConfiguration(Action<IMapperConfigurationExpression> configure)
            : this(Build(configure))
        {
        }


        public void Validate(ValidationContext context)
        {
            foreach (var validator in Validators)
            {
                validator(context);
            }
        }

        private Validator[] Validators { get; }

        public IExpressionBuilder ExpressionBuilder { get; }

        public Func<Type, object> ServiceCtor { get; }

        public bool EnableNullPropagationForQueryMapping { get; }

        public int MaxExecutionPlanDepth { get; }

        private ProfileMap Configuration { get; }

        public IEnumerable<ProfileMap> Profiles { get; }

        public Func<TSource, TDestination, ResolutionContext, TDestination> GetMapperFunc<TSource, TDestination>(TypePair types)
        {
            var key = new TypePair(typeof(TSource), typeof(TDestination));
            var mapRequest = new MapRequest(key, types);
            return (Func<TSource, TDestination, ResolutionContext, TDestination>)GetMapperFunc(mapRequest);
        }

        public void CompileMappings()
        {
            foreach (var request in _typeMapPlanCache.Keys.Select(types => new MapRequest(types, types)).ToArray())
            {
                GetMapperFunc(request);
            }
        }

        public Delegate GetMapperFunc(MapRequest mapRequest) => _mapPlanCache.GetOrAdd(mapRequest).Typed;

        public UntypedMapperFunc GetUntypedMapperFunc(MapRequest mapRequest) => _mapPlanCache.GetOrAdd(mapRequest).Untyped;

        private MapperFuncs CreateMapperFuncs(MapRequest mapRequest) => new MapperFuncs(mapRequest, BuildExecutionPlan(mapRequest));

        public LambdaExpression BuildExecutionPlan(Type sourceType, Type destinationType)
        {
            var typePair = new TypePair(sourceType, destinationType);
            return BuildExecutionPlan(new MapRequest(typePair, typePair));
        }

        public LambdaExpression BuildExecutionPlan(MapRequest mapRequest)
        {
            var typeMap = ResolveTypeMap(mapRequest.RuntimeTypes) ?? ResolveTypeMap(mapRequest.RequestedTypes);
            if (typeMap != null)
            {
                return GenerateTypeMapExpression(mapRequest, typeMap);
            }
            var mapperToUse = FindMapper(mapRequest.RuntimeTypes);
            return GenerateObjectMapperExpression(mapRequest, mapperToUse, this);
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
                var requestedSourceParameter = Parameter(requestedSourceType, "source");
                var requestedDestinationParameter = Parameter(requestedDestinationType, "typeMapDestination");
                var contextParameter = Parameter(typeof(ResolutionContext), "context");

                mapExpression = Lambda(ToType(Invoke(typeMap.MapExpression,
                    ToType(requestedSourceParameter, typeMapSourceParameter.Type),
                    ToType(requestedDestinationParameter, typeMapDestinationParameter.Type),
                    contextParameter
                    ), mapRequest.RuntimeTypes.DestinationType),
                    requestedSourceParameter, requestedDestinationParameter, contextParameter);
            }

            return mapExpression;
        }

        private LambdaExpression GenerateObjectMapperExpression(MapRequest mapRequest, IObjectMapper mapperToUse, MapperConfiguration mapperConfiguration)
        {
            var destinationType = mapRequest.RequestedTypes.DestinationType;

            var source = Parameter(mapRequest.RequestedTypes.SourceType, "source");
            var destination = Parameter(destinationType, "mapperDestination");
            var context = Parameter(typeof(ResolutionContext), "context");
            Expression fullExpression;
            if (mapperToUse == null)
            {
                var message = Constant("Missing type map configuration or unsupported mapping.");
                fullExpression = Block(Throw(New(ExceptionConstructor, message, Constant(null, typeof(Exception)), Constant(mapRequest.RequestedTypes))), Default(destinationType));
            }
            else
            {
                var map = mapperToUse.MapExpression(mapperConfiguration, Configuration, null, 
                                                                        ToType(source, mapRequest.RuntimeTypes.SourceType), 
                                                                        ToType(destination, mapRequest.RuntimeTypes.DestinationType), 
                                                                        context);
                var exception = Parameter(typeof(Exception), "ex");
                fullExpression =
                    TryCatch(ToType(map, destinationType),
                    MakeCatchBlock(typeof(Exception), exception, Block(
                        Throw(New(ExceptionConstructor, Constant("Error mapping types."), exception, Constant(mapRequest.RequestedTypes))),
                        Default(destination.Type)), null));
            }
            var nullCheckSource = NullCheckSource(Configuration, source, destination, fullExpression);
            return Lambda(nullCheckSource, source, destination, context);
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
            var typeMap = _typeMapPlanCache.GetOrAdd(typePair);
            // if it's a dynamically created type map, we need to seal it outside GetTypeMap to handle recursion
            if (typeMap != null && typeMap.MapExpression == null && _typeMapRegistry.GetTypeMap(typePair) == null)
            {
                lock(typeMap)
                {
                    typeMap.Seal(this);
                }
            }
            return typeMap;
        }

        private TypeMap GetTypeMap(TypePair initialTypes)
        {
            foreach (var types in initialTypes.GetRelatedTypePairs())
            {
                if (types != initialTypes && _typeMapPlanCache.TryGetValue(types, out var typeMap))
                {
                    return typeMap;
                }
                typeMap = FindTypeMapFor(types);
                if (typeMap != null)
                {
                    return typeMap;
                }
                typeMap = FindClosedGenericTypeMapFor(types);
                if (typeMap != null)
                {
                    return typeMap;
                }
                if (FindMapper(initialTypes) == null)
                {
                    typeMap = FindConventionTypeMapFor(types);
                    if (typeMap != null)
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
            _validator.AssertConfigurationIsValid(_typeMapRegistry.TypeMaps.Where(typeMap => typeMap.Profile.Name == profileName));
        }

        public void AssertConfigurationIsValid<TProfile>()
            where TProfile : Profile, new()
        {
            AssertConfigurationIsValid(new TProfile().ProfileName);
        }

        public void AssertConfigurationIsValid()
        {
            _expressionValidator.AssertConfigurationExpressionIsValid();

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

        private void Seal()
        {
            var derivedMaps = new List<Tuple<TypePair, TypeMap>>();
            var redirectedTypes = new List<Tuple<TypePair, TypePair>>();

            foreach (var profile in Profiles)
            {
                profile.Register(_typeMapRegistry);
            }

            foreach (var profile in Profiles)
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
                typeMap.Seal(this);
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

        private TypeMap FindConventionTypeMapFor(TypePair typePair)
        {
            var profile = Profiles.FirstOrDefault(p => p.IsConventionMap(typePair));
            if(profile == null)
            {
                return null;
            }
            TypeMap typeMap;
            lock(this)
            {
                typeMap = profile.CreateConventionTypeMap(_typeMapRegistry, typePair);
            }
            return typeMap;
        }

        private TypeMap FindClosedGenericTypeMapFor(TypePair typePair)
        {
            if(typePair.GetOpenGenericTypePair() == null)
            {
                return null;
            }
            var mapInfo = Profiles.Select(p => new { GenericMap = p.GetGenericMap(typePair), Profile = p }).FirstOrDefault(p => p.GenericMap != null);
            if(mapInfo == null)
            {
                return null;
            }
            TypeMap typeMap;
            lock(this)
            {
                typeMap = mapInfo.Profile.CreateClosedGenericTypeMap(mapInfo.GenericMap, _typeMapRegistry, typePair);
            }
            return typeMap;
        }

        public IObjectMapper FindMapper(TypePair types) =>_mappers.FirstOrDefault(m => m.IsMatch(types));

        internal struct MapperFuncs
        {
            public Delegate Typed { get; }

            public UntypedMapperFunc Untyped { get; }

            public MapperFuncs(MapRequest mapRequest, LambdaExpression typedExpression)
            {
                Typed = typedExpression.Compile();
                Untyped = Wrap(mapRequest, Typed).Compile();
            }

            private static Expression<UntypedMapperFunc> Wrap(MapRequest mapRequest, Delegate typedDelegate)
            {
                var sourceParameter = Parameter(typeof(object), "source");
                var destinationParameter = Parameter(typeof(object), "destination");
                var contextParameter = Parameter(typeof(ResolutionContext), "context");
                var requestedSourceType = mapRequest.RequestedTypes.SourceType;
                var requestedDestinationType = mapRequest.RequestedTypes.DestinationType;

                var destination = requestedDestinationType.IsValueType() ? Coalesce(destinationParameter, New(requestedDestinationType)) : (Expression)destinationParameter;
                // Invoking a delegate here
                return Lambda<UntypedMapperFunc>(
                            ToType(
                                Invoke(Constant(typedDelegate), ToType(sourceParameter, requestedSourceType), ToType(destination, requestedDestinationType), contextParameter)
                                , typeof(object)),
                          sourceParameter, destinationParameter, contextParameter);
            }
        }
    }

    public struct ValidationContext
    {
        public IObjectMapper ObjectMapper { get; }
        public PropertyMap PropertyMap { get; }
        public TypeMap TypeMap { get; }
        public TypePair Types { get; }

        public ValidationContext(TypePair types, PropertyMap propertyMap, IObjectMapper objectMapper) : this(types, propertyMap, objectMapper, null)
        {
        }

        public ValidationContext(TypePair types, PropertyMap propertyMap, TypeMap typeMap) : this(types, propertyMap, null, typeMap)
        {
        }

        private ValidationContext(TypePair types, PropertyMap propertyMap, IObjectMapper objectMapper, TypeMap typeMap)
        {
            ObjectMapper = objectMapper;
            TypeMap = typeMap;
            Types = types;
            PropertyMap = propertyMap;
        }
    }
}
