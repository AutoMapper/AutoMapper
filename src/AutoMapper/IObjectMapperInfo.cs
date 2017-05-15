namespace AutoMapper
{
    public interface IObjectMapperInfo
    {
        TypePair GetAssociatedTypes(TypePair initialTypes);
    }
}