using System.Reflection;
using System.Reflection.Emit;

namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Linq.Expressions;
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
        private readonly MapperConfigurationExpression _mapperConfigurationExpression;

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

        private void Seal(IMapperConfigurationExpression configuration)
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

            private MapperFuncs(MapRequest mapRequest, LambdaExpression typedExpression)
            {
                Typed = typedExpression.MakeDelegate();

                _untyped = new Lazy<UntypedMapperFunc>(() => (UntypedMapperFunc)Wrap(mapRequest, typedExpression).MakeDelegate());
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

            private static LambdaExpression GenerateObjectMapperExpression(MapRequest mapRequest, IObjectMapper mapperToUse, MapperConfiguration mapperConfiguration)
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
                else if (mapperToUse is IObjectMapper)
                {
                    var exprMapper = (IObjectMapper)mapperToUse;
                    var map = exprMapper.MapExpression(mapperConfiguration._typeMapRegistry, mapperConfiguration, null, ToType(source, mapRequest.RuntimeTypes.SourceType), destination, context);
                    var mapToDestination = Lambda(ToType(map, destinationType), source, destination, context);
                    fullExpression = TryCatch(mapToDestination, source, destination, context);
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

    public static class DelegateExtensions
    {
        public static Delegate MakeDelegate(this LambdaExpression lamdaExpression)
        {
#if NET45
            var assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    new AssemblyName("MyAssembly_" + Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.Run);

            var moduleBuilder = assemblyBuilder.DefineDynamicModule("Module");
            var typeBuilder = moduleBuilder.DefineType("MyType_" + Guid.NewGuid().ToString("N"),
                TypeAttributes.Public);
            var methodName = Guid.NewGuid().ToString("N");
            var methodBuilder = typeBuilder.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Static);
            

            lamdaExpression.CompileToMethod(methodBuilder);

            var resultingType = typeBuilder.CreateType();

            return Delegate.CreateDelegate(lamdaExpression.Type,resultingType.GetMethod(methodName));
#else
                return lamdaExpression.Compile();
#endif
        }
    }
}

