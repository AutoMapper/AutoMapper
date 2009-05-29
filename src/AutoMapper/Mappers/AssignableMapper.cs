namespace AutoMapper.Mappers
{
	public class AssignableMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			if (context.SourceValue == null)
			{
				return context.DestinationValue ?? mapper.CreateObject(context.DestinationType);
			}

			return context.SourceValue;
		}

		public bool IsMatch(ResolutionContext context)
		{
			return context.DestinationType.IsAssignableFrom(context.SourceType);
		}
	}

}