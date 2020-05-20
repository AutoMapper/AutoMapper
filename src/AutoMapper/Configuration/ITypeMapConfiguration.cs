using System;
using System.ComponentModel;

namespace AutoMapper.Configuration
{
    [EditorBrowsable(EditorBrowsableState.Never)]
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