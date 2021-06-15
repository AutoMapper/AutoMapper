using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

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

    public class MapperConfiguration : IConfigurationProvider
    {
        private static readonly ConstructorInfo ExceptionConstructor = typeof(AutoMapperMappingException).GetDeclaredConstructors().Single(c => c.GetParameters().Length == 3);

        private readonly IObjectMapper[] _mappers;
        private readonly Dictionary<TypePair, TypeMap> _configuredMaps = new Dictionary<TypePair, TypeMap>();
        private LockingConcurrentDictionary<TypePair, TypeMap> _resolvedMaps;
        private readonly LockingConcurrentDictionary<MapRequest, Delegate> _executionPlans;
        private readonly ConfigurationValidator _validator;
        private readonly Func<MapRequest,MethodBuilder> _methodBuilderFactory;

        public MapperConfiguration(MapperConfigurationExpression configurationExpression)
        {
            MustBeGeneratedCompatible = configurationExpression.MustBeGeneratedCompatible;
            _methodBuilderFactory = configurationExpression.MethodBuilderFactory;

            _mappers = configurationExpression.Mappers.ToArray();
            _resolvedMaps = new LockingConcurrentDictionary<TypePair, TypeMap>(GetTypeMap);
            _executionPlans = new LockingConcurrentDictionary<MapRequest, Delegate>(CompileExecutionPlan);
            _validator = new ConfigurationValidator(this, configurationExpression);
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

        private static MapperConfigurationExpression Build(Action<IMapperConfigurationExpression> configure)
        {
            var expr = new MapperConfigurationExpression();
            configure(expr);
            return expr;
        }

        public bool MustBeGeneratedCompatible { get; set; }

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
            foreach (var request in _resolvedMaps.Keys.Where(t=>!t.IsGenericTypeDefinition).Select(types => new MapRequest(types, types)).ToArray())
            {
                GetExecutionPlan(request);
            }
        }

        private Delegate GetExecutionPlan(MapRequest mapRequest) => _executionPlans.GetOrAdd(mapRequest);

        private Delegate CompileExecutionPlan(MapRequest mapRequest)
        {
            LambdaExpression executionPlan = BuildExecutionPlan(mapRequest);
#if !NETSTANDARD
            if (!MustBeGeneratedCompatible || !(_methodBuilderFactory is { } mb))
            {
                return executionPlan.Compile();
            }

            MethodBuilder method = _methodBuilderFactory(mapRequest); 
            executionPlan.CompileToMethod(method);
            Action p = () => { };
            return p;

#else
            return executionPlan.Compile(); // breakpoint here to inspect all execution plans
#endif
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
            if (mapRequest.RequestedTypes == typeMap.Types)
            {
                return typeMap.MapExpression;
            }
            var mapDestinationType = typeMap.DestinationType;
            var requestedDestinationType = mapRequest.RequestedTypes.DestinationType;
            var source = Parameter(mapRequest.RequestedTypes.SourceType, "source");
            var destination = Parameter(requestedDestinationType, "typeMapDestination");
            var context = Parameter(typeof(ResolutionContext), "context");
            var checkNullValueTypeDest = CheckNullValueType(destination, mapDestinationType);
            return
                Lambda(
                    ToType(
                        Invoke(typeMap.MapExpression, ToType(source, typeMap.SourceType), ToType(checkNullValueTypeDest, mapDestinationType), context),
                        requestedDestinationType), 
                source, destination, context);
        }
        private static Expression CheckNullValueType(Expression expression, Type runtimeType) =>
            !expression.Type.IsValueType && runtimeType.IsValueType ? Coalesce(expression, New(runtimeType)) : expression;
        private LambdaExpression GenerateObjectMapperExpression(MapRequest mapRequest, IObjectMapper mapperToUse)
        {
            var destinationType = mapRequest.RequestedTypes.DestinationType;
            var source = Parameter(mapRequest.RequestedTypes.SourceType, "source");
            var destination = Parameter(destinationType, "mapperDestination");
            var context = Parameter(typeof(ResolutionContext), "context");
            Expression fullExpression;
            if (mapperToUse == null)
            {
                fullExpression = Throw("Missing type map configuration or unsupported mapping.", Constant(null, typeof(Exception)));
            }
            else
            {
                var runtimeDestinationType = mapRequest.RuntimeTypes.DestinationType;
                var checkNullValueTypeDest = CheckNullValueType(destination, runtimeDestinationType);
                var map = mapperToUse.MapExpression(this, Configuration, mapRequest.MemberMap, 
                                                                        ToType(source, mapRequest.RuntimeTypes.SourceType), 
                                                                        ToType(checkNullValueTypeDest, runtimeDestinationType), 
                                                                        context);
                var exception = Parameter(typeof(Exception), "ex");
                fullExpression = TryCatch(ToType(map, destinationType), Catch(exception, Throw("Error mapping types.", exception)));
            }
            var profileMap = mapRequest.MemberMap?.TypeMap?.Profile ?? Configuration;
            var nullCheckSource = NullCheckSource(profileMap, source, destination, fullExpression, mapRequest.MemberMap);
            return Lambda(nullCheckSource, source, destination, context);
            BlockExpression Throw(string message, Expression innerException) =>
                Block(Expression.Throw(New(ExceptionConstructor, Constant(message), innerException, TypePairToExpression(mapRequest.RequestedTypes))), Default(destinationType));
        }
        public TypeMap[] GetAllTypeMaps() => _configuredMaps.Values.ToArray();

        public TypeMap FindTypeMapFor(Type sourceType, Type destinationType) => FindTypeMapFor(new TypePair(sourceType, destinationType));

        public TypeMap FindTypeMapFor<TSource, TDestination>() => FindTypeMapFor(new TypePair(typeof(TSource), typeof(TDestination)));

        public TypeMap FindTypeMapFor(TypePair typePair) => _configuredMaps.GetOrDefault(typePair);

        public TypeMap ResolveTypeMap(Type sourceType, Type destinationType) => ResolveTypeMap(new TypePair(sourceType, destinationType));

        public TypeMap ResolveTypeMap(TypePair typePair)
        {
            var typeMap = _resolvedMaps.GetOrAdd(typePair);
            // if it's a dynamically created type map, we need to seal it outside GetTypeMap to handle recursion
            if (typeMap != null && typeMap.MapExpression == null && _configuredMaps.GetOrDefault(typePair) == null)
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
            var typeMap = FindClosedGenericTypeMapFor(initialTypes);
            if (typeMap != null)
            {
                return typeMap;
            }
            foreach (var types in initialTypes.GetRelatedTypePairs().Skip(1))
            {
                typeMap = GetCachedMap(types);
                if (typeMap != null)
                {
                    return typeMap;
                }
                typeMap = FindClosedGenericTypeMapFor(types);
                if (typeMap != null)
                {
                    return typeMap;
                }
            }
            return null;
        }

        private TypeMap GetCachedMap(TypePair types) => _resolvedMaps.GetOrDefault(types);

        public IMapper CreateMapper() => new Mapper(this);

        public IMapper CreateMapper(Func<Type, object> serviceCtor) => new Mapper(this, serviceCtor);

        public IEnumerable<IObjectMapper> GetMappers() => _mappers;

        private void Seal()
        {
            var derivedMaps = new List<Tuple<TypePair, TypeMap>>();
            var redirectedTypes = new List<Tuple<TypePair, TypePair>>();

            foreach (var profile in Profiles)
            {
                profile.Register(this);
            }

            foreach (var typeMap in _configuredMaps.Values.Where(tm => tm.IncludeAllDerivedTypes))
            {
                foreach (var derivedMap in _configuredMaps
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

            foreach (var typeMap in _configuredMaps.Values)
            {
                _resolvedMaps[typeMap.Types] = typeMap;

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
                    _resolvedMaps[redirectedType.Item1] = derivedMap;
                }
            }
            foreach (var derivedMap in derivedMaps.Where(derivedMap => !_resolvedMaps.ContainsKey(derivedMap.Item1)))
            {
                _resolvedMaps[derivedMap.Item1] = derivedMap.Item2;
            }

            foreach (var typeMap in _configuredMaps.Values)
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

        private TypeMap FindClosedGenericTypeMapFor(TypePair typePair)
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
            if(userMap == null && (cachedMap = GetCachedMap(genericTypePair.Value)) != null)
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

        public IObjectMapper FindMapper(TypePair types)
        {
            foreach (var mapper in _mappers)
            {
                if (mapper.IsMatch(types))
                {
                    return mapper;
                }
            }
            return null;
        }

        public void RegisterTypeMap(TypeMap typeMap) => _configuredMaps[typeMap.Types] = typeMap;

        public void AssertConfigurationIsValid(TypeMap typeMap) => _validator.AssertConfigurationIsValid(new[] { typeMap });

        public void AssertConfigurationIsValid(string profileName)
        {
            if (Profiles.All(x => x.Name != profileName))
            {
                throw new ArgumentOutOfRangeException(nameof(profileName), $"Cannot find any profiles with the name '{profileName}'.");
            }
            _validator.AssertConfigurationIsValid(_configuredMaps.Values.Where(typeMap => typeMap.Profile.Name == profileName));
        }

        public void AssertConfigurationIsValid<TProfile>() where TProfile : Profile, new() => AssertConfigurationIsValid(new TProfile().ProfileName);

        public void AssertConfigurationIsValid() => _validator.AssertConfigurationExpressionIsValid(_configuredMaps.Values);
    }
}