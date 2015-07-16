using System.Reflection;

namespace AutoMapper
{
    public interface ISourceToDestinationNameMapper
    {
        IGetTypeInfoMembers GetMembers { get; set; }
        MemberInfo GetMatchingMemberInfo(TypeInfo typeInfo, string nameToSearch);
    }
}