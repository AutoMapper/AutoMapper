namespace AutoMapper.Mappers
{
    public interface ITypeMapObjectMapper
    {
        object Map(ResolutionContext context);
        bool IsMatch(ResolutionContext context);
    }
}