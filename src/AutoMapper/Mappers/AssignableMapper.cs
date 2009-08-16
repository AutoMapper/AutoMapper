namespace AutoMapper.Mappers
{
	public class AssignableMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			if (context.SourceValue == null)
			{
				return mapper.CreateObject(context);
			}

			return context.SourceValue;
		}

		public bool IsMatch(ResolutionContext context)
		{
			return context.DestinationType.IsAssignableFrom(context.SourceType);
		}
	}

}