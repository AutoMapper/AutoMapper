using AutoMapper.Internal;

namespace AutoMapper.Features
{
    public interface IRuntimeFeature
    {
        void Seal(IGlobalConfiguration configurationProvider);
    }
}