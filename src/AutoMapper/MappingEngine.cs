namespace AutoMapper
{
    using System;
    using System.Linq;
    using System.Collections.Concurrent;

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
                    var typeMapMapper = ConfigurationProvider.GetTypeMapMappers().First(objectMapper => objectMapper.IsMatch(context));

                    // check whether the context passes conditions before attempting to map the value (depth check)
                    object mappedObject = !context.TypeMap.ShouldAssignValue(context) ? null : typeMapMapper.Map(context.SourceValue, context);

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
    }
}
