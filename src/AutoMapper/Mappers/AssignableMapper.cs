namespace AutoMapper.Mappers
{
	public class AssignableMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			return context.SourceValue;
		}

		public bool IsMatch(ResolutionContext context)
		{
			return context.DestinationType.IsAssignableFrom(context.SourceType);
		}
	}

}