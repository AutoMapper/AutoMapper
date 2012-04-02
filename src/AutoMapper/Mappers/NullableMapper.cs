namespace AutoMapper.Mappers
{
	public class NullableMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			return context.SourceValue;
		}

		public bool IsMatch(ResolutionContext context)
		{
			return context.DestinationType.IsNullableType();
		}
	}

	public class NullableSourceMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			return context.SourceValue ?? mapper.CreateObject(context);
		}

		public bool IsMatch(ResolutionContext context)
		{
			return context.SourceType.IsNullableType() && ! context.DestinationType.IsNullableType();
		}
	}
}