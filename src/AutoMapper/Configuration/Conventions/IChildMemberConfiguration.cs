namespace AutoMapper.Configuration.Conventions
{
    using System;
    using System.Collections.Generic;

    public interface IChildMemberConfiguration
    {
        bool MapDestinationPropertyToSource(IProfileConfiguration options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, LinkedList<IMemberGetter> resolvers, IMemberConfiguration parent);
    }
}