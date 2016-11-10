using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Configuration.Conventions
{
    public class SourceToDestinationNameMapperAttributesMember : ISourceToDestinationNameMapper
    {
        private static readonly SourceMember[] Empty = new SourceMember[0];
        private readonly Dictionary<TypeDetails, SourceMember[]> allSourceMembers = new Dictionary<TypeDetails, SourceMember[]>();

        public MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch)
        {
            SourceMember[] sourceMembers;
            if(!allSourceMembers.TryGetValue(typeInfo, out sourceMembers))
            {
                sourceMembers = getTypeInfoMembers.GetMemberInfos(typeInfo).Select(sourceMember => new SourceMember(sourceMember)).Where(s=>s.Attribute != null).ToArray();
                allSourceMembers[typeInfo] = sourceMembers.Length == 0 ? Empty : sourceMembers;
            }
            return sourceMembers.FirstOrDefault(d => d.Attribute.IsMatch(typeInfo, d.Member, destType, destMemberType, nameToSearch)).Member;
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