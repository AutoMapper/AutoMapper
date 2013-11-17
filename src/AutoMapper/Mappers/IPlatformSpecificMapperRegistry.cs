namespace AutoMapper.Mappers
{
    public interface IPlatformSpecificMapperRegistry
    {
        void Initialize();
    }

    public class PlatformSpecificMapperRegistry : IPlatformSpecificMapperRegistry
    {
        public void Initialize()
        {
            // no op
        }
    }
}