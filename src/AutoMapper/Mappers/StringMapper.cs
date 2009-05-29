namespace AutoMapper.Mappers
{
	public class StringMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			if (context.SourceValue == null)
			{
				return mapper.FormatValue(context.CreateValueContext(null));
			}
			return mapper.FormatValue(context);
		}

		public bool IsMatch(ResolutionContext context)
		{
			return context.DestinationType.Equals(typeof(string));
		}
	}
}