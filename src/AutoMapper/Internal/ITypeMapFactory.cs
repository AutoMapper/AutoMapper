using System;

namespace AutoMapper
{
    public interface ITypeMapFactory
    {
        TypeMap CreateTypeMap(Type sourceType, Type destinationType);
    }
}