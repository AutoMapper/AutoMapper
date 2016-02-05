namespace AutoMapper.Configuration.Conventions
{
    using System;
    using System.Collections.Generic;
    using Internal;

    // Source Destination Mapper

    public class DefaultMember : IChildMemberConfiguration
    {
        public IParentSourceToDestinationNameMapper NameMapper { get; set; }

        public bool MapDestinationPropertyToSource(IProfileConfiguration options, TypeDetails sourceType, Type destType, string nameToSearch, LinkedList<IValueResolver> resolvers, IMemberConfiguration parent = null)
        {
            if (string.IsNullOrEmpty(nameToSearch))
                return true;
            var matchingMemberInfo = NameMapper.GetMatchingMemberInfo(sourceType, destType, nameToSearch);

            if (matchingMemberInfo != null)
                resolvers.AddLast(matchingMemberInfo.ToMemberGetter());
            return matchingMemberInfo != null;
        }
    }
}