using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Configuration;
using AutoMapper.Features;
using AutoMapper.Internal;
using AutoMapper.QueryableExtensions;
using AutoMapper.QueryableExtensions.Impl;

namespace AutoMapper
{
    using static Expression;
    using static ExpressionFactory;
    using static Execution.ExpressionBuilder;
    using Validator = Action<ValidationContext>;

    public class MapperConfiguration : IConfigurationProvider
    {
        private static readonly Type[] ExcludedTypes = { typeof(object), typeof(ValueType), typeof(Enum) };
        private static readonly ConstructorInfo ExceptionConstructor = typeof(AutoMapperMappingException).GetDeclaredConstructors().Single(c => c.GetParameters().Length == 3);

        private readonly IEnumerable<IObjectMapper> _mappers;
        private readonly Dictionary<TypePair, TypeMap> _typeMapRegistry = new Dictionary<TypePair, TypeMap>();
        private LockingConcurrentDictionary<TypePair, TypeMap> _typeMapPlanCache;
        private readonly LockingConcurrentDictionary<MapRequest, Delegate> _mapPlanCache;
        private readonly ConfigurationValidator _validator;
        private readonly MapperConfigurationExpressionValidator _expressionValidator;

        public MapperConfiguration(MapperConfigurationExpression configurationExpression)
        {
            _mappers = configurationExpression.Mappers.ToArray();
            _typeMapPlanCache = new LockingConcurrentDictionary<TypePair, TypeMap>(GetTypeMap);
            _mapPlanCache = new LockingConcurrentDictionary<MapRequest, Delegate>(CompileExecutionPlan);
            Validators = configurationExpression.Advanced.GetValidators();
            _validator = new ConfigurationValidator(this);
            _expressionValidator = new MapperConfigurationExpressionValidator(configurationExpression);
            ExpressionBuilder = new ExpressionBuilder(this);

            ServiceCtor = configurationExpression.ServiceCtor;
            EnableNullPropagationForQueryMapping = configurationExpression.EnableNullPropagationForQueryMapping ?? false;
            MaxExecutionPlanDepth = configurationExpression.Advanced.MaxExecutionPlanDepth + 1;
            ResultConverters = configurationExpression.Advanced.QueryableResultConverters.ToArray();
            Binders = configurationExpression.Advanced.QueryableBinders.ToArray();
            RecursiveQueriesMaxDepth = configurationExpression.Advanced.RecursiveQueriesMaxDepth;

            Configuration = new ProfileMap(configurationExpression);
            Profiles = new[] { Configuration }.Concat(configurationExpression.Profiles.Select(p => new ProfileMap(p, configurationExpression))).ToArray();

            configurationExpression.Features.Configure(this);

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

        public IEnumerable<IExpressionResultConverter> ResultConverters { get; }

        public IEnumerable<IExpressionBinder> Binders { get; }
        
        public int RecursiveQueriesMaxDepth { get; }

        public Features<IRuntimeFeature> Features { get; } = new Features<IRuntimeFeature>();

        public Func<TSource, TDestination, ResolutionContext, TDestination> GetExecutionPlan<TSource, TDestination>(MapRequest mapRequest) 
            => (Func<TSource, TDestination, ResolutionContext, TDestination>)GetExecutionPlan(mapRequest);

        public void CompileMappings()
        {
            foreach (var request in _typeMapPlanCache.Keys.Where(t=>!t.IsGenericTypeDefinition).Select(types => new MapRequest(types, types)).ToArray())
            {
                GetExecutionPlan(request);
            }
        }

        private Delegate GetExecutionPlan(MapRequest mapRequest) => _mapPlanCache.GetOrAdd(mapRequest);

        private Delegate CompileExecutionPlan(MapRequest mapRequest)
        {
            var executionPlan = BuildExecutionPlan(mapRequest);
            return executionPlan.Compile(); // breakpoint here to inspect all execution plans
        }

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
            return GenerateObjectMapperExpression(mapRequest, mapperToUse);
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

        private LambdaExpression GenerateObjectMapperExpression(MapRequest mapRequest, IObjectMapper mapperToUse)
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
                var map = mapperToUse.MapExpression(this, Configuration, mapRequest.MemberMap, 
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
            var profileMap = mapRequest.MemberMap?.TypeMap?.Profile ?? Configuration;
            var nullCheckSource = NullCheckSource(profileMap, source, destination, fullExpression, mapRequest.MemberMap);
            return Lambda(nullCheckSource, source, destination, context);
        }

        public TypeMap[] GetAllTypeMaps() => _typeMapRegistry.Values.ToArray();

        public TypeMap FindTypeMapFor(Type sourceType, Type destinationType) => FindTypeMapFor(new TypePair(sourceType, destinationType));

        public TypeMap FindTypeMapFor<TSource, TDestination>() => FindTypeMapFor(new TypePair(typeof(TSource), typeof(TDestination)));

        public TypeMap FindTypeMapFor(TypePair typePair) => _typeMapRegistry.GetOrDefault(typePair);

        public TypeMap ResolveTypeMap(Type sourceType, Type destinationType)
        {
            var typePair = new TypePair(sourceType, destinationType);

            return ResolveTypeMap(typePair);
        }

        public TypeMap ResolveTypeMap(TypePair typePair)
        {
            var typeMap = _typeMapPlanCache.GetOrAdd(typePair);
            // if it's a dynamically created type map, we need to seal it outside GetTypeMap to handle recursion
            if (typeMap != null && typeMap.MapExpression == null && _typeMapRegistry.GetOrDefault(typePair) == null)
            {
                lock (typeMap)
                {
                    typeMap.Seal(this);
                    if (typeMap.IsClosedGeneric)
                    {
                        AssertConfigurationIsValid(typeMap);
                    }
                }
            }
            return typeMap;
        }

        private TypeMap GetTypeMap(TypePair initialTypes)
        {
            foreach (var types in initialTypes.GetRelatedTypePairs())
            {
                var typeMap = GetCachedMap(initialTypes, types);
                if(typeMap != null)
                {
                    return typeMap;
                }
                typeMap = FindTypeMapFor(types);
                if (typeMap != null)
                {
                    return typeMap;
                }
                typeMap = FindClosedGenericTypeMapFor(initialTypes, types);
                if (typeMap != null)
                {
                    return typeMap;
                }
            }

            return null;
        }

        private TypeMap GetCachedMap(TypePair initialTypes, TypePair types) => (types != initialTypes && _typeMapPlanCache.TryGetValue(types, out var typeMap)) ? typeMap : null;

        public void AssertConfigurationIsValid(TypeMap typeMap)
        {
            _validator.AssertConfigurationIsValid(Enumerable.Repeat(typeMap, 1));
        }

        public void AssertConfigurationIsValid(string profileName)
        {
            if (Profiles.All(x => x.Name != profileName))
            {
                throw new ArgumentOutOfRangeException(nameof(profileName), $"Cannot find any profiles with the name '{profileName}'.");
            }

            _validator.AssertConfigurationIsValid(_typeMapRegistry.Values.Where(typeMap => typeMap.Profile.Name == profileName));
        }

        public void AssertConfigurationIsValid<TProfile>()
            where TProfile : Profile, new()
        {
            AssertConfigurationIsValid(new TProfile().ProfileName);
        }

        public void AssertConfigurationIsValid()
        {
            _expressionValidator.AssertConfigurationExpressionIsValid();

            _validator.AssertConfigurationIsValid(_typeMapRegistry.Values);
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
                profile.Register(this);
            }

            foreach (var typeMap in _typeMapRegistry.Values.Where(tm => tm.IncludeAllDerivedTypes))
            {
                foreach (var derivedMap in _typeMapRegistry
                    .Where(tm =>
                        typeMap.SourceType.IsAssignableFrom(tm.Key.SourceType) &&
                        typeMap.DestinationType.IsAssignableFrom(tm.Key.DestinationType) &&
                        typeMap != tm.Value)
                    .Select(tm => tm.Value))
                {
                    typeMap.IncludeDerivedTypes(derivedMap.SourceType, derivedMap.DestinationType);
                }
            }

            foreach (var profile in Profiles)
            {
                profile.Configure(this);
            }

            foreach (var typeMap in _typeMapRegistry.Values)
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

            foreach (var typeMap in _typeMapRegistry.Values)
            {
                typeMap.Seal(this);
            }

            Features.Seal(this);
        }

