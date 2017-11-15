using System;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Configuration.Conventions
{
    public class SourceToDestinationNameMapperAttributesMember : ISourceToDestinationNameMapper
    {
        public MemberInfo GetMatchingMemberInfo(IGetTypeInfoMembers getTypeInfoMembers, TypeDetails typeInfo, Type destType, Type destMemberType, string nameToSearch)
        {
            var sourceMembers = getTypeInfoMembers.GetMemberInfos(typeInfo)
                .Select(sourceMember => new SourceMember(sourceMember))
                .Where(s => s.Attribute != null)
                .ToArray();

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