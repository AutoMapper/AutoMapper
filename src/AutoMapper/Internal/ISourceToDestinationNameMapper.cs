using System;
using System.Reflection;

namespace AutoMapper
{
    public interface ISourceToDestinationNameMapper
    {
        MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeInfo typeInfo, Type destType, string nameToSearch);
    }
}