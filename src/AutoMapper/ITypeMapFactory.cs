namespace AutoMapper
{
    using System;

    public interface ITypeMapFactory
    {
        TypeMap CreateTypeMap(Type sourceType, Type destinationType, IMappingOptions mappingOptions, MemberList memberList);
    }
}