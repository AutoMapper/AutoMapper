namespace AutoMapper.Mappers
{
	public class StringMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			return mapper.FormatValue(context);
		}

		public bool IsMatch(ResolutionContext context)
		{
			return context.DestinationType.Equals(typeof(string));
		}
	}
}