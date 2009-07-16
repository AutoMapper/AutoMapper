namespace AutoMapper.Mappers
{
    public interface ITypeMapObjectMapper
    {
        object Map(ResolutionContext context, IMappingEngineRunner mapper);
        bool IsMatch(ResolutionContext context, IMappingEngineRunner mapper);
    }
}