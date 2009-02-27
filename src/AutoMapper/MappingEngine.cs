using System;
using System.Linq;

namespace AutoMapper
{
	public class MappingEngine : IMappingEngine, IMappingEngineRunner
	{
		private readonly IConfiguration _configuration;

		public MappingEngine(IConfiguration configuration)
		{
			_configuration = configuration;
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
				IObjectMapper mapperToUse = Configuration.GetMappers().FirstOrDefault(mapper => mapper.IsMatch(context));

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
