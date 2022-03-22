using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Configuration;
using AutoMapper.Features;
using AutoMapper.Internal;
using AutoMapper.Internal.Mappers;
using AutoMapper.QueryableExtensions.Impl;
namespace AutoMapper
{
    using static Expression;
    using static Execution.ExpressionBuilder;
    public interface IConfigurationProvider
    {
        /// <summary>
        /// Dry run all configured type maps and throw <see cref="AutoMapperConfigurationException"/> for each problem
        /// </summary>
        void AssertConfigurationIsValid();

        /// <summary>
        /// Create a mapper instance based on this configuration. Mapper instances are lightweight and can be created as needed.
        /// </summary>
        /// <returns>The mapper instance</returns>
        IMapper CreateMapper();

        /// <summary>
        /// Create a mapper instance with the specified service constructor to be used for resolvers and type converters.
        /// </summary>
        /// <param name="serviceCtor">Service factory to create services</param>
        /// <returns>The mapper instance</returns>
        IMapper CreateMapper(Func<Type, object> serviceCtor);

        /// <summary>
        /// Builds the execution plan used to map the source to destination.
        /// Useful to understand what exactly is happening during mapping.
        /// See <a href="https://automapper.readthedocs.io/en/latest/Understanding-your-mapping.html">the wiki</a> for details.
        /// </summary>
        /// <param name="sourceType">the runtime type of the source object</param>
        /// <param name="destinationType">the runtime type of the destination object</param>
        /// <returns>the execution plan</returns>
        LambdaExpression BuildExecutionPlan(Type sourceType, Type destinationType);

        /// <summary>
        /// Compile all underlying mapping expressions to cached delegates.
        /// Use if you want AutoMapper to compile all mappings up front instead of deferring expression compilation for each first map.
        /// </summary>
        void CompileMappings();
    }
    public class MapperConfiguration : IGlobalConfiguration
    {
        private static readonly MethodInfo MappingError = typeof(MapperConfiguration).GetMethod(nameof(GetMappingError));

        private readonly IObjectMapper[] _mappers;
        private readonly Dictionary<TypePair, TypeMap> _configuredMaps;
        private readonly Dictionary<TypePair, TypeMap> _resolvedMaps;
        private readonly LockingConcurrentDictionary<TypePair, TypeMap> _runtimeMaps;
        private readonly ProjectionBuilder _projectionBuilder;
        private readonly LockingConcurrentDictionary<MapRequest, Delegate> _executionPlans;
        private readonly ConfigurationValidator _validator;
        private readonly Features<IRuntimeFeature> _features = new();
        private readonly int _recursiveQueriesMaxDepth;
        private readonly int _maxExecutionPlanDepth;
        private readonly bool _enableNullPropagationForQueryMapping;
        private readonly Func<Type, object> _serviceCtor;
        private readonly bool _sealed;
        private readonly bool _hasOpenMaps;

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
            _projectionBuilder = new(this, configuration.ProjectionMappers.ToArray());

            _serviceCtor = configuration.ServiceCtor;
            _enableNullPropagationForQueryMapping = configuration.EnableNullPropagationForQueryMapping ?? false;
            _maxExecutionPlanDepth = configuration.MaxExecutionPlanDepth + 1;
            _recursiveQueriesMaxDepth = configuration.RecursiveQueriesMaxDepth;

            Configuration = new((IProfileConfiguration)configuration);
            int typeMapsCount = Configuration.TypeMapsCount;
            int openTypeMapsCount = Configuration.OpenTypeMapsCount;
            Profiles = new ProfileMap[configuration.Profiles.Count + 1];
            Profiles[0] = Configuration;
            int index = 1;
            foreach (var profile in configuration.Profiles)
            {
                var profileMap = new ProfileMap(profile, configuration);
                Profiles[index++] = profileMap;
                typeMapsCount += profileMap.TypeMapsCount;
                openTypeMapsCount += profileMap.OpenTypeMapsCount;
            }
            _configuredMaps = new(typeMapsCount);
            _hasOpenMaps = openTypeMapsCount > 0;
            _runtimeMaps = new(GetTypeMap, openTypeMapsCount);
            _resolvedMaps = new(2 * typeMapsCount);
            configuration.Features.Configure(this);

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
                return GenerateTypeMapExpression(mapRequest.RequestedTypes, typeMap);
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
        ProfileMap[] IGlobalConfiguration.Profiles => Profiles;
        internal ProfileMap[] Profiles { get; }
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

