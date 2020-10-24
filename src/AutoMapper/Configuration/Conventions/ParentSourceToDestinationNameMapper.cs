using System;
using System.Collections.Generic;
using System.Reflection;

namespace AutoMapper.Configuration.Conventions
{
    public class ParentSourceToDestinationNameMapper : IParentSourceToDestinationNameMapper
    {
        public List<ISourceToDestinationNameMapper> NamedMappers { get; } = new List<ISourceToDestinationNameMapper> { new DefaultName() };
        public MemberInfo GetMatchingMemberInfo(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch)
        {
            MemberInfo memberInfo = null;
            foreach (var namedMapper in NamedMappers)
            {
                memberInfo = namedMapper.GetMatchingMemberInfo(sourceTypeDetails, destType, destMemberType, nameToSearch);
                if (memberInfo != null)
                    break;
            }
            return memberInfo;
        }
    }
}