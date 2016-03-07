namespace AutoMapper
{
    using System;
    using System.Linq;
    using System.Collections.Concurrent;

    public class MappingEngine : IMappingEngine
    {
        private readonly ConcurrentDictionary<TypePair, IObjectMapper> _objectMapperCache = new ConcurrentDictionary<TypePair, IObjectMapper>();
        private readonly Func<TypePair, IObjectMapper> _getObjectMapper;

        public MappingEngine(IConfigurationProvider configurationProvider, IMapper mapper)
        {
            ConfigurationProvider = configurationProvider;
            Mapper = mapper;
            _getObjectMapper = GetObjectMapper;
        }

        public IConfigurationProvider ConfigurationProvider { get; }
        public IMapper Mapper { get; }


        private IObjectMapper GetObjectMapper(TypePair types) => ConfigurationProvider.GetMappers().FirstOrDefault(mapper => mapper.IsMatch(types));

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

                IObjectMapper mapperToUse = _objectMapperCache.GetOrAdd(context.Types, _getObjectMapper);
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
