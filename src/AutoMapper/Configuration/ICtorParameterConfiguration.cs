namespace AutoMapper.Configuration
{
    public interface ICtorParameterConfiguration
    {
        string CtorParamName { get; }
        void Configure(TypeMap typeMap);
    }
}