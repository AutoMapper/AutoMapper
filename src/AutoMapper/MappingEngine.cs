namespace AutoMapper
{
    using System;
    using System.Linq;
    using System.Collections.Concurrent;
    using Internal;
    using Mappers;

    public class MappingEngine : IMappingEngine
    {
        private readonly ConcurrentDictionary<TypePair, IObjectMapper> _objectMapperCache = new ConcurrentDictionary<TypePair, IObjectMapper>();

        public MappingEngine(IConfigurationProvider configurationProvider, IMapper mapper)
        {
            ConfigurationProvider = configurationProvider;
            Mapper = mapper;
        }

        public IConfigurationProvider ConfigurationProvider { get; }
        public IMapper Mapper { get; }

        public object Map(ResolutionContext context)
        {
            try
            {
                if (context.TypeMap != null)
                {
                    context.TypeMap.Seal();

                    var typeMapMapper = ConfigurationProvider.GetTypeMapMappers().First(objectMapper => objectMapper.IsMatch(context));

                    // check whether the context passes conditions before attempting to map the value (depth check)
                    object mappedObject = !context.TypeMap.ShouldAssignValue(context) ? null : typeMapMapper.Map(context);

                    return mappedObject;
                }

                var contextTypePair = new TypePair(context.SourceType, context.DestinationType);

                Func<TypePair, IObjectMapper> missFunc =
                    tp => ConfigurationProvider.GetMappers().FirstOrDefault(mapper => mapper.IsMatch(contextTypePair));

                IObjectMapper mapperToUse = _objectMapperCache.GetOrAdd(contextTypePair, missFunc);
                if (mapperToUse == null)
                {
                    throw new AutoMapperMappingException(context, "Missing type map configuration or unsupported mapping.");
                }

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
        }

        public object CreateObject(ResolutionContext context)
        {
            var typeMap = context.TypeMap;
            var destinationType = context.DestinationType;

            if (typeMap != null)
                if (typeMap.DestinationCtor != null)
                    return typeMap.DestinationCtor(context);
                else if (typeMap.ConstructDestinationUsingServiceLocator)
                    return context.Options.ServiceCtor(destinationType);
                else if (typeMap.ConstructorMap != null && typeMap.ConstructorMap.CtorParams.All(p => p.CanResolve))
                    return typeMap.ConstructorMap.ResolveValue(context);

            if (context.DestinationValue != null)
                return context.DestinationValue;

            if (destinationType.IsInterface())
#if PORTABLE
                throw new PlatformNotSupportedException("Mapping to interfaces through proxies not supported.");
#else
                destinationType = new ProxyGenerator().GetProxyType(destinationType);
#endif

                return !ConfigurationProvider.AllowNullDestinationValues
                ? ObjectCreator.CreateNonNullValue(destinationType)
                : ObjectCreator.CreateObject(destinationType);
        }

        public bool ShouldMapSourceValueAsNull(ResolutionContext context)
        {
            if (context.DestinationType.IsValueType() && !context.DestinationType.IsNullableType())
                return false;

            var typeMap = context.GetContextTypeMap();
            if (typeMap != null)
				return ConfigurationProvider.GetProfileConfiguration(typeMap.Profile).AllowNullDestinationValues;

			return ConfigurationProvider.AllowNullDestinationValues;
        }

        public bool ShouldMapSourceCollectionAsNull(ResolutionContext context)
        {
            var typeMap = context.GetContextTypeMap();
            if (typeMap != null)
				return ConfigurationProvider.GetProfileConfiguration(typeMap.Profile).AllowNullCollections;

            return ConfigurationProvider.AllowNullCollections;
        }

    }
}
