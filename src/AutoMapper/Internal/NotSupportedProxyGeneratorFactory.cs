using System;

namespace AutoMapper.Internal
{
    public class ProxyGeneratorFactory : IProxyGeneratorFactory
    {
        public IProxyGenerator Create()
        {
            return new NotSupportedProxyGenerator();
        }

        public class NotSupportedProxyGenerator : IProxyGenerator
        {
            public Type GetProxyType(Type interfaceType)
            {
                throw new PlatformNotSupportedException("Proxy generation not supported on this platform.");
            }
        }
    }
}
