using System;
using System.Reflection;

namespace AutoMapper.Configuration.Conventions
{
    public interface ISourceToDestinationNameMapper
    {
        MemberInfo GetMatchingMemberInfo(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch);
    }
}