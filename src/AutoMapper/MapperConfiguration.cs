using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Configuration;
using AutoMapper.Features;
using AutoMapper.Internal;
using AutoMapper.QueryableExtensions.Impl;

namespace AutoMapper
{
    using static Expression;
    using static ExpressionFactory;
    using static Execution.ExpressionBuilder;

    public class MapperConfiguration : IGlobalConfiguration
    {
        private static readonly MethodInfo MappingError = typeof(MapperConfiguration).GetMethod("GetMappingError");

        private readonly IObjectMapper[] _mappers;
        private readonly Dictionary<TypePair, TypeMap> _configuredMaps;
        private readonly Dictionary<TypePair, TypeMap> _resolvedMaps;
        private readonly Dictionary<TypePair, IObjectMapper> _resolvedMappers;
        private readonly LockingConcurrentDictionary<TypePair, TypeMap> _runtimeMaps;
        private readonly ProjectionBuilder _projectionBuilder;
        private readonly LockingConcurrentDictionary<MapRequest, Delegate> _executionPlans;
        private readonly ConfigurationValidator _validator;
        private readonly Features<IRuntimeFeature> _features = new();
        private readonly int _recursiveQueriesMaxDepth;
        private readonly IProjectionMapper[] _projectionMappers;
        private readonly int _maxExecutionPlanDepth;
        private readonly bool _enableNullPropagationForQueryMapping;
        private readonly Func<Type, object> _serviceCtor;
        private readonly bool _sealed;

        public MapperConfiguration(MapperConfigurationExpression configurationExpression)
        {
            var configuration = (IGlobalConfigurationExpression)configurationExpression;
            if (configuration.MethodMappingEnabled != false)
            {
                configuration.IncludeSourceExtensionMethods(typeof(Enumerable));
            }
            _mappers = configuration.Mappers.ToArray();
            _executionPlans = new(CompileExecutionPlan);
            _validator = new(this, configuration);
            _projectionBuilder = new(this);

            _serviceCtor = configuration.ServiceCtor;
            _enableNullPropagationForQueryMapping = configuration.EnableNullPropagationForQueryMapping ?? false;
            _maxExecutionPlanDepth = configuration.MaxExecutionPlanDepth + 1;
            _projectionMappers = configuration.ProjectionMappers.ToArray();
            _recursiveQueriesMaxDepth = configuration.RecursiveQueriesMaxDepth;

            Configuration = new((IProfileConfiguration)configuration);
            var profileMaps = new List<ProfileMap>(configuration.Profiles.Count + 1){ Configuration };
            int typeMapsCount = Configuration.TypeMapsCount;
            int openTypeMapsCount = Configuration.OpenTypeMapsCount;
            foreach (var profile in configuration.Profiles)
            {
                var profileMap = new ProfileMap(profile, configuration);
                typeMapsCount += profileMap.TypeMapsCount;
                openTypeMapsCount += profileMap.OpenTypeMapsCount;
                profileMaps.Add(profileMap);
            }
            Profiles = profileMaps;
            _configuredMaps = new(typeMapsCount);
            _runtimeMaps = new(GetTypeMap, openTypeMapsCount);
            _resolvedMaps = new(2 * typeMapsCount);
            _resolvedMappers = new(typeMapsCount);
            configuration.Features.Configure(this);

            foreach (var beforeSealAction in configuration.BeforeSealActions)
            {
                beforeSealAction.Invoke(this);
            }
            Seal();
            foreach (var profile in Profiles)
            {
                profile.Clear();
            }
            _sealed = true;
        }
        public MapperConfiguration(Action<IMapperConfigurationExpression> configure)
            : this(Build(configure))
        {
        }
        public void AssertConfigurationIsValid() => _validator.AssertConfigurationExpressionIsValid(_configuredMaps.Values);
        public IMapper CreateMapper() => new Mapper(this);
        public IMapper CreateMapper(Func<Type, object> serviceCtor) => new Mapper(this, serviceCtor);
        public void CompileMappings()
        {
            foreach (var request in _resolvedMaps.Keys.Where(t => !t.IsGenericTypeDefinition).Select(types => new MapRequest(types, types)).ToArray())
            {
                GetExecutionPlan(request);
            }
        }
        public LambdaExpression BuildExecutionPlan(Type sourceType, Type destinationType)
        {
            var typePair = new TypePair(sourceType, destinationType);
            return this.Internal().BuildExecutionPlan(new(typePair, typePair));
        }
        LambdaExpression IGlobalConfiguration.BuildExecutionPlan(in MapRequest mapRequest)
        {
            var typeMap = ResolveTypeMap(mapRequest.RuntimeTypes) ?? ResolveTypeMap(mapRequest.RequestedTypes);
            if (typeMap != null)
            {
                return GenerateTypeMapExpression(mapRequest, typeMap);
            }
            var mapperToUse = FindMapper(mapRequest.RuntimeTypes);
            return GenerateObjectMapperExpression(mapRequest, mapperToUse);
        }

