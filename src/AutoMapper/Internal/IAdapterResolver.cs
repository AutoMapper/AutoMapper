using System;

namespace AutoMapper.Internal
{
    internal interface IAdapterResolver
    {
        object Resolve(Type type);
    }
}
