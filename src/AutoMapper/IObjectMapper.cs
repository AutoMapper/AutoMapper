namespace AutoMapper
{
	public interface IObjectMapper
	{
		object Map(ResolutionContext context, IMappingEngineRunner mapper);
		bool IsMatch(ResolutionContext context);
	}
}