        private static MapperConfigurationExpression Build(Action<IMapperConfigurationExpression> configure)
        {
            var expr = new MapperConfigurationExpression();
            configure(expr);
            return expr;
        }

        IProjectionBuilder IGlobalConfiguration.ProjectionBuilder => _projectionBuilder;
        Func<Type, object> IGlobalConfiguration.ServiceCtor => _serviceCtor;
        bool IGlobalConfiguration.EnableNullPropagationForQueryMapping => _enableNullPropagationForQueryMapping;
        int IGlobalConfiguration.MaxExecutionPlanDepth => _maxExecutionPlanDepth;
        private ProfileMap Configuration { get; }
        internal List<ProfileMap> Profiles { get; }
        IProjectionMapper[] IGlobalConfiguration.ProjectionMappers => _projectionMappers;
        int IGlobalConfiguration.RecursiveQueriesMaxDepth => _recursiveQueriesMaxDepth;
        Features<IRuntimeFeature> IGlobalConfiguration.Features => _features;
        Func<TSource, TDestination, ResolutionContext, TDestination> IGlobalConfiguration.GetExecutionPlan<TSource, TDestination>(in MapRequest mapRequest)
            => (Func<TSource, TDestination, ResolutionContext, TDestination>)GetExecutionPlan(mapRequest);

        private Delegate GetExecutionPlan(in MapRequest mapRequest) => _executionPlans.GetOrAdd(mapRequest);

        private Delegate CompileExecutionPlan(MapRequest mapRequest)
        {
            var executionPlan = ((IGlobalConfiguration)this).BuildExecutionPlan(mapRequest);
            return executionPlan.Compile(); // breakpoint here to inspect all execution plans
        }

        TypeMap IGlobalConfiguration.ResolveAssociatedTypeMap(in TypePair types)
        {
            var typeMap = ResolveTypeMap(types);
            if (typeMap != null)
            {
                return typeMap;
            }
            var mapper = FindMapper(types);
            if (mapper is IObjectMapperInfo objectMapperInfo)
            {
                return ResolveTypeMap(objectMapperInfo.GetAssociatedTypes(types));
            }
            return null;
        }

