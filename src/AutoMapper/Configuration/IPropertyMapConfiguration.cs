namespace AutoMapper.Configuration
{
    using System.Reflection;

    public interface IPropertyMapConfiguration
    {
        void Configure(TypeMap typeMap);
        MemberInfo DestinationMember { get; }
    }
}