using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.Internal;
using LinFu.DynamicProxy;

namespace AutoMapper
{
	public class MappingEngine : IMappingEngine, IMappingEngineRunner
	{
		private readonly IConfigurationProvider _configurationProvider;
		private readonly ProxyFactory _proxyFactory = new ProxyFactory();
		private readonly IObjectMapper[] _mappers;
		private readonly IDictionary<TypePair, IObjectMapper> _objectMapperCache = new Dictionary<TypePair, IObjectMapper>();

		public MappingEngine(IConfigurationProvider configurationProvider)
		{
			_configurationProvider = configurationProvider;
			_mappers = configurationProvider.GetMappers();
		}

		public IConfigurationProvider ConfigurationProvider
		{
			get { return _configurationProvider; }
		}

		public TDestination Map<TSource, TDestination>(TSource source)
		{
			Type modelType = typeof (TSource);
			Type destinationType = typeof (TDestination);

			return (TDestination) Map(source, modelType, destinationType);
		}

		public void Map<TSource, TDestination>(TSource source, TDestination destination)
		{
			Type modelType = typeof (TSource);
			Type destinationType = typeof (TDestination);

			Map(source, destination, modelType, destinationType);
		}

		public TDestination DynamicMap<TSource, TDestination>(TSource source)
		{
			Type modelType = typeof(TSource);
			Type destinationType = typeof(TDestination);

			return (TDestination)DynamicMap(source, modelType, destinationType);
		}
			
		public TDestination DynamicMap<TDestination>(object source)
		{
			Type modelType = source == null ? typeof(object) : source.GetType();
			Type destinationType = typeof(TDestination);

			return (TDestination)DynamicMap(source, modelType, destinationType);
		}

		public object DynamicMap(object source, Type sourceType, Type destinationType)
		{
			var typeMap = ConfigurationProvider.FindTypeMapFor(sourceType, destinationType);
			if (typeMap == null)
			{
				typeMap = ConfigurationProvider.CreateTypeMap(sourceType, destinationType);
				ConfigurationProvider.AssertConfigurationIsValid(typeMap);
			}

			return Map(source, sourceType, destinationType);
		}

		public object Map(object source, Type sourceType, Type destinationType)
		{
			TypeMap typeMap = ConfigurationProvider.FindTypeMapFor(sourceType, destinationType);

			var context = new ResolutionContext(typeMap, source, sourceType, destinationType);

			return ((IMappingEngineRunner) this).Map(context);
		}

		public void Map(object source, object destination, Type sourceType, Type destinationType)
		{
			TypeMap typeMap = ConfigurationProvider.FindTypeMapFor(sourceType, destinationType);

			var context = new ResolutionContext(typeMap, source, destination, sourceType, destinationType);

			((IMappingEngineRunner) this).Map(context);
		}

		object IMappingEngineRunner.Map(ResolutionContext context)
		{
			try
			{
				if (context.SourceValue == null && ShouldMapSourceValueAsNull(context))
				{
					return null;
				}

				var contextTypePair = new TypePair(context.SourceType, context.DestinationType);

				IObjectMapper mapperToUse;

                if (!_objectMapperCache.TryGetValue(contextTypePair, out mapperToUse))
                {
                    lock (_objectMapperCache)
                    {
                        if (!_objectMapperCache.TryGetValue(contextTypePair, out mapperToUse))
                        {
                            // Cache miss
                            mapperToUse = _mappers.FirstOrDefault(mapper => mapper.IsMatch(context));
                            _objectMapperCache.Add(contextTypePair, mapperToUse);
                        }
                    }
                }

			    if (mapperToUse == null)
				{
					throw new AutoMapperMappingException(context, "Missing type map configuration or unsupported mapping.");
				}

				return mapperToUse.Map(context, this);
			}
			catch (Exception ex)
			{
				throw new AutoMapperMappingException(context, ex);
			}
		}

		string IMappingEngineRunner.FormatValue(ResolutionContext context)
		{
			TypeMap contextTypeMap = context.GetContextTypeMap();
			IFormatterConfiguration configuration = contextTypeMap != null
												? ConfigurationProvider.GetProfileConfiguration(contextTypeMap.Profile)
			                                 	: ConfigurationProvider.GetProfileConfiguration(Configuration.DefaultProfileName);

			var valueFormatter = new ValueFormatter(configuration);

			return valueFormatter.FormatValue(context);
		}

		object IMappingEngineRunner.CreateObject(Type type)
		{
			return type.IsInterface
			       	? _proxyFactory.CreateProxy(type, new PropertyBehaviorInterceptor())
			       	: Activator.CreateInstance(type, true);
		}

		private bool ShouldMapSourceValueAsNull(ResolutionContext context)
		{
			var typeMap = context.GetContextTypeMap();
			if (typeMap != null)
				return ConfigurationProvider.GetProfileConfiguration(typeMap.Profile).MapNullSourceValuesAsNull;

			return ConfigurationProvider.MapNullSourceValuesAsNull;
		}
	}
}