        TypeMap IGlobalConfiguration.ResolveAssociatedTypeMap(TypePair types)
        {
            var typeMap = ResolveTypeMap(types);
            if (typeMap != null)
            {
                return typeMap;
            }
            if (FindMapper(types) is IObjectMapperInfo objectMapperInfo)
            {
                return ResolveTypeMap(objectMapperInfo.GetAssociatedTypes(types));
            }
            return null;
        }

        private static LambdaExpression GenerateTypeMapExpression(TypePair requestedTypes, TypeMap typeMap)
        {
            typeMap.CheckProjection();
            if (requestedTypes == typeMap.Types)
            {
                return typeMap.MapExpression;
            }
            var mapDestinationType = typeMap.DestinationType;
            var requestedDestinationType = requestedTypes.DestinationType;
            var source = Parameter(requestedTypes.SourceType, "source");
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
            !expression.Type.IsValueType && runtimeType.IsValueType ? Coalesce(expression, Default(runtimeType)) : expression;
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
                var newException = Call(MappingError, ExceptionParameter, Constant(mapRequest));
                var throwExpression = Throw(newException, runtimeDestinationType);
                fullExpression = TryCatch(ToType(map, runtimeDestinationType), Catch(ExceptionParameter, throwExpression));
            }
            var profileMap = mapRequest.MemberMap?.Profile ?? Configuration;
            var nullCheckSource = NullCheckSource(profileMap, source, destination, fullExpression, mapRequest.MemberMap);
            return Lambda(nullCheckSource, source, destination, ContextParameter);
        }
        public static AutoMapperMappingException GetMappingError(Exception innerException, in MapRequest mapRequest) =>
            new("Error mapping types.", innerException, mapRequest.RuntimeTypes) { MemberMap = mapRequest.MemberMap };
        IReadOnlyCollection<TypeMap> IGlobalConfiguration.GetAllTypeMaps() => _configuredMaps.Values;
        TypeMap IGlobalConfiguration.FindTypeMapFor(Type sourceType, Type destinationType) => FindTypeMapFor(sourceType, destinationType);
        TypeMap IGlobalConfiguration.FindTypeMapFor<TSource, TDestination>() => FindTypeMapFor(typeof(TSource), typeof(TDestination));
        TypeMap IGlobalConfiguration.FindTypeMapFor(TypePair typePair) => FindTypeMapFor(typePair);
        TypeMap FindTypeMapFor(Type sourceType, Type destinationType) => FindTypeMapFor(new(sourceType, destinationType));
        TypeMap FindTypeMapFor(TypePair typePair) => _configuredMaps.GetValueOrDefault(typePair);
        TypeMap IGlobalConfiguration.ResolveTypeMap(Type sourceType, Type destinationType) => ResolveTypeMap(new(sourceType, destinationType));
        TypeMap IGlobalConfiguration.ResolveTypeMap(TypePair typePair) => ResolveTypeMap(typePair);
        TypeMap ResolveTypeMap(TypePair typePair)
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
                        typeMap.Seal(this, null);
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
                    var derivedMap = globalConfiguration.GetIncludedTypeMap(typeMap.AsPair());
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
                    _resolvedMaps.TryAdd(includedPair, derivedMap);
                }
            }
            var typeMapsPath = new HashSet<TypeMap>();
            foreach (var typeMap in _configuredMaps.Values)
            {
                typeMap.Seal(this, typeMapsPath);
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
        TypeMap IGlobalConfiguration.GetIncludedTypeMap(TypePair pair) => GetIncludedTypeMap(pair);
        TypeMap GetIncludedTypeMap(TypePair pair)
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
        private TypeMap FindClosedGenericTypeMapFor(TypePair typePair)
        {
            if (!_hasOpenMaps || !typePair.IsConstructedGenericType)
            {
                return null;
            }
            return FindClosedGenericMap(typePair);
            TypeMap FindClosedGenericMap(TypePair typePair)
            {
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
        }
        IObjectMapper IGlobalConfiguration.FindMapper(TypePair types) => FindMapper(types);
        IObjectMapper FindMapper(TypePair types)
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
    }
}