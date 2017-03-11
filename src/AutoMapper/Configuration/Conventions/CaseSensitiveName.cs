using System;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Configuration.Conventions
{
    public class CaseSensitiveName : ISourceToDestinationNameMapper
    {
        public bool MethodCaseSensitive { get; set; }

        public MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch)
        {
            return
                getTypeInfoMembers.GetMemberInfos(typeInfo)
                    .FirstOrDefault(
                        mi =>
                            typeof (ParameterInfo).IsAssignableFrom(destType) || !MethodCaseSensitive
                                ? string.Compare(mi.Name, nameToSearch, StringComparison.OrdinalIgnoreCase) == 0
                                : string.CompareOrdinal(mi.Name, nameToSearch) == 0);
        }
    }
}