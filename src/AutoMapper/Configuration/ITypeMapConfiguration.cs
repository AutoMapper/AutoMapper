namespace AutoMapper.Configuration
{
    using System;

    public interface ITypeMapConfiguration
    {
        void Configure(IProfileConfiguration profile, TypeMap typeMap);
        MemberList MemberList { get; }
        Type SourceType { get; }
        Type DestinationType { get; }
        TypePair Types { get; }
        ITypeMapConfiguration ReverseTypeMap { get; }
    }
}