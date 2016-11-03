using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Configuration.Conventions
{
    public class SourceToDestinationNameMapperAttributesMember : ISourceToDestinationNameMapper
    {
        private readonly Dictionary<TypeDetails, SourceMember[]> allSourceMembers = new Dictionary<TypeDetails, SourceMember[]>();

        public MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch)
        {
            SourceMember[] sourceMembers;
            if(!allSourceMembers.TryGetValue(typeInfo, out sourceMembers))
            {
                sourceMembers = getTypeInfoMembers.GetMemberInfos(typeInfo).Select(sourceMember => new SourceMember(sourceMember)).ToArray();
                allSourceMembers[typeInfo] = sourceMembers;
            }
            return sourceMembers.FirstOrDefault(d => d.Attribute != null && d.Attribute.IsMatch(typeInfo, d.Member, destType, destMemberType, nameToSearch)).Member;
        }

        struct SourceMember
        {
            public SourceMember(MemberInfo sourceMember)
            {
                Member = sourceMember;
                Attribute = sourceMember.GetCustomAttribute<SourceToDestinationMapperAttribute>(inherit:true);
            }

            public MemberInfo Member { get; }
            public SourceToDestinationMapperAttribute Attribute { get; }
        }
    }
}