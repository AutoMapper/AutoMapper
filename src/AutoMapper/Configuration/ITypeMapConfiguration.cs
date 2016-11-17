namespace AutoMapper.Configuration
{
    using System;

    public interface ITypeMapConfiguration
    {
        void Configure(TypeMap typeMap);
        MemberList MemberList { get; }
        Type SourceType { get; }
        Type DestinationType { get; }
        bool IsOpenGeneric { get; }
        TypePair Types { get; }
        ITypeMapConfiguration ReverseTypeMap { get; }
    }
}