        private IEnumerable<TypeMap> GetDerivedTypeMaps(TypeMap typeMap)
        {
            foreach (var derivedMap in GetIncludedTypeMaps(typeMap.IncludedDerivedTypes))
            {
                yield return derivedMap;
                foreach (var derivedTypeMap in GetDerivedTypeMaps(derivedMap))
                {
                    yield return derivedTypeMap;
                }
            }
        }

        public IEnumerable<TypeMap> GetIncludedTypeMaps(IEnumerable<TypePair> includedTypes)
        {
            foreach(var pair in includedTypes)
            {
                var typeMap = FindTypeMapFor(pair);
                if(typeMap != null)
                {
                    yield return typeMap;
                }
                else
                {
                    typeMap = ResolveTypeMap(pair);
                    // we want the exact map the user included, but we could instantiate an open generic
                    if(typeMap == null || typeMap.Types != pair)
                    {
                        throw QueryMapperHelper.MissingMapException(pair);
                    }
                    yield return typeMap;
                }
            }
        }

        private TypeMap FindClosedGenericTypeMapFor(TypePair initialTypes, TypePair typePair)
        {
            var genericTypePair = typePair.GetOpenGenericTypePair();
            if(genericTypePair == null)
            {
                return null;
            }
            ITypeMapConfiguration genericMap;
            ProfileMap profile;
            TypeMap cachedMap = null;
            var userMap = FindTypeMapFor(genericTypePair.Value);
            if(userMap?.DestinationTypeOverride != null)
            {
                genericTypePair = new TypePair(genericTypePair.Value.SourceType, userMap.DestinationTypeOverride).GetOpenGenericTypePair();
                if(genericTypePair == null)
                {
                    return null;
                }
                userMap = null;
            }
            if(userMap == null && (cachedMap = GetCachedMap(initialTypes, genericTypePair.Value)) != null)
            {
                if(!cachedMap.Types.IsGeneric)
                {
                    return cachedMap;
                }

                genericMap = cachedMap.Profile.GetGenericMap(cachedMap.Types);
                profile = cachedMap.Profile;
                typePair = cachedMap.Types.CloseGenericTypes(typePair);
            }
            else if (userMap == null)
            {
                var item = Profiles
                    .Select(p => new {GenericMap = p.GetGenericMap(typePair), Profile = p})
                    .FirstOrDefault(p => p.GenericMap != null);
                genericMap = item?.GenericMap;
                profile = item?.Profile;
            }
            else
            {
                genericMap = userMap.Profile.GetGenericMap(typePair);
                profile = userMap.Profile;
            }

            if(genericMap == null)
            {
                return null;
            }
            TypeMap typeMap;
            lock(this)
            {
                typeMap = profile.CreateClosedGenericTypeMap(genericMap, typePair, this);
            }
            cachedMap?.CopyInheritedMapsTo(typeMap);
            return typeMap;
        }

        public IObjectMapper FindMapper(TypePair types) =>_mappers.FirstOrDefault(m => m.IsMatch(types));

        public void RegisterTypeMap(TypeMap typeMap) => _typeMapRegistry[typeMap.Types] = typeMap;
    }

    public readonly struct ValidationContext
    {
        public IObjectMapper ObjectMapper { get; }
        public IMemberMap MemberMap { get; }
        public TypeMap TypeMap { get; }
        public TypePair Types { get; }

        public ValidationContext(TypePair types, IMemberMap memberMap, IObjectMapper objectMapper) : this(types, memberMap, objectMapper, null)
        {
        }

        public ValidationContext(TypePair types, IMemberMap memberMap, TypeMap typeMap) : this(types, memberMap, null, typeMap)
        {
        }

        private ValidationContext(TypePair types, IMemberMap memberMap, IObjectMapper objectMapper, TypeMap typeMap)
        {
            ObjectMapper = objectMapper;
            TypeMap = typeMap;
            Types = types;
            MemberMap = memberMap;
        }
    }
}
