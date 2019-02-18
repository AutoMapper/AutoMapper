namespace AutoMapper
{
    public interface IMappingExpressionFeature
    {
        void Configure(TypeMap typeMap);
        IMappingExpressionFeature Reverse();
    }
}
