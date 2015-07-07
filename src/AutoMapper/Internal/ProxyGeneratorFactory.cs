#if MONODROID || NET4
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