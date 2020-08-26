using System;
using System.Reflection;

namespace AutoMapper.Configuration.Conventions
{
    public class DefaultName : CaseSensitiveName
    {
        public override MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch) =>
            typeInfo.GetMember(nameToSearch);
    }
}