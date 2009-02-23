namespace AutoMapper.Mappers
{
	public class CustomTypeMapMapper : IObjectMapper
	{
		public object Map(ResolutionContext context, IMappingEngineRunner mapper)
		{
			return context.SourceValueTypeMap.CustomMapper(context.SourceValue);
		}

		public bool IsMatch(ResolutionContext context)
		{
			return context.SourceValueTypeMap != null && context.SourceValueTypeMap.CustomMapper != null;
		}
	}
}