namespace AutoMapper.Features
{
    public interface IRuntimeFeature
    {
        void Seal(IConfigurationProvider configurationProvider);
    }
}