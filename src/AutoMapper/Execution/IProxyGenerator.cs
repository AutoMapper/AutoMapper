namespace AutoMapper.Execution
{
    using System;

    public interface IProxyGenerator
    {
        Type GetProxyType(Type interfaceType);
    }
}