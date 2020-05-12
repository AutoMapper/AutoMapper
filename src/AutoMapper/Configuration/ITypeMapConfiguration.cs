using System;

namespace AutoMapper.Configuration
{
    public interface ITypeMapConfiguration
    {
        void Configure(TypeMap typeMap);
        Type SourceType { get; }
        Type DestinationType { get; }
        bool IsOpenGeneric { get; }
        bool IsReverseMap { get; }
        TypePair Types { get; }
        ITypeMapConfiguration ReverseTypeMap { get; }
    }
}