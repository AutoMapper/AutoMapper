namespace AutoMapper.Internal
{
    using System;

    public interface IProxyGenerator
    {
        Type GetProxyType(Type interfaceType);
    }
}