namespace AutoMapper.Configuration
{
    public interface ICtorParameterConfiguration
    {
        void Configure(TypeMap typeMap);
        bool CheckCtorParamName(string paramName);
    }
}