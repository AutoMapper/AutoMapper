using System;

namespace AutoMapper
{
	public class MappingEngine : IMappingEngine, IMappingEngineRunner
	{
		private readonly IConfiguration _configuration;
		private readonly IObjectMapper[] _mappers;

		public MappingEngine(IConfiguration configuration, IObjectMapper[] mappers)
		{
			_configuration = configuration;
			_mappers = mappers;
		}

		public IConfiguration Configuration
		{
			get { return _configuration; }
		}

		public TDestination Map<TSource, TDestination>(TSource source)
		{
			Type modelType = typeof (TSource);
			Type destinationType = typeof (TDestination);

			return (TDestination) Map(source, modelType, destinationType);
		}

		public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
		{
			Type modelType = typeof (TSource);
			Type destinationType = typeof (TDestination);

			return (TDestination) Map(source, destination, modelType, destinationType);
		}

		public object Map(object source, Type sourceType, Type destinationType)
		{
			TypeMap typeMap = Configuration.FindTypeMapFor(sourceType, destinationType);

			var context = new ResolutionContext(typeMap, source, sourceType, destinationType);

			return ((IMappingEngineRunner) this).Map(context);
		}

		public object Map(object source, object destination, Type sourceType, Type destinationType)
		{
			TypeMap typeMap = Configuration.FindTypeMapFor(sourceType, destinationType);

			var context = new ResolutionContext(typeMap, source, destination, sourceType, destinationType);

			return ((IMappingEngineRunner) this).Map(context);
		}

		object IMappingEngineRunner.Map(ResolutionContext context)
		{
			try
			{
				IObjectMapper mapperToUse = null;

				foreach (var mapper in _mappers)
				{
					if (mapper.IsMatch(context))
					{
						mapperToUse = mapper;
						break;
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
			IValueFormatter valueFormatter = context.ContextTypeMap != null
			                                 	? Configuration.GetValueFormatter(context.ContextTypeMap.Profile)
			                                 	: Configuration.GetValueFormatter();

			return valueFormatter.FormatValue(context);
		}

		object IMappingEngineRunner.CreateObject(Type type)
		{
			return Activator.CreateInstance(type, true);
		}
	}
}
