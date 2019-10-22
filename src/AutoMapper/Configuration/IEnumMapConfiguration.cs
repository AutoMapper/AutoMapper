namespace AutoMapper.Configuration
{
    public interface IEnumMapConfiguration
    {
        void Configure(TypeMap typeMap);
        IEnumMapConfiguration Reverse();
    }
}