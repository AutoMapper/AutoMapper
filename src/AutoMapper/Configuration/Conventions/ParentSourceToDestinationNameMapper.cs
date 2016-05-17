namespace AutoMapper.Configuration.Conventions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    public class ParentSourceToDestinationNameMapper : IParentSourceToDestinationNameMapper
    {
        public IGetTypeInfoMembers GetMembers { get; } = new AllMemberInfo();

        public ICollection<ISourceToDestinationNameMapper> NamedMappers { get; } = new Collection<ISourceToDestinationNameMapper> {new DefaultName(), new SourceToDestinationNameMapperAttributesMember()};

        public MemberInfo GetMatchingMemberInfo(TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch)
        {
            MemberInfo memberInfo = null;
            foreach (var namedMapper in NamedMappers)
            {
                memberInfo = namedMapper.GetMatchingMemberInfo(GetMembers, typeInfo, destType, destMemberType, nameToSearch);
                if (memberInfo != null)
                    break;
            }
            return memberInfo;
        }
    }
}