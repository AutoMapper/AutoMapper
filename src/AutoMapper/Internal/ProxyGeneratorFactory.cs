using AutoMapper.Impl;

namespace AutoMapper.Internal
{
    public class ProxyGeneratorFactory : IProxyGeneratorFactory
    {
        public IProxyGenerator Create()
        {
            return new ProxyGenerator();
        }
    }
}
