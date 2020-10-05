using AutoMapper.Internal;

namespace AutoMapper.Features
{
    public interface IGlobalFeature
    {
        void Configure(IGlobalConfiguration configurationProvider);
    }
}
