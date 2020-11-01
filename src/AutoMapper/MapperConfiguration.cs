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
        private static readonly ConstructorInfo ExceptionConstructor = typeof(AutoMapperMappingException).GetDeclaredConstructors().Single(c => c.GetParameters().Length == 3);

        private readonly IObjectMapper[] _mappers;
        private readonly Dictionary<TypePair, TypeMap> _configuredMaps;
        private LockingConcurrentDictionary<TypePair, TypeMap> _resolvedMaps;
        private readonly IProjectionBuilder _projectionBuilder;
        private readonly LockingConcurrentDictionary<MapRequest, Delegate> _executionPlans;
        private readonly ConfigurationValidator _validator;
        private readonly Features<IRuntimeFeature> _features = new Features<IRuntimeFeature>();
        private readonly int _recursiveQueriesMaxDepth;
        private readonly IProjectionMapper[] _projectionMappers;
        private readonly int _maxExecutionPlanDepth;
        private readonly bool _enableNullPropagationForQueryMapping;
        private readonly Func<Type, object> _serviceCtor;

        public MapperConfiguration(MapperConfigurationExpression configurationExpression)
        {
            var configuration = (IGlobalConfigurationExpression)configurationExpression;
            foreach (var extensionMehodsType in configuration.SourceExtensionMethodsTypes ?? new[] { typeof(Enumerable) })
            {
                configuration.IncludeSourceExtensionMethods(extensionMehodsType);
            }
            _mappers = configuration.Mappers.ToArray();
            _executionPlans = new LockingConcurrentDictionary<MapRequest, Delegate>(CompileExecutionPlan);
            _validator = new ConfigurationValidator(this, configuration);
            _projectionBuilder = new ProjectionBuilder(this);

            _serviceCtor = configuration.ServiceCtor;
            _enableNullPropagationForQueryMapping = configuration.EnableNullPropagationForQueryMapping ?? false;
            _maxExecutionPlanDepth = configuration.MaxExecutionPlanDepth + 1;
            _projectionMappers = configuration.ProjectionMappers.ToArray();
            _recursiveQueriesMaxDepth = configuration.RecursiveQueriesMaxDepth;

            Configuration = new ProfileMap((IProfileConfiguration)configuration);
            var profileMaps = new List<ProfileMap>(configuration.Profiles.Count + 1){ Configuration };
            int typeMapsCount = Configuration.TypeMapsCount;
            foreach (var profile in configuration.Profiles)
            {
                var profileMap = new ProfileMap(profile, configuration);
                typeMapsCount += profileMap.TypeMapsCount;
                profileMaps.Add(profileMap);
            }
            Profiles = profileMaps;
            _configuredMaps = new Dictionary<TypePair, TypeMap>(typeMapsCount);
            _resolvedMaps = new LockingConcurrentDictionary<TypePair, TypeMap>(GetTypeMap, 2*typeMapsCount);
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
            return this.Internal().BuildExecutionPlan(new MapRequest(typePair, typePair));
        }
        LambdaExpression IGlobalConfiguration.BuildExecutionPlan(in MapRequest mapRequest)
        {
            var typeMap = ResolveTypeMap(mapRequest.RuntimeTypes) ?? ResolveTypeMap(mapRequest.RequestedTypes);
            if (typeMap != null)
            {
                return GenerateTypeMapExpression(mapRequest, typeMap);
            }
            var mapperToUse = ((IGlobalConfiguration)this).FindMapper(mapRequest.RuntimeTypes);
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

        private static LambdaExpression GenerateTypeMapExpression(in MapRequest mapRequest, TypeMap typeMap)
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
        private LambdaExpression GenerateObjectMapperExpression(in MapRequest mapRequest, IObjectMapper mapperToUse)
        {
            var destinationType = mapRequest.RequestedTypes.DestinationType;
            var source = Parameter(mapRequest.RequestedTypes.SourceType, "source");
            var destination = Parameter(destinationType, "mapperDestination");
            var context = Parameter(typeof(ResolutionContext), "context");
            Expression fullExpression;
            if (mapperToUse == null)
            {
                fullExpression = Throw(mapRequest, "Missing type map configuration or unsupported mapping.", Constant(null, typeof(Exception)));
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
                fullExpression = TryCatch(ToType(map, destinationType), Catch(exception, Throw(mapRequest, "Error mapping types.", exception)));
            }
            var profileMap = mapRequest.MemberMap?.TypeMap?.Profile ?? Configuration;
            var nullCheckSource = NullCheckSource(profileMap, source, destination, fullExpression, mapRequest.MemberMap);
            return Lambda(nullCheckSource, source, destination, context);
            static BlockExpression Throw(in MapRequest mapRequest, string message, Expression innerException) =>
                Block(Expression.Throw(New(ExceptionConstructor, Constant(message), innerException, Constant(mapRequest.RequestedTypes))), Default(mapRequest.RequestedTypes.DestinationType));
        }
        TypeMap[] IGlobalConfiguration.GetAllTypeMaps() => _configuredMaps.Values.ToArray();
        TypeMap IGlobalConfiguration.FindTypeMapFor(Type sourceType, Type destinationType) => FindTypeMapFor(new TypePair(sourceType, destinationType));
        TypeMap IGlobalConfiguration.FindTypeMapFor<TSource, TDestination>() => FindTypeMapFor(new TypePair(typeof(TSource), typeof(TDestination)));
        TypeMap IGlobalConfiguration.FindTypeMapFor(in TypePair typePair) => FindTypeMapFor(typePair);
        TypeMap FindTypeMapFor(in TypePair typePair) => _configuredMaps.GetOrDefault(typePair);
        TypeMap IGlobalConfiguration.ResolveTypeMap(Type sourceType, Type destinationType) => ResolveTypeMap(new TypePair(sourceType, destinationType));
        TypeMap IGlobalConfiguration.ResolveTypeMap(in TypePair typePair) => ResolveTypeMap(typePair);
        TypeMap ResolveTypeMap(in TypePair typePair)
        {
            var typeMap = _resolvedMaps.GetOrAdd(typePair);
            // if it's a dynamically created type map, we need to seal it outside GetTypeMap to handle recursion
            if (typeMap != null && typeMap.MapExpression == null && !_configuredMaps.ContainsKey(typePair))
            {
                lock (typeMap)
                {
                    typeMap.Seal(this);
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
        IEnumerable<IObjectMapper> IGlobalConfiguration.GetMappers() => _mappers;

        private void Seal()
        {
            foreach (var profile in Profiles)
            {
                profile.Register(this);
            }
            foreach (var typeMap in _configuredMaps.Values.Where(tm => tm.IncludeAllDerivedTypes))
            {
                foreach (var derivedMap in _configuredMaps
                    .Where(tm =>
                        typeMap != tm.Value &&
                        typeMap.SourceType.IsAssignableFrom(tm.Key.SourceType) &&
                        typeMap.DestinationType.IsAssignableFrom(tm.Key.DestinationType))
                    .Select(tm => tm.Value))
                {
                    typeMap.IncludeDerivedTypes(derivedMap.SourceType, derivedMap.DestinationType);
                }
            }
            foreach (var profile in Profiles)
            {
                profile.Configure(this);
            }
            IGlobalConfiguration globalConfiguration = this;
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
                var derivedMaps = GetDerivedTypeMaps(typeMap);
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

        private IEnumerable<TypeMap> GetDerivedTypeMaps(TypeMap typeMap)
        {
            foreach (var derivedMap in this.Internal().GetIncludedTypeMaps(typeMap.IncludedDerivedTypes))
            {
                yield return derivedMap;
                foreach (var derivedTypeMap in GetDerivedTypeMaps(derivedMap))
                {
                    yield return derivedTypeMap;
                }
            }
        }

        IEnumerable<TypeMap> IGlobalConfiguration.GetIncludedTypeMaps(IEnumerable<TypePair> includedTypes) => includedTypes.Select(this.Internal().GetIncludedTypeMap);
        TypeMap IGlobalConfiguration.GetIncludedTypeMap(Type sourceType, Type destinationType) => this.Internal().GetIncludedTypeMap(new TypePair(sourceType, destinationType));
        TypeMap IGlobalConfiguration.GetIncludedTypeMap(TypePair pair)
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
                if (typeMap == null || typeMap.Types != pair)
                {
                    throw QueryMapperHelper.MissingMapException(pair);
                }
                return typeMap;
            }
        }
        private TypeMap FindClosedGenericTypeMapFor(TypePair typePair)
        {
            var genericTypePair = typePair.GetOpenGenericTypePair();
            if (genericTypePair == null)
            {
                return null;
            }
            ITypeMapConfiguration genericMap;
            ProfileMap profile;
            TypeMap cachedMap = null;
            var userMap = FindTypeMapFor(genericTypePair.Value);
            if (userMap?.DestinationTypeOverride != null)
            {
                genericTypePair = new TypePair(genericTypePair.Value.SourceType, userMap.DestinationTypeOverride).GetOpenGenericTypePair();
                if (genericTypePair == null)
                {
                    return null;
                }
                userMap = null;
            }
            if (userMap == null && (cachedMap = GetCachedMap(genericTypePair.Value)) != null)
            {
                if (!cachedMap.Types.IsGeneric)
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
                    .Select(p => new { GenericMap = p.GetGenericMap(typePair), Profile = p })
                    .FirstOrDefault(p => p.GenericMap != null);
                genericMap = item?.GenericMap;
                profile = item?.Profile;
            }
            else
            {
                genericMap = userMap.Profile.GetGenericMap(typePair);
                profile = userMap.Profile;
            }
            if (genericMap == null)
            {
                return null;
            }
            TypeMap typeMap;
            lock (this)
            {
                typeMap = profile.CreateClosedGenericTypeMap(genericMap, typePair, this);
            }
            cachedMap?.CopyInheritedMapsTo(typeMap);
            return typeMap;
        }

        IObjectMapper IGlobalConfiguration.FindMapper(in TypePair types)
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