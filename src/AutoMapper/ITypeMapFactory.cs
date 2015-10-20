namespace AutoMapper
{
    using System;

    public interface ITypeMapFactory
    {
        TypeDetails GetTypeInfo(Type type, IMappingOptions mappingOptions);
        TypeMap CreateTypeMap(Type sourceType, Type destinationType, IMappingOptions mappingOptions, MemberList memberList);
    }
}