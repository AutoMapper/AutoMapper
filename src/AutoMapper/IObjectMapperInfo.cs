namespace AutoMapper
{
    public interface IObjectMapperInfo : IObjectMapper
    {
        TypePair GetAssociatedTypes(TypePair initialTypes);
    }
}