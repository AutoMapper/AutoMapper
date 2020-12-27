namespace AutoMapper.Internal.Mappers
{
    public interface IObjectMapperInfo : IObjectMapper
    {
        TypePair GetAssociatedTypes(in TypePair initialTypes);
    }
}