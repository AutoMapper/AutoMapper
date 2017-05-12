using System.Reflection;

namespace AutoMapper.Configuration
{
    public interface IPropertyMapConfiguration
    {
        void Configure(TypeMap typeMap);
        MemberInfo DestinationMember { get; }
        IPropertyMapConfiguration Reverse();
    }
}