namespace AutoMapper.Mappers
{
	public class CustomTypeMapMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			return context.TypeMap.CustomMapper(context.SourceValue);
		}

		public bool IsMatch(ResolutionContext context)
		{
			return context.TypeMap != null && context.TypeMap.CustomMapper != null;
		}
	}
}