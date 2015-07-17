#if MONODROID || NET4
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