        private static LambdaExpression GenerateTypeMapExpression(in MapRequest mapRequest, TypeMap typeMap)
        {
            typeMap.CheckProjection();
            if (mapRequest.RequestedTypes == typeMap.Types)
            {
                return typeMap.MapExpression;
            }
            var mapDestinationType = typeMap.DestinationType;
            var requestedDestinationType = mapRequest.RequestedTypes.DestinationType;
            var source = Parameter(mapRequest.RequestedTypes.SourceType, "source");
            var destination = Parameter(requestedDestinationType, "typeMapDestination");
            var checkNullValueTypeDest = CheckNullValueType(destination, mapDestinationType);
            return
                Lambda(
                    ToType(
                        Invoke(typeMap.MapExpression, ToType(source, typeMap.SourceType), ToType(checkNullValueTypeDest, mapDestinationType), ContextParameter),
                        requestedDestinationType),
                source, destination, ContextParameter);
        }
        private static Expression CheckNullValueType(Expression expression, Type runtimeType) =>
            !expression.Type.IsValueType && runtimeType.IsValueType ? Coalesce(expression, New(runtimeType)) : expression;
        private LambdaExpression GenerateObjectMapperExpression(in MapRequest mapRequest, IObjectMapper mapperToUse)
        {
            var source = Parameter(mapRequest.RequestedTypes.SourceType, "source");
            var destination = Parameter(mapRequest.RequestedTypes.DestinationType, "mapperDestination");
            var runtimeDestinationType = mapRequest.RuntimeTypes.DestinationType;
            Expression fullExpression;
            if (mapperToUse == null)
            {
                var exception = new AutoMapperMappingException("Missing type map configuration or unsupported mapping.", null, mapRequest.RuntimeTypes)
                {
                    MemberMap = mapRequest.MemberMap
                };
                fullExpression = Throw(Constant(exception), runtimeDestinationType);
            }
            else
            {
                var checkNullValueTypeDest = CheckNullValueType(destination, runtimeDestinationType);
                var map = mapperToUse.MapExpression(this, Configuration, mapRequest.MemberMap,
                                                                        ToType(source, mapRequest.RuntimeTypes.SourceType),
                                                                        ToType(checkNullValueTypeDest, runtimeDestinationType));
                var newException = Call(MappingError, ExceptionParameter, Constant(mapRequest.RuntimeTypes), Constant(mapRequest.MemberMap, typeof(IMemberMap)));
                var throwExpression = Throw(newException, runtimeDestinationType);
                fullExpression = TryCatch(ToType(map, runtimeDestinationType), Catch(ExceptionParameter, throwExpression));
            }
            var profileMap = mapRequest.MemberMap?.TypeMap?.Profile ?? Configuration;
            var nullCheckSource = NullCheckSource(profileMap, source, destination, fullExpression, mapRequest.MemberMap);
            return Lambda(nullCheckSource, source, destination, ContextParameter);
        }
        public static AutoMapperMappingException GetMappingError(Exception innerException, TypePair types, IMemberMap memberMap) =>
            new("Error mapping types.", innerException, types) { MemberMap = memberMap };
        IReadOnlyCollection<TypeMap> IGlobalConfiguration.GetAllTypeMaps() => _configuredMaps.Values;
        TypeMap IGlobalConfiguration.FindTypeMapFor(Type sourceType, Type destinationType) => FindTypeMapFor(sourceType, destinationType);
        TypeMap IGlobalConfiguration.FindTypeMapFor<TSource, TDestination>() => FindTypeMapFor(typeof(TSource), typeof(TDestination));
        TypeMap IGlobalConfiguration.FindTypeMapFor(in TypePair typePair) => FindTypeMapFor(typePair);
        TypeMap FindTypeMapFor(Type sourceType, Type destinationType) => FindTypeMapFor(new(sourceType, destinationType));
        TypeMap FindTypeMapFor(in TypePair typePair) => _configuredMaps.GetOrDefault(typePair);
        TypeMap IGlobalConfiguration.ResolveTypeMap(Type sourceType, Type destinationType) => ResolveTypeMap(new(sourceType, destinationType));
        TypeMap IGlobalConfiguration.ResolveTypeMap(in TypePair typePair) => ResolveTypeMap(typePair);
        TypeMap ResolveTypeMap(in TypePair typePair)
        {
            if (_resolvedMaps.TryGetValue(typePair, out TypeMap typeMap))
            {
                return typeMap;
            }
            if (_sealed)
            {
                typeMap = _runtimeMaps.GetOrAdd(typePair);
                // if it's a dynamically created type map, we need to seal it outside GetTypeMap to handle recursion
                if (typeMap != null && typeMap.MapExpression == null)
                {
                    lock (typeMap)
                    {
                        typeMap.Seal(this);
                    }
                }
            }
            else
            {
                typeMap = GetTypeMap(typePair);
                _resolvedMaps.Add(typePair, typeMap);
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
            var allSourceTypes = GetTypeInheritance(initialTypes.SourceType);
            var allDestinationTypes = GetTypeInheritance(initialTypes.DestinationType);
            foreach (var destinationType in allDestinationTypes)
            {
                foreach (var sourceType in allSourceTypes)
                {
                    if (sourceType == initialTypes.SourceType && destinationType == initialTypes.DestinationType)
                    {
                        continue;
                    }
                    var types = new TypePair(sourceType, destinationType);
                    if (_resolvedMaps.TryGetValue(types, out typeMap))
                    {
                        return typeMap;
                    }
                    typeMap = FindClosedGenericTypeMapFor(types);
                    if (typeMap != null)
                    {
                        return typeMap;
                    }
                }
            }
            return null;
        }
        static List<Type> GetTypeInheritance(Type type)
        {
            var interfaces = type.GetInterfaces();
            var lastIndex = interfaces.Length - 1;
            var types = new List<Type>(interfaces.Length + 2) { type };
            Type baseType = type;
            while ((baseType = baseType.BaseType) != null)
            {
                types.Add(baseType);
                foreach (var interfaceType in baseType.GetInterfaces())
                {
                    var interfaceIndex = Array.LastIndexOf(interfaces, interfaceType);
                    if (interfaceIndex != lastIndex)
                    {
                        interfaces[interfaceIndex] = interfaces[lastIndex];
                        interfaces[lastIndex] = interfaceType;
                    }
                }
            }
            foreach (var interfaceType in interfaces)
            {
                types.Add(interfaceType);
            }
            return types;
        }
        IEnumerable<IObjectMapper> IGlobalConfiguration.GetMappers() => _mappers;
        private void Seal()
        {
            foreach (var profile in Profiles)
            {
                profile.Register(this);
            }
            foreach (var profile in Profiles)
            {
                profile.Configure(this);
            }
            IGlobalConfiguration globalConfiguration = this;
            var derivedMaps = new List<TypeMap>();
            foreach (var typeMap in _configuredMaps.Values)
            {
                if (typeMap.DestinationTypeOverride != null)
                {
                    var derivedMap = globalConfiguration.GetIncludedTypeMap(typeMap.SourceType, typeMap.DestinationTypeOverride);
                    _resolvedMaps[typeMap.Types] = derivedMap;
                }
                else
                {
                    _resolvedMaps[typeMap.Types] = typeMap;
                }
                derivedMaps.Clear();
                GetDerivedTypeMaps(typeMap,derivedMaps);
                foreach (var derivedMap in derivedMaps)
                {
                    var includedPair = new TypePair(derivedMap.SourceType, typeMap.DestinationType);
                    if (!_resolvedMaps.ContainsKey(includedPair))
                    {
                        _resolvedMaps[includedPair] = derivedMap;
                    }
                }
            }
            foreach (var typeMap in _configuredMaps.Values)
            {
                typeMap.Seal(this);
            }
            _features.Seal(this);
        }

        private void GetDerivedTypeMaps(TypeMap typeMap, List<TypeMap> typeMaps)
        {
            foreach (var derivedMap in this.Internal().GetIncludedTypeMaps(typeMap))
            {
                typeMaps.Add(derivedMap);
                GetDerivedTypeMaps(derivedMap, typeMaps);
            }
        }

        TypeMap[] IGlobalConfiguration.GetIncludedTypeMaps(IReadOnlyCollection<TypePair> includedTypes)
        {
            if (includedTypes.Count == 0)
            {
                return Array.Empty<TypeMap>();
            }
            var typeMaps = new TypeMap[includedTypes.Count];
            int index = 0;
            foreach (var pair in includedTypes)
            {
                typeMaps[index] = GetIncludedTypeMap(pair);
                index++;
            }
            return typeMaps;
        }
        TypeMap IGlobalConfiguration.GetIncludedTypeMap(Type sourceType, Type destinationType) => GetIncludedTypeMap(new(sourceType, destinationType));
        TypeMap IGlobalConfiguration.GetIncludedTypeMap(in TypePair pair) => GetIncludedTypeMap(pair);
        TypeMap GetIncludedTypeMap(in TypePair pair)
        {
            var typeMap = FindTypeMapFor(pair);
            if (typeMap != null)
            {
                return typeMap;
            }
            else
            {
                typeMap = ResolveTypeMap(pair);
                // we want the exact map the user included, but we could instantiate an open generic
                if (typeMap?.Types != pair)
                {
                    throw QueryMapperHelper.MissingMapException(pair);
                }
                return typeMap;
            }
        }
        private TypeMap FindClosedGenericTypeMapFor(in TypePair typePair)
        {
            if (!typePair.IsGeneric)
            {
                return null;
            }
            var genericTypePair = typePair.GetTypeDefinitionIfGeneric();
            var userMap =
                FindTypeMapFor(genericTypePair.SourceType, typePair.DestinationType) ??
                FindTypeMapFor(typePair.SourceType, genericTypePair.DestinationType) ??
                FindTypeMapFor(genericTypePair);
            ITypeMapConfiguration genericMapConfig;
            ProfileMap profile;
            TypeMap cachedMap;
            TypePair closedTypes;
            if (userMap != null && userMap.DestinationTypeOverride == null)
            {
                genericMapConfig = userMap.Profile.GetGenericMap(userMap.Types);
                profile = userMap.Profile;
                cachedMap = null;
                closedTypes = typePair;
            }
            else
            {
                var foundGenericMap = _resolvedMaps.TryGetValue(genericTypePair, out cachedMap) && cachedMap.Types.ContainsGenericParameters;
                if (!foundGenericMap)
                {
                    return cachedMap;
                }
                genericMapConfig = cachedMap.Profile.GetGenericMap(cachedMap.Types);
                profile = cachedMap.Profile;
                closedTypes = cachedMap.Types.CloseGenericTypes(typePair);
            }
            if (genericMapConfig == null)
            {
                return null;
            }
            var typeMap = profile.CreateClosedGenericTypeMap(genericMapConfig, closedTypes, this);
            cachedMap?.CopyInheritedMapsTo(typeMap);
            return typeMap;
        }
        IObjectMapper IGlobalConfiguration.FindMapper(in TypePair types) => FindMapper(types);
        IObjectMapper FindMapper(in TypePair types)
        {
            if (_resolvedMappers.TryGetValue(types, out var resolvedMapper))
            {
                return resolvedMapper;
            }
            foreach (var mapper in _mappers)
            {
                if (mapper.IsMatch(types))
                {
                    if (!_sealed)
                    {
                        _resolvedMappers.Add(types, mapper);
                    }
                    return mapper;
                }
            }
            return null;
        }
        void IGlobalConfiguration.RegisterTypeMap(TypeMap typeMap) => _configuredMaps[typeMap.Types] = typeMap;
        void IGlobalConfiguration.AssertConfigurationIsValid(TypeMap typeMap) => _validator.AssertConfigurationIsValid(new[] { typeMap });
        void IGlobalConfiguration.AssertConfigurationIsValid(string profileName)
        {
            if (Profiles.All(x => x.Name != profileName))
            {
                throw new ArgumentOutOfRangeException(nameof(profileName), $"Cannot find any profiles with the name '{profileName}'.");
            }
            _validator.AssertConfigurationIsValid(_configuredMaps.Values.Where(typeMap => typeMap.Profile.Name == profileName));
        }
        void IGlobalConfiguration.AssertConfigurationIsValid<TProfile>() => this.Internal().AssertConfigurationIsValid(typeof(TProfile).FullName);
        IEnumerable<ProfileMap> IGlobalConfiguration.GetProfiles() => Profiles;
    }
}