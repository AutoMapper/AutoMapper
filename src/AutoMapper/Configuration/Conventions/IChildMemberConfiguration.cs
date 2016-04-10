namespace AutoMapper.Configuration.Conventions
{
    using System;
    using System.Collections.Generic;

    public interface IChildMemberConfiguration
    {
        bool MapDestinationPropertyToSource(IProfileConfiguration options, TypeDetails sourceType, Type destType, string nameToSearch, LinkedList<IValueResolver> resolvers, IMemberConfiguration parent);
    }
}