#if MONODROID || NETFX_CORE || NET4
using AutoMapper.Impl;

namespace AutoMapper.Internal
{
    public class ProxyGeneratorFactoryOverride : IProxyGeneratorFactory
    {
        public IProxyGenerator Create()
        {
            return new ProxyGenerator();
        }
    }
}
#endif