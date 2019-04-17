namespace AutoMapper
{
    public interface IMappingFeature
    {
        void Configure(TypeMap typeMap);
        IMappingFeature Reverse();
    }
}
