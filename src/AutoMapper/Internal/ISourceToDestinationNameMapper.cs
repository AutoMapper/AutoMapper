using System;
using System.Reflection;

namespace AutoMapper
{
    public interface ISourceToDestinationNameMapper
    {
        IGetTypeInfoMembers GetMembers { get; set; }
        MemberInfo GetMatchingMemberInfo(TypeInfo typeInfo, Type destType, string nameToSearch);
    }
}