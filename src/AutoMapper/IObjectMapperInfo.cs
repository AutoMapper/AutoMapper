namespace AutoMapper
{
    public interface IObjectMapperInfo : IObjectMapper
    {
        TypePair GetAssociatedTypes(in TypePair initialTypes);
    }
}