#if NET45
namespace AutoMapper.Execution
{
    using System;

    public interface IProxyGenerator
    {
        Type GetProxyType(Type interfaceType);
    }
}
#endif