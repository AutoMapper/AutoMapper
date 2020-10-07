using System;
using System.Reflection;

namespace AutoMapper.Configuration.Conventions
{
    public sealed class DefaultName : ISourceToDestinationNameMapper
    {
        public MemberInfo GetMatchingMemberInfo(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch) => sourceTypeDetails.GetMember(nameToSearch);
    }
}