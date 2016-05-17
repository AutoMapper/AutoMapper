namespace AutoMapper.Configuration.Conventions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public interface IParentSourceToDestinationNameMapper
    {
        ICollection<ISourceToDestinationNameMapper> NamedMappers { get; }
        IGetTypeInfoMembers GetMembers { get; }
        MemberInfo GetMatchingMemberInfo(TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch);
    }
}