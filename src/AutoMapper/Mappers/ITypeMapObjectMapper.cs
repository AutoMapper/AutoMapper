namespace AutoMapper.Mappers
{
    public interface ITypeMapObjectMapper
    {
        object Map(object source, ResolutionContext context);
        bool IsMatch(ResolutionContext context);
    }
}