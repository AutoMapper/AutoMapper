using System;
using System.Collections.Generic;
using System.Reflection;

namespace AutoMapper.Configuration.Conventions
{
    // Source Destination Mapper

    public class DefaultMember : IChildMemberConfiguration
    {
        public IParentSourceToDestinationNameMapper NameMapper { get; set; }

        public bool MapDestinationPropertyToSource(ProfileMap options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, LinkedList<MemberInfo> resolvers, IMemberConfiguration parent = null)
        {
            if (string.IsNullOrEmpty(nameToSearch))
                return true;
            var matchingMemberInfo = NameMapper.GetMatchingMemberInfo(sourceType, destType, destMemberType, nameToSearch);

            if (matchingMemberInfo != null)
                resolvers.AddLast(matchingMemberInfo);
            return matchingMemberInfo != null;
        }
    }
}