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
                    // check whether the context passes conditions before attempting to map the value (depth check)
                    object mappedObject = Map(context.SourceValue, context);

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

        private object Map(object source, ResolutionContext context)
        {
            if (!context.TypeMap.ShouldAssignValue(context))
            {
                return null;
            }

            if (context.TypeMap.Substitution != null)
            {
                var newSource = context.TypeMap.Substitution(context.SourceValue);

                return context.Mapper.Map(newSource, context.DestinationValue, newSource.GetType(), context.DestinationType, context);
            }

            if (context.TypeMap.CustomMapper != null)
            {
                return context.TypeMap.CustomMapper(source, context);
            }

            if (context.SourceValue == null && context.TypeMap.Profile.AllowNullDestinationValues)
            {
                return null;
            }

            if (!context.Options.DisableCache && context.DestinationValue == null &&
                context.InstanceCache.ContainsKey(context))
            {
                return context.InstanceCache[context];
            }

            var mappedObject = context.DestinationValue ?? context.Mapper.CreateObject(context);

            if (mappedObject == null)
            {
                throw new InvalidOperationException("Cannot create destination object. " + context);
            }

            if (context.SourceValue != null && !context.Options.DisableCache)
                context.InstanceCache[context] = mappedObject;

            context.TypeMap.BeforeMap(context.SourceValue, mappedObject, context);
            context.BeforeMap(mappedObject);

            foreach (PropertyMap propertyMap in context.TypeMap.GetPropertyMaps())
            {
                try
                {
                    propertyMap.MapValue(mappedObject, context);
                }
                catch (AutoMapperMappingException ex)
                {
                    ex.PropertyMap = propertyMap;

                    throw;
                }
                catch (Exception ex)
                {
                    throw new AutoMapperMappingException(context, ex) { PropertyMap = propertyMap };
                }
            }

            context.AfterMap(mappedObject);
            context.TypeMap.AfterMap(context.SourceValue, mappedObject, context);

            return mappedObject;
        }
    }
}
