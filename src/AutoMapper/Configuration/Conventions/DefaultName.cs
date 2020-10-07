using System;
using System.Reflection;

namespace AutoMapper.Configuration.Conventions
{
    public sealed class DefaultName : ISourceToDestinationNameMapper
    {
        public MemberInfo GetMatchingMemberInfo(TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch) => typeInfo.GetMember(nameToSearch);
    }
}