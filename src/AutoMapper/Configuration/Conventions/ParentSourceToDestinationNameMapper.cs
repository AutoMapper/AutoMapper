using System;
using System.Collections.Generic;
using System.Reflection;

namespace AutoMapper.Configuration.Conventions
{
    public class ParentSourceToDestinationNameMapper : IParentSourceToDestinationNameMapper
    {
        private readonly List<ISourceToDestinationNameMapper> _namedMappers = new List<ISourceToDestinationNameMapper> { new DefaultName(), new SourceToDestinationNameMapperAttributesMember() };

        public IGetTypeInfoMembers GetMembers { get; } = new AllMemberInfo();

        public ICollection<ISourceToDestinationNameMapper> NamedMappers => _namedMappers;
        public MemberInfo GetMatchingMemberInfo(TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch)
        {
            MemberInfo memberInfo = null;
            foreach (var namedMapper in _namedMappers)
            {
                memberInfo = namedMapper.GetMatchingMemberInfo(GetMembers, typeInfo, destType, destMemberType, nameToSearch);
                if (memberInfo != null)
                    break;
            }
            return memberInfo;
        }
    }
}