namespace AutoMapper
{
    using System;

    public interface IMemberResolver : IValueResolver
    {
        Type MemberType { get; }
    